using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodInfo : MonoBehaviour
{
    public Vector2 blCorner;
    public Vector2 trCorner;
    private Collider2D[] overs;
    public Collider2D[] over{get{return overs;}}
    private TongueManager tm;
    public bool isOverlapping {get{return(overs.Length > 0 || tm.isOverlapping);}}
    void Awake(){
        tm = transform.Find("Head").GetChild(0).GetComponent<TongueManager>();
    }

    // Update is called once per frame
    void Update(){
        overs = Physics2D.OverlapAreaAll(blCorner + (Vector2)transform.position, trCorner + (Vector2)transform.position, LayerMask.GetMask("Default"));
    }
}
