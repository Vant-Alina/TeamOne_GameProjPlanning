using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public Timer timer;

    [SerializeField] string sceneName;

    //If the trigger collides with the player, load the next level
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            TelemetryLogger.Log(this, "Time Spent before beating level", timer.CalculateTimeSpent());
            NextLevel();
            
        }
    }

    //Loads the next level
    void NextLevel()
    {
        //Switches to the next level in the list
        GameObject.FindGameObjectWithTag("SceneLoader").GetComponent<SceneLoader>().IncrementLevel();
        //Loads the level
        GameObject.FindGameObjectWithTag("SceneLoader").GetComponent<SceneLoader>().LoadNextLevel();
    }
}
