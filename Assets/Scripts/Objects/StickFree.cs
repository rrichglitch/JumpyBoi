using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickFree : MonoBehaviour
{
    public List<Collider2D> free {get;} = new List<Collider2D>();
    void OnCollisionEnter2D(Collision2D collis){
        // Debug.Log(oColid.collider.name);
        //add a rigidbody to the list that will be scanned for when checking if sticking is allowed
        free.Add(collis.collider);
        //go through all the children and make sure they are not already stuck to whats "free"
        foreach(Sticky s in transform.GetChild(0).GetComponentsInChildren<Sticky>())
            foreach(Collider2D collid in free)
                s.unStick(collid);
    }
    void OnCollisionExit2D(Collision2D collis) {
        //remove the rigidbody from the free list
        if(free.Contains(collis.collider))
            free.Remove(collis.collider);
    }
}
