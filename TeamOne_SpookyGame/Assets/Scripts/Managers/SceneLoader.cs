using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    private static SceneLoader mySceneLoader = null;

    //Keeps track of what level should be loaded
    int currentLevel = 0;

    float timer = 0;

    //Keeps track of all of the levels
    [SerializeField] string[] Levels;
    
    void Awake()
    {
        //If a scene loader hasn't been instantiated, do it
        if (mySceneLoader == null)
        {
            mySceneLoader = this;
            DontDestroyOnLoad(gameObject);
        }
        //If a scene loader has been isntantiated, go destroy yourself
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
    }

    //Loads the selected level
    public void LoadNextLevel()
    {
        SceneManager.LoadScene(Levels[currentLevel]);
    }


    //Increments what level is selected (1 -> 2, 3 -> 4 etc.)
    public void IncrementLevel()
    {
        currentLevel++;
        TelemetryLogger.Log(this, "Total seconds spent between resets", timer);
        timer = 0;
    }
}
