using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] string sceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            NextLevel();
        }
    }

    void NextLevel()
    {
        GameObject.FindGameObjectWithTag("SceneLoader").GetComponent<SceneLoader>().IncrementLevel();
        GameObject.FindGameObjectWithTag("SceneLoader").GetComponent<SceneLoader>().LoadNextLevel();
    }
}
