using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Collider2D collid;
    public Collider2D oCollid;
    void Awake(){
        collid = GetComponent<Collider2D>();
    }
    void Update(){
        
    }

    [ContextMenu("run test")]
    void runTest(){
        // Vector2 closeToA = collid.ClosestPoint(oCollid.transform.TransformPoint(oCollid.bounds.center));
        Debug.Log(Commons.collidersIntersect(collid, oCollid));
    }

}
