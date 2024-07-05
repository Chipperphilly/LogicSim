using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    public void LoadScene(int sceneIdx)
    {
        SceneManager.LoadScene(sceneIdx);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
