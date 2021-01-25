using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickFree : MonoBehaviour
{
    public List<Rigidbody2D> free = new List<Rigidbody2D>();
    private List<Joint2D> tipStucks;
    void OnCollisionEnter2D(Collision2D oColid){
        // Debug.Log(oColid.collider.name);
        //add a rigidbody to the list that will be scanned for when checking if sticking is allowed
        free.Add(oColid.collider.attachedRigidbody);
        //go through all the children and make sure they are already stuck to whats no "free"
        foreach(Sticky s in transform.GetChild(0).GetComponentsInChildren<Sticky>())
            for (int i = s.stucks.Count - 1; i >= 0; i--)
                if(free.Contains(s.stucks[i].connectedBody)){
                    Destroy(s.stucks[i]);
                    s.stucks.Remove(s.stucks[i]);
                }
    }
    void OnCollisionExit2D(Collision2D oColid) {
        //remove the rigidbody from the free list
        if(free.Contains(oColid.collider.attachedRigidbody))
            free.Remove(oColid.collider.attachedRigidbody);
    }
}
