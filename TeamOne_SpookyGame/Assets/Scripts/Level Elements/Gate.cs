using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Material gateClosed;
    [SerializeField] Material gateOpen;

    BoxCollider myBoxCollider;
    private void Start()
    {
        myBoxCollider = gameObject.GetComponent<BoxCollider>();
    }

    public void OpenGate()
    {
        myBoxCollider.enabled = false;
        gameObject.GetComponent<MeshRenderer>().material = gateOpen;
    }

    public void CloseGate()
    {
        myBoxCollider.enabled = true;
        gameObject.GetComponent<MeshRenderer>().material = gateClosed;
    }
}
