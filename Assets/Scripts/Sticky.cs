using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sticky : MonoBehaviour
{
    public float breakForce;
    public bool stickOn = true;
    // public float timeOff;
    // private float gotOff;
    private List<Joint2D> stucks = new List<Joint2D>();
    // Start is called before the first frame update
    // void Start()
    // {
    //     FJ2D.connectedBody = test;
    //     FJ2D.enabled = true;
    // }
    void Start(){stickOn = true;}
    void OnTriggerEnter2D(Collider2D oColid){
        if(stickOn){
            FixedJoint2D nj;
            if(name =="Tip_Pic")
                nj = transform.parent.gameObject.AddComponent<FixedJoint2D>() as FixedJoint2D;
            else
                nj = gameObject.AddComponent<FixedJoint2D>() as FixedJoint2D;
            nj.connectedBody = oColid.attachedRigidbody;
            nj.breakForce = breakForce;
            nj.breakTorque = breakForce;
            stucks.Add(nj);
        }
    }
    void OnJointBreak2D(Joint2D broke){
        int ind = stucks.IndexOf(broke);
        if(ind != -1)
            stucks.RemoveAt(ind);
    }
}