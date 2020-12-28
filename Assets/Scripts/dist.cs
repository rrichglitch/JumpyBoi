using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dist : MonoBehaviour
{
    public Transform go1;
    private float last;
    void Update()
    {
        float ne = Vector2.Distance(go1.position,transform.position);
        if(ne != last){Debug.Log(ne);last = ne;}
    }
}
