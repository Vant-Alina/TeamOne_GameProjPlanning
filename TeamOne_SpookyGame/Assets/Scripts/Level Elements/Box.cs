using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    Rigidbody rb;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.velocity.x != 0 && rb.velocity.z !=0)
        {
            if (Mathf.Abs(rb.velocity.x) > Mathf.Abs(rb.velocity.z))
            {
                rb.velocity = new Vector3(rb.velocity.magnitude * Mathf.Sign(rb.velocity.x), 0, 0);
            } else
            {
                rb.velocity = new Vector3(0, 0, rb.velocity.magnitude * Mathf.Sign(rb.velocity.z));
            }
        }
    }
}
