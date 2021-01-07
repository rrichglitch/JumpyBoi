using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sticky : MonoBehaviour
{
    public float breakForce;

    private bool stickOn;
    public List<Joint2D> stucks = new List<Joint2D>();
    public List<Rigidbody2D> free;
    [SerializeField] private const bool defVal = true;
    void Start(){
        // if(free == null && tongue) free = transform.parent.parent.GetComponent<StickFree>().free;
        stickOn = defVal;
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
    public bool defStick(bool set = defVal){stickOn = set; return set;}
}