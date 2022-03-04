using UnityEngine;

public class TelemetryZone : MonoBehaviour
{
    public string sectionName;

    private void OnTriggerEnter(Collider other) {
        TelemetryLogger.GetLogger(this).ChangeSection(sectionName);
    }
}
