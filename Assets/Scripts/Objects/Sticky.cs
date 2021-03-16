using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sticky : MonoBehaviour
{
    public float force;
    public float torque;
    public float freq;

    private bool stickOn;
    private Dictionary<Collider2D, Joint2D[]> stucks = new Dictionary<Collider2D, Joint2D[]>();
    public int stucksCount {get{return stucks.Count;}}
    private StickFree sf;
    [SerializeField] private bool defVal = true;
    [SerializeField] private bool triggered = false;
    void Start(){
        stickOn = defVal;
        sf = transform.GetComponent<StickFree>();
        if(sf == null && transform.parent != null) sf = transform.parent.GetComponent<StickFree>();
    }
    public void defStick(bool set = true){ if(set) stickOn = defVal; else stickOn = set; }

    void OnTriggerEnter2D(Collider2D oCollid){ if(triggered) stick(oCollid); }
    void OnTriggerExit2D(Collider2D oCollid){ unStick(oCollid); }
    void OnCollisionEnter2D(Collision2D collis){ if(!triggered) stick(collis.collider); }
    // void OnCollisionExit2D(Collision2D collis){ if(!triggered) unStick(collis.collider); }

    public void stick(Collider2D oCollid){
        if(stickOn){
            if(sf == null || !sf.free.Contains(oCollid)){
                if(stucks.ContainsKey(oCollid)) unStick(oCollid);
                FrictionJoint2D fj = gameObject.AddComponent<FrictionJoint2D>();
                fj.enableCollision = true; //this line is needed so that On(Trigger/Collision)Stay works properly
                fj.connectedBody = oCollid.attachedRigidbody;
                fj.autoConfigureConnectedAnchor = true;
                fj.maxForce = force;
                fj.maxTorque = torque;
                SpringJoint2D sj = null;
                if(freq > 0){
                    sj = gameObject.AddComponent<SpringJoint2D>();
                    sj.enableCollision = true; //this line is needed so that On(Trigger/Collision)Stay works properly
                    sj.connectedBody = oCollid.attachedRigidbody;
                    sj.autoConfigureConnectedAnchor = true;
                    sj.dampingRatio = 1;
                    sj.frequency = freq;
                }
                stucks.Add(oCollid, new Joint2D[]{fj, sj});
            }
        }
    }

    public void unStick(Collider2D oCollid){
        // Debug.Log("unstick "+ oCollid.name);
        if(stucks.ContainsKey(oCollid)){
            Destroy(stucks[oCollid][0]);
            Destroy(stucks[oCollid][1]);
            stucks.Remove(oCollid);
        }
    }

    //a simple method to run through and clear all the joints this script has made
    public void clearSticks(){
        Collider2D[] saveKeys = new Collider2D[stucks.Count];
        stucks.Keys.CopyTo(saveKeys,0);
        foreach(Collider2D collid in saveKeys)
            unStick(collid);
    }

    // void OnTriggerStay2D(){ Debug.Log("stay"); }
    // void Update(){Debug.Log(stucks.Count);}
}