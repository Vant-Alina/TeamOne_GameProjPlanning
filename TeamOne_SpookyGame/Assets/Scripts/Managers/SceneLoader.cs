using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    private static SceneLoader mySceneLoader = null;

    int currentLevel = 0;
    [SerializeField] string[] Levels;
    // Start is called before the first frame update
    void Awake()
    {
        if (mySceneLoader == null)
        {
            mySceneLoader = this;
            DontDestroyOnLoad(gameObject);

            //Initialization code goes here[/INDENT]
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadScene(Levels[currentLevel]);
    }

    public void IncrementLevel()
    {
        currentLevel++;
    }
}
