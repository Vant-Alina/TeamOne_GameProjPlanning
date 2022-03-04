using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    float timeAtStart;
    float currentTime;
    public float timeSpent;

    // Start is called before the first frame update
    void Start()
    {
        timeAtStart = Time.time;
    }

    public float CalculateTimeSpent()
    {
        currentTime = Time.time;
        return (currentTime - timeAtStart)/60;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
