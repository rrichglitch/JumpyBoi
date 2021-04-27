using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Persistent : MonoBehaviour
{
    static GameObject instance = null;
    void Awake(){
        if(instance == null){
            DontDestroyOnLoad(gameObject);
            instance = gameObject;
        }
        else{
            Destroy(gameObject);
        }
    }
}
