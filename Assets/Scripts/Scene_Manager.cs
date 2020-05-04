using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Manager : MonoBehaviour
{
    public void LoadGameScene()
    {
        SceneManager.LoadScene("Game Scene", LoadSceneMode.Single);
    }

    public void ShowCreditScene()
    {
        SceneManager.LoadScene("Credits Scene", LoadSceneMode.Additive);
    }

    public void LoadStartScene()
    {
        SceneManager.LoadScene("Start Scene", LoadSceneMode.Single);
    }

  
}
