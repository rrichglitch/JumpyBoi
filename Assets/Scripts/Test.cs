using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private Collider2D colid;
    public Vector2 blCorner;
    public Vector2 trCorner;
    void Start(){
        colid = GetComponent<Collider2D>();
        blCorner += (Vector2)transform.position;
        trCorner += (Vector2)transform.position;
    }
    void Update(){
        // Debug.Log(colid.IsTouchingLayers());
        Collider2D[] overs = Physics2D.OverlapAreaAll(blCorner, trCorner, LayerMask.GetMask("Frog"));
        Debug.Log(overs.Length);
        // if(overs.Length > 0) Debug.Log(overs[0]);
        Debug.DrawLine(trCorner, new Vector2(trCorner.x, blCorner.y));
        Debug.DrawLine(blCorner, new Vector2(blCorner.x, trCorner.y));
    }
}
