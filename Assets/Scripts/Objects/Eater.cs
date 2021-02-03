using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eater : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collis){
        Info inf = collis.gameObject.GetComponent<Info>();
        if(inf != null && inf.flags.Contains("edible")) collis.gameObject.SetActive(false);
    }
}
