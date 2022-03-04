using UnityEngine;

/// <summary>
/// Central repository for telemetry login details and policies for how to log in this game version.
/// </summary>
[CreateAssetMenu(fileName = "TelemetrySettings.asset", menuName = "Telemetry/Settings")]
public class TelemetrySettings : ScriptableObject
{   
    /// <summary>
    /// Modes controlling when to send telemetry to the server, vs. only logging locally.
    /// </summary>
    public enum NetworkingMode
    {
        Disabled,
        RuntimeOnly,
        RuntimeAndEditor,
    }

    /// <summary>
    /// Modes controlling how much information to print to the Unity console / text log.
    /// </summary>
    public enum LoggingPolicy
    {
        All,
        Connection,
        WarningsAndErrors,
        ErrorsOnly,
        None
    }


    [Tooltip("This is used as the name of the database table to log events into.")]
    public string userName = "Your Sheridan user name or group name";

    [Tooltip("This helps avoid anyone accidentally writing into the wrong table - only the password given to you by the instructor will let you modify the data.")]
    public string secret = "Your student # or group passphrase";

    [Tooltip("Arbitrary version identifier - change this when you make a different test build, so you know which version each event came from.")]
    [SerializeField] string _version = "1.0";
    /// <summary>
    /// Get the version text, automatically appended with the platform it's running on.
    /// </summary>
    public string Version {
        get {
            return $"{_version}-{Application.platform}";
        }
    }

    [Tooltip("Address of the telemetry database server.")]
    [SerializeField] string _serverURL = "https://dd-telemetry.herokuapp.com/";

    /// <summary>
    /// Used to prod the server in case it's sleeping, so that it's awake when we try to log in.
    /// </summary>
    public string WakeUpURL { get { return $"{_serverURL}awake";} }

    /// <summary>
    /// Used to authenticate and start a new telemetry logging session.
    /// </summary>
    public string ConnectURL { get { return $"{_serverURL}connect";} }

    /// <summary>
    /// Used to log new telemetry events in the current session.
    /// </summary>
    public string LogURL { get { return $"{_serverURL}log";} }

    [Tooltip("Allows you to turn off logging to the database for testing builds.")]
    public NetworkingMode serverCommunication;

    [Tooltip("Controls how many mesages the telemetry scripts will print in the Unity console.")]
    public LoggingPolicy debugLogging;

#if UNITY_EDITOR
    /// <summary>
    /// Convenience method to quickly find a TelemetrySettings asset in the project, since there's usually only one.
    /// </summary>
    /// <returns>The first TelemetrySettings asset found in your project. If there is more than one, don't rely on this choosing the right one.</returns>
    /// <exception cref="System.IO.FileNotFoundException"></exception>
    public static TelemetrySettings GetDefault() {
        string typeName = typeof(TelemetrySettings).Name;
        string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:{typeName}");

        if (guids.Length > 1) {
            Debug.LogWarning($"{guids.Length} Telemetry Settings found - double-check that this logger is using the one you intend.");
        } else if (guids.Length == 0) {
            throw new System.IO.FileNotFoundException($"Could not find Telemetry Settings asset. Be sure to create one.");
        }

        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
        return UnityEditor.AssetDatabase.LoadAssetAtPath<TelemetrySettings>(path);        
    }
#endif

}
