using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sticky : MonoBehaviour
{
    public float breakForce;

    private bool stickOn;
    public List<Joint2D> stucks {get; private set;} = new List<Joint2D>();
    public List<Rigidbody2D> free;
    [SerializeField] private bool defVal = true;
    void Start(){
        // if(free == null && tongue) free = transform.parent.parent.GetComponent<StickFree>().free;
        stickOn = defVal;
    }

    //make a new fixed joint and add it to the list so it can be managed later
    void OnCollisionEnter2D(Collision2D oColid){
        if(stickOn){
            if(!free.Contains(oColid.rigidbody)){
                FixedJoint2D nj;
                nj = gameObject.AddComponent<FixedJoint2D>() as FixedJoint2D;
                nj.connectedBody = oColid.rigidbody;
                nj.breakForce = breakForce;
                nj.breakTorque = breakForce;
                stucks.Add(nj);
            }
        }
    }

    //remove a joint from the list of owned joints it breaks
    void OnJointBreak2D(Joint2D broke){
        int ind = stucks.IndexOf(broke);
        if(ind != -1)
            stucks.RemoveAt(ind);
    }

    //a simple method to run through and clear all the joints this script has made
    public void clearSticks(){
        // foreach(Joint2D j in stucks)
        //     Destroy(j);
        // stucks.Clear();
        for(int i = stucks.Count-1; i >=0; i--){
            Destroy(stucks[i]);
            stucks.RemoveAt(i);
        }
    }
    public void defStick(bool set = true){if(set) stickOn = defVal; else stickOn = set;}
}