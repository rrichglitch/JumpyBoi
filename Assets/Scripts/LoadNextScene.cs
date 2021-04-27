using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextScene : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collid){
        if(collid.CompareTag("Player")){
            if(!SceneManager.GetSceneByName("DemoScene").isLoaded)
                SceneManager.LoadSceneAsync("DemoScene", LoadSceneMode.Additive);
        }
    }
}
