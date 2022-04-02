using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public Timer timer;

    [SerializeField] string sceneName;

    AudioManager AM;
    private void Awake()
    {
        AM = FindObjectOfType<AudioManager>();
    }

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
        AM.PlaySFX("Win");

        SceneLoader sceneLoader = GameObject.FindGameObjectWithTag("SceneLoader").GetComponent<SceneLoader>();

        //Switches to the next level in the list
        sceneLoader.IncrementLevel();

        TelemetryLogger.Log(this, "Total seconds spent between resets", sceneLoader.GetTotalTime());
        sceneLoader.ResetTimer();
        //Loads the level
        sceneLoader.LoadNextLevel();
    }
}
