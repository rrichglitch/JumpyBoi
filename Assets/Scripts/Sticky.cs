using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sticky : MonoBehaviour
{
    public float breakForce;
    private List<Joint2D> stucks = new List<Joint2D>();
    // Start is called before the first frame update
    // void Start()
    // {
    //     FJ2D.connectedBody = test;
    //     FJ2D.enabled = true;
    // }
    void OnTriggerStay2D(Collider2D oColid){
        // FJ2D.connectedBody = oColid.attachedRigidbody;
        // FJ2D.enabled = true;
        FixedJoint2D nj = gameObject.AddComponent<FixedJoint2D>() as FixedJoint2D;
        nj.connectedBody = oColid.attachedRigidbody;
        nj.breakForce = breakForce;
        nj.breakTorque = breakForce;
        stucks.Add(nj);
    }
    void OnJointBreak2D(Joint2D broke){
        int ind = stucks.IndexOf(broke);
        if(ind != -1){
            // Destroy(broke);
            stucks.Remove(broke);
        }
    }
}