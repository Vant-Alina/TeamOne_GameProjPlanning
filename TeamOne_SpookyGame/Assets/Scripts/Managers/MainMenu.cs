using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    AudioManager AM;

    private void Awake()
    {

        AM = FindObjectOfType<AudioManager>();
    }

    // Start is called before the first frame update
    public void startGame()
    {
        AM.PlayMusic("BGSong");
        //Loads the first level
        GameObject.FindGameObjectWithTag("SceneLoader").GetComponent<SceneLoader>().ResetTimer();
        GameObject.FindGameObjectWithTag("SceneLoader").GetComponent<SceneLoader>().LoadNextLevel();
    }
}
