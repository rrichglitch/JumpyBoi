using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    
    void Start(){
        
    }
    void Update(){

    }
    void OnCollisionEnter2D(Collision2D oColid){ Debug.Log("enter"); }
    // void OnCollisionStay2D(Collision2D oColid){ Debug.Log("stay"); }
    void OnCollisionExit2D(Collision2D oColid){Debug.Log("exit");}
}
