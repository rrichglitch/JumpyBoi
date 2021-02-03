using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NStick : MonoBehaviour
{
    public float strength;

    private bool stickOn;
    public Dictionary<Collider2D, Joint2D> stucks = new Dictionary<Collider2D, Joint2D>();
    public List<Rigidbody2D> free;
    [SerializeField] private bool defVal = true;
    [SerializeField] private bool triggered = false;
    void Start(){
        // if(free == null && tongue) free = transform.parent.parent.GetComponent<StickFree>().free;
        stickOn = defVal;
    }
    public void defStick(bool set = true){ if(set) stickOn = defVal; else stickOn = set; }

    void OnTriggerEnter2D(Collider2D oCollid){ if(triggered) stick(oCollid); }
    void OnTriggerExit2D(Collider2D oCollid){ if(triggered) unStick(oCollid); }
    void OnCollisionEnter2D(Collision2D collis){ if(!triggered) stick(collis.collider); }
    void OnCollisionExit2D(Collision2D collis){ if(!triggered) unStick(collis.collider); }

    void stick(Collider2D oCollid){
        if(stickOn){
            if(!free.Contains(oCollid.attachedRigidbody)){
                FrictionJoint2D nj = gameObject.AddComponent<FrictionJoint2D>();
                nj.enableCollision = true; //this line is needed so that On(Trigger/Collision)Stay works properly
                nj.connectedBody = oCollid.attachedRigidbody;
                nj.maxForce = strength;
                nj.maxTorque = strength/2;
                stucks.Add(oCollid, nj);
            }
        }
    }

    void unStick(Collider2D oCollid){
        // Debug.Log("unstick "+ oCollid.name);
        if(stucks.ContainsKey(oCollid)){
            Destroy(stucks[oCollid]);
            stucks.Remove(oCollid);
        }
    }

    //a simple method to run through and clear all the joints this script has made
    public void clearSticks(){ stucks.Clear(); }

    // void OnTriggerStay2D(){ Debug.Log("stay"); }
    void Update(){Debug.Log(stucks.Count);}
}
