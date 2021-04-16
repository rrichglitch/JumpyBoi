using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheeseInteract : MonoBehaviour
{
    [SerializeField] private FlyManager flyMan;
    private bool run = true;

    void OnCollisionEnter2D(Collision2D collis){
        if(run &&(collis.gameObject.name.Contains("Tongue") || collis.gameObject.name == "Jaw")){
            flyMan.Cheese();
            run = false;
        }
    }
}
