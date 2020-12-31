using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickFree : MonoBehaviour
{
    public List<Rigidbody2D> free = new List<Rigidbody2D>();
    public List<Sticky> stickies;
    void OnTriggerEnter2D(Collider2D oColid){
        free.Add(oColid.attachedRigidbody);
        foreach(Sticky s in stickies)
            foreach(Joint2D fj in s.stucks)
                if(free.Contains(fj.connectedBody)){
                    s.stucks.Remove(fj);
                    Destroy(fj);
                }
    }
    void OnTriggerExit2D(Collider2D oColid) {
        if(free.Contains(oColid.attachedRigidbody))
            free.Remove(oColid.attachedRigidbody);
    }
}
