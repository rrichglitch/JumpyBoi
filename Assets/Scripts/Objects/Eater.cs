using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eater : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collid){
        Info inf = collid.gameObject.GetComponent<Info>();
        if(inf != null && inf.flags.Contains("edible")) collid.gameObject.SetActive(false);
    }
}
