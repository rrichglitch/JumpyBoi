using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickFree : MonoBehaviour
{
    public List<Rigidbody2D> free = new List<Rigidbody2D>();
    private List<Joint2D> tipStucks;
    void Start(){}
    void OnTriggerEnter2D(Collider2D oColid){
        free.Add(oColid.attachedRigidbody);
        foreach(Sticky s in transform.GetChild(0).GetComponentsInChildren<Sticky>())
            for (int i = s.stucks.Count - 1; i >= 0; i--)
                if(free.Contains(s.stucks[i].connectedBody)){
                    Destroy(s.stucks[i]);
                    s.stucks.Remove(s.stucks[i]);
                }
    }
    void OnTriggerExit2D(Collider2D oColid) {
        if(free.Contains(oColid.attachedRigidbody))
            free.Remove(oColid.attachedRigidbody);
    }
}
