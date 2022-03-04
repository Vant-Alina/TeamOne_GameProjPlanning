using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using LoggingPolicy = TelemetrySettings.LoggingPolicy;
using NetworkingMode = TelemetrySettings.NetworkingMode;

// Tucking the Service inside the TelemetryLogger class keeps Unity from showing
// it as an option in the "Add Component" menu, reducing distractions.
public partial class TelemetryLogger : MonoBehaviour
{
    /// <summary>
    /// Networking layer to communicate with the telemetry database server.
    /// You don't need to create one of these by hand - it's automatically spun up
    /// as needed by the TelemetryLogger. It lives on a hidden GameObject
    /// with DontDestroyOnLoad, so that logging isn't interrupted and no
    /// events are dropped during scene changes.
    /// </summary>
    private class Service : MonoBehaviour
    {
        // Rate limiting constants - please do not change these.
        const int MAX_LOGS_PER_SECOND = 10;
        const float MIN_SECONDS_BETWEEN_LOGS = 1f / MAX_LOGS_PER_SECOND;
        const int RATE_WARNING_THRESHOLD = 2 * MAX_LOGS_PER_SECOND;

        // State machine tracking.
        public enum State
        {
            Uninitialized,
            Waking,
            Authenticating,
            Ready,
            Disconnected
        }

        TelemetrySettings _settings;

        bool _enableNetwork;

        Coroutine _requestInProgress;

        readonly Queue<string> _queuedRequests = new Queue<string>();

        State _state;

        float _lastMessageTime;

        int _currentWarningThreshold = RATE_WARNING_THRESHOLD;

        const string SESSION_KEY_PLACEHOLDER = "#SESSION_KEY#";
        string _sessionKey = SESSION_KEY_PLACEHOLDER;
        int _sessionIndex = -1;
        string _lastSection = null;

        static Dictionary<TelemetrySettings, Service> _serviceMap = new Dictionary<TelemetrySettings, Service>();

        bool IsNetworkIdle { get { return _requestInProgress == null; } }

        int _nextSequenceNumber = 0;
        int _queuedBeforeReadyCount = 0;

        public event System.Action<int> OnConnectionSuccess;

        public event System.Action<string> OnConnectionFail;

        public static Service Connect(TelemetryLogger logger)
        {
            if (logger.Settings == null)
            {
                Debug.LogError($"Telemetry logger {logger.name} has no Telemetry Settings assigned.");
                return null;
            }

            if (!_serviceMap.TryGetValue(logger.Settings, out var service))
            {
                service = (new GameObject("Telemetry Service")).AddComponent<Service>();
                service.Initialize(logger.Settings);
                DontDestroyOnLoad(service.gameObject);
                service.gameObject.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;

                _serviceMap.Add(logger.Settings, service);
                service.ConnectWith(logger);
            }
            else
            {
                service.RequestServiceFor(logger);
            }
            return service;
        }

        void Initialize(TelemetrySettings settings)
        {
            _settings = settings;
            _enableNetwork = _settings.serverCommunication switch
            {
                NetworkingMode.RuntimeAndEditor => true,
                NetworkingMode.RuntimeOnly => Application.isEditor == false,
                NetworkingMode.Disabled => false,
                _ => false
            };
            if (_settings.debugLogging <= LoggingPolicy.Connection)
                Debug.Log($"Loaded {settings.name}: telemetry logging {(_enableNetwork ? "enabled" : "disabled")}.");
        }

        public void RequestServiceFor(TelemetryLogger logger)
        {
            if (logger.Section != _lastSection)
            {
                var telemetry = new TelemetryEvent<string>("ChangeSection", logger.Section);
                telemetry.section = _lastSection;
                TryLog(logger, telemetry);
            }

            _lastSection = logger.Section;
            
            if (_state < State.Ready && IsNetworkIdle)
            {
                // This case should no longer be possible... but I feel safer leaving it in
                // rather than risk introducing a new bug before class. ;)
                ConnectWith(logger);
            } else if (_state == State.Ready) {
                logger.OnConnectionSuccess?.Invoke(_sessionIndex);
            }
        }

        void ConnectWith(TelemetryLogger logger)
        {
            _lastSection = logger.Section;
            _requestInProgress = StartCoroutine(WakeAndAuthenticate(logger.Section));
        }

        public void TryLog<T>(TelemetryLogger logger, TelemetryEvent<T> telemetry)
        {
            telemetry.sessionKey = _sessionKey;
            telemetry.sequence = _nextSequenceNumber++;
            _queuedRequests.Enqueue(JsonUtility.ToJson(telemetry));

            if (_state == State.Ready) {
                if (IsNetworkIdle) _requestInProgress = StartCoroutine(TransmitLogs());
            } else {
                _queuedBeforeReadyCount++;
            }
        }

        IEnumerator WakeAndAuthenticate(string section)
        {
            if (!_enableNetwork) {
                yield return new WaitForSeconds(0.1f);
                _sessionIndex = Random.Range(-100, -10);
                _sessionKey = "";
                OnConnectionSuccess?.Invoke(_sessionIndex);
                _state = State.Ready;
                _requestInProgress = null;
                yield break;
            }

            if (_state < State.Authenticating)
            {
                _state = State.Waking;

                string awakeURL = _settings.WakeUpURL;
                int attempt = 1;

                while (true)
                {
                    if (_settings.debugLogging <= LoggingPolicy.Connection)
                        Debug.Log($"Attempting to wake telemetry server, attempt {attempt++}...");

                    using (var wakeRequest = UnityWebRequest.Get(awakeURL))
                    {
                        wakeRequest.timeout = 2;
                        yield return wakeRequest.SendWebRequest();

                        if (wakeRequest.result == UnityWebRequest.Result.Success)
                            break;
                    }
                    yield return new WaitForSecondsRealtime(1f);
                }
            }

            _state = State.Authenticating;
            if (_settings.debugLogging <= LoggingPolicy.Connection)
                Debug.Log($"Telemetry server is awake. Attempting to authenticate as {_settings.userName}...");


            var login = new AuthenticationRequest
            {
                userName = _settings.userName,
                secret = _settings.secret,
                version = _settings.Version,
                section = section
            };

            using (var loginRequest = MakeJsonRequest(_settings.ConnectURL, login))
            {
                _lastMessageTime = Time.realtimeSinceStartup;
                yield return loginRequest.SendWebRequest();

                if (loginRequest.result == UnityWebRequest.Result.Success)
                {
                    if (TryParseJson(loginRequest.downloadHandler.text, out AuthenticationResponse response))
                    {
                        if (_settings.debugLogging <= LoggingPolicy.Connection)
                            Debug.Log(response.message);

                        _sessionKey = response.sessionKey;
                        _sessionIndex = response.sessionIndex;
                        _state = State.Ready;
                        
                        OnConnectionSuccess?.Invoke(response.sessionIndex);
                    }
                    else
                    {                        
                        _state = State.Disconnected;
                        OnConnectionFail?.Invoke("Failure in JSON parsing");
                    }
                }
                else
                {
                    string error = GetErrorMessage(loginRequest);
                    if (_settings.debugLogging <= LoggingPolicy.ErrorsOnly)
                        Debug.LogError($"Failed to authenticate with telemetry server: {error}");

                    _state = State.Disconnected;
                    OnConnectionFail?.Invoke(error);
                }
            }

            if (_state == State.Ready && _queuedRequests.Count > 0)
            {
                yield return TransmitLogs();
            }

            _requestInProgress = null;
        }

        IEnumerator TransmitLogs()
        {
            string logURL = _settings.LogURL;
            while (_queuedRequests.Count > 0)
            {
                if (_queuedRequests.Count > _currentWarningThreshold)
                {
                    _currentWarningThreshold *= 2;
                    if (_settings.debugLogging <= LoggingPolicy.WarningsAndErrors)
                        Debug.LogWarning($"Logging telemetry faster than the rate limit: {_queuedRequests.Count} events in queue.");
                }
                else if (_currentWarningThreshold > RATE_WARNING_THRESHOLD && _queuedRequests.Count < RATE_WARNING_THRESHOLD/2)
                {
                    _currentWarningThreshold = RATE_WARNING_THRESHOLD;
                    if (_settings.debugLogging <= LoggingPolicy.WarningsAndErrors)
                        Debug.Log("Telemetry queue draining - logging has caught up.");
                }

                string json = _queuedRequests.Dequeue();
                
                // If this message was logged before we knew our session handle, inject the correct handle into the string.
                if (_queuedBeforeReadyCount > 0) {
                    json = json.Replace(SESSION_KEY_PLACEHOLDER, _sessionKey);
                    _queuedBeforeReadyCount--;
                }

                if (_settings.debugLogging <= LoggingPolicy.All)
                    Debug.Log($"Logging event: {json}");


                _lastMessageTime = Time.realtimeSinceStartup;
                if (_enableNetwork)
                {
                    using (var request = MakeJsonRequest(logURL, json))
                    {                        
                        yield return request.SendWebRequest();

                        if (request.result != UnityWebRequest.Result.Success)
                        {
                            if (_settings.debugLogging <= LoggingPolicy.ErrorsOnly)
                                Debug.LogError($"Failed to log event: {GetErrorMessage(request)}\n{json}");
                        }
                    }
                }

                float secondsSinceLastMessage = Time.realtimeSinceStartup - _lastMessageTime;
                yield return new WaitForSecondsRealtime(MIN_SECONDS_BETWEEN_LOGS - secondsSinceLastMessage);
            }

            _requestInProgress = null;
        }

        static UnityWebRequest MakeJsonRequest<T>(string url, T data)
        {
            string json = JsonUtility.ToJson(data);

            return MakeJsonRequest(url, json);
        }

        static UnityWebRequest MakeJsonRequest(string url, string json)
        {
            var request = UnityWebRequest.Put(url, json);

            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
            request.timeout = 3;

            return request;
        }

        static string GetErrorMessage(UnityWebRequest request)
        {
            string error = null;

            if (request.downloadHandler != null)
                error = request.downloadHandler.text;

            if (string.IsNullOrWhiteSpace(error))
                error = request.error;

            return error;
        }

        bool TryParseJson<T>(string json, out T result)
        {
            try
            {
                result = JsonUtility.FromJson<T>(json);
                return true;
            }
            catch (System.Exception e)
            {
                if (_settings.debugLogging <= LoggingPolicy.ErrorsOnly)
                    Debug.LogError($"Error parsing JSON: {e.Message}\n{json}");

                result = default;
                return false;
            }
        }

        [System.Serializable]
        public struct AuthenticationRequest
        {
            public string userName;
            public string secret;
            public string version;
            public string section;
        }

        [System.Serializable]
        public struct AuthenticationResponse
        {
            public string sessionKey;
            public int sessionIndex;
            public string message;
        }
    }
}