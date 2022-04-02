using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Material gateClosed;
    [SerializeField] Material gateOpen;

    BoxCollider myBoxCollider;

    AudioManager AM;

    private void Awake()
    {
        AM = FindObjectOfType<AudioManager>();
    }
    private void Start()
    {
        myBoxCollider = gameObject.GetComponent<BoxCollider>();
    }

    public void OpenGate()
    {
        myBoxCollider.enabled = false;
        AM.PlaySFX("GateOpen");
        gameObject.GetComponent<MeshRenderer>().material = gateOpen;
    }

    public void CloseGate()
    {
        myBoxCollider.enabled = true;
        gameObject.GetComponent<MeshRenderer>().material = gateClosed;
    }
}
