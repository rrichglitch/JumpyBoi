using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sticky : MonoBehaviour
{
    public float breakForce;
    public bool stickOn = true;
    public List<Joint2D> stucks = new List<Joint2D>();
    private List<Rigidbody2D> free;
    void Start(){
        free = transform.parent.parent.GetComponent<StickFree>().free;
    }
    void OnTriggerEnter2D(Collider2D oColid){
        if(stickOn){
            if(!free.Contains(oColid.attachedRigidbody)){
                FixedJoint2D nj;
                nj = gameObject.AddComponent<FixedJoint2D>() as FixedJoint2D;
                nj.connectedBody = oColid.attachedRigidbody;
                nj.breakForce = breakForce;
                nj.breakTorque = breakForce;
                stucks.Add(nj);
            }
        }
    }
    void OnJointBreak2D(Joint2D broke){
        int ind = stucks.IndexOf(broke);
        if(ind != -1)
            stucks.RemoveAt(ind);
    }
}