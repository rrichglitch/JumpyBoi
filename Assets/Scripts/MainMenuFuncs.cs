using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenuFuncs : MonoBehaviour
{
    public void PlayGame(){
        SceneManager.LoadScene("DemoScene");
    }

    public void QuitGame(){
        Application.Quit();
    }
}
