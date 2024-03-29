﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Experimental.Rendering.Universal;

public class TongueManager : MonoBehaviour
{
    [SerializeField] private GameObject prefab; //prefab of the tongue segments
    [SerializeField] private float outScale;
    [SerializeField] private float inScale;
    [SerializeField] private float speed;
    [SerializeField] private int maxSegs;
    [SerializeField] private float pullForce;
    [SerializeField] private float lengthMod;
    [SerializeField] private float rayOffset;
    public bool someStuck {get; private set;} = false; //true when atleast one tongue segment is sticking to something
    public bool th {get; private set;} = false; //th is true when the tongue button is being held
    public bool isOverlapping = false;
    private GameObject last;
    private GameObject next;
    private float width;
    private Vector2 startPos;
    private Transform tip;
    private HingeJoint2D jawHinge;
    private bool peaked = false;
    private bool tLit = false;
    private Color tCol;
    private Color fCol;
    private Rigidbody2D holderBody;
    private DistanceJoint2D[] headDists;
    private SpringJoint2D[] headSprings;
    private float frFrames = -1;
    public List<GameObject> tongueSegs {get; private set;} = new List<GameObject>();
    private List<GameObject> tongueHid = new List<GameObject>();


    // Start is called before the first frame update
    void Start(){
        width = prefab.GetComponent<SpriteRenderer>().bounds.size.x;
        startPos = transform.localPosition;
        jawHinge = transform.parent.Find("Jaw").GetComponent<HingeJoint2D>();
        tip = transform.GetChild(0);
        tip.GetComponent<Sticky>().defStick(false);
        holderBody = transform.parent.parent.GetComponent<Rigidbody2D>();
        headDists = transform.parent.parent.GetComponents<DistanceJoint2D>();
        headSprings = transform.parent.parent.GetComponents<SpringJoint2D>();
        tCol = tip.GetComponent<Light2D>().color;
        next = tip.gameObject;
        for(int i = 0; i < maxSegs; i++){
            tongueHid.Add(Instantiate(prefab, transform));
            tongueHid[i].SetActive(false);

            if(tongueHid.Count > 1) next = tongueHid[tongueHid.Count-2];
            initConstraints(tongueHid[i], next);
        }
    }
    private void initConstraints(GameObject last, GameObject next){
        last.GetComponent<SliderJoint2D>().connectedBody = holderBody;
        last.GetComponent<HingeJoint2D>().connectedBody = next.GetComponent<Rigidbody2D>();
        // last.GetComponent<FrictionJoint2D>().connectedBody = next.GetComponent<Rigidbody2D>();
        //adjust specific things for th tip
        if(next.name == "Tongue_Tip"){
            Vector2 conAnc = last.GetComponent<HingeJoint2D>().connectedAnchor;
            conAnc.x *= 1.1F;
            last.GetComponent<HingeJoint2D>().connectedAnchor = conAnc;
            // last.GetComponent<FrictionJoint2D>().connectedAnchor = conAnc;
        }
        if(tongueHid.Count < 10){
            Sticky ls = last.GetComponent<Sticky>();
            Sticky ts = tip.GetComponent<Sticky>();
            ls.force += ts.force/(tongueHid.Count+1);
            ls.torque += ts.torque/(tongueHid.Count+1);
            ls.freq += ts.freq/(tongueHid.Count+1);
        }
    }

    public void OnTong(){OnTongue(new InputAction.CallbackContext());}
    //handles shooting the tongue and then deactivating the stickiness
    public void OnTongue(InputAction.CallbackContext ctx){
        if(Commons.useTongue){
            th = ctx.performed;
            // Debug.Log(th);
            if(th){
                //shoot the tip of the tongue
                FixedJoint2D fj = tip.GetComponent<FixedJoint2D>();
                if(fj.enabled){
                    fj.enabled = false;

                    //open the mouth
                    JointMotor2D mtr = jawHinge.motor;
                    mtr.motorSpeed = Mathf.Abs(mtr.motorSpeed);
                    jawHinge.motor = mtr;

                    // the actual change in position from velocity is velocity/50 every physics step
                    Vector2 vel = tip.InverseTransformDirection(tip.GetComponent<Rigidbody2D>().velocity);

                    // check if there is something infront of the mouth that the tongue will get stuck in and adjust the velocity accordingly
                    Vector2 rayStart = tip.localPosition;
                    rayStart.x += rayOffset;
                    rayStart = tip.TransformPoint(rayStart);
                    RaycastHit2D rCH2D= Physics2D.Raycast(rayStart, tip.TransformDirection(Vector2.right), speed*.01F);
                    if(rCH2D) vel.x += speed * (.05F + rCH2D.fraction);
                    else vel.x += speed;

                    //try to kill weird rotation of tip
                    tip.GetComponent<Rigidbody2D>().angularVelocity = 0;
                    // tip.GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(speed*.45F,0), ForceMode2D.Impulse);
                    tip.GetComponent<Rigidbody2D>().velocity = tip.TransformDirection(vel);
                    holderBody.AddForce(tip.TransformDirection(new Vector2(speed*-.1F,0)), ForceMode2D.Impulse);
                    tip.GetComponent<Sticky>().defStick();
                }
                else{
                    // Debug.Log("button with tongue out");
                    suppressStickies();
                }
            }
        }
    }

    //suppress the stickiness of the tongue
    void suppressStickies(){
        Sticky save;
        for(int i = 0; i < tongueHid.Count; i++){
            save = tongueHid[i].GetComponent<Sticky>();
            save.clearSticks();
            save.defStick(false);
        }
        save = tip.GetComponent<Sticky>();
        save.clearSticks();
        save.defStick(false);
    }

    public void ForceRetract(){
        // Debug.Log("start force");
        th = true;
        suppressStickies();
        frFrames = 60;
    }

    //to be called from fixedUpdate but could just go with update to avoid problems with decon
    void forceRetractSeg(){
        if(frFrames > 0){
            if(tongueSegs.Count > 0){
                putAwayNeat();
                if(tongueSegs.Count > 0){
                    Vector2 diff = tongueSegs[tongueSegs.Count-1].transform.position - transform.position;
                    foreach(GameObject seg in tongueSegs){
                        seg.transform.position -= (Vector3)diff;
                        // Debug.Log((Vector3)diff);
                    }
                    tip.position -= (Vector3)diff;
                }
                else slurp();
                frFrames--;
            }
            else frFrames = -1;
            // Debug.Log(frFrames+", "+tongueSegs.Count);
        }
    }

    // Update is called once per frame
    void Update(){
        // Debug.Log(th);
        //make debugging ray that points where the tip measures distance
        Vector2 rayStart = tip.localPosition;
        rayStart.x += rayOffset;
        rayStart = tip.TransformPoint(rayStart);
        Debug.DrawRay(rayStart, tip.TransformDirection(Vector2.right), Color.green);

        //check if segments are overlapping colliders in the world
        // isOverlapping = false;
        // foreach(Overlap seg in GetComponentsInChildren<Overlap>()){
        //     if(seg.CheckOverlap()){
        //         // Debug.Log("over");
        //         isOverlapping = true;
        //         break;
        //     }
        // }
        // Debug.Log(isOverlapping);

        spawn();

        //stuff to run when the tongue is out
        if(tongueSegs.Count > 0){
            //tighten leash(distance joint) from head to last seg
            if(!th || tongueSegs.Count > maxSegs-3 || transform.InverseTransformVector(tongueSegs[tongueSegs.Count-1].GetComponent<Rigidbody2D>().velocity).x < -.1) headDists[0].distance = .16F;
            
            if(!th){
                peaked = true;
                eatInMouth();
                //covers the case when there is only one segment and its not moving in
                if(tongueSegs.Count == 1){
                    putAwayNeat();
                    slurp();
                }
            }
        }
        else if(peaked) slurp();
    }
    void FixedUpdate(){
        // put methods that actually move shit here so that the movement is consistent
        spawn();
        pull();
        forceRetractSeg();
        
        //if the tip is getting far out then enable the long head spring
        // headSprings[1].enabled = (Vector2.Distance(transform.position, tip.position) > headSprings[1].distance);
    }

    //wee helper method to set the segment second to last as "next", make sure to set next BEFORE adding "last"
    private GameObject getNext(){
        if(tongueSegs.Count > 0) return tongueSegs[tongueSegs.Count-1];
        return tip.gameObject;
    }


    private void spawn(){
        //loop to build segments back from the part of the tongue out of the mouth
        next = getNext();
        Vector2 spawnSpot = next.transform.localPosition;
        for(int lc = 0; spawnSpot.x > width*outScale && tongueSegs.Count < maxSegs && lc <maxSegs; lc++){
            //the tip needs to be slid back a bit due to the sprite size
            float mod = width*outScale/spawnSpot.magnitude;
            if(next.name == "Tongue_Tip") spawnSpot = spawnSpot-(spawnSpot * width*outScale/(spawnSpot.magnitude*1.1F));
            else spawnSpot = spawnSpot-(spawnSpot * mod);
            last = tongueHid[tongueSegs.Count];
            last.transform.localPosition = spawnSpot;
            last.transform.localEulerAngles = Vector3.zero;
            last.SetActive(true);
            tongueSegs.Add(last);
            setConstraints(last, next);
            //this sets the distance joint that keeps the shape of the overall tongue relative to the head
            // headDists[0].distance = width*((2)+(tongueSegs.Count*outScale*lengthMod));

            //prepare conditions for next loop
            next = getNext();
            spawnSpot = next.transform.localPosition;
            
            //added to hopefully reduce the mean tug at the end
            if(tongueSegs.Count == maxSegs) killVelocity();
        }
    }
    void killVelocity(){
        Vector2 newVel = holderBody.velocity;
        foreach( GameObject seg in tongueSegs){
            seg.GetComponent<Rigidbody2D>().velocity = newVel;
        }
    }
    
    //set the component constraints on the parameters
    private void setConstraints(GameObject last, GameObject next){
        headDists[0].connectedBody = last.GetComponent<Rigidbody2D>();
        headSprings[0].connectedBody = last.GetComponent<Rigidbody2D>();
        last.GetComponent<SliderJoint2D>().connectedBody = holderBody;
        next.GetComponent<SliderJoint2D>().enabled = false;
        if(last.transform.localPosition.magnitude > width*outScale*1.3F){
            last.transform.rotation = Quaternion.Lerp(next.transform.rotation, transform.rotation, .5F);
            last.GetComponent<Rigidbody2D>().velocity = (next.GetComponent<Rigidbody2D>().velocity + holderBody.velocity)/2;
        }
    }

    private void pull(){
        if(tongueSegs.Count > 1 && !th){
            last = tongueSegs[tongueSegs.Count-1];
            next = tongueSegs[tongueSegs.Count-2];
            //vector math to approximate appropriate force on tongue by head
            Vector2 force = transform.position - last.transform.position;
            float mod = pullForce/force.magnitude;
            Vector2 lv = last.GetComponent<Rigidbody2D>().velocity;
            Vector2 mush = lv/force;
            float mm = mush.x+mush.y;
            // if(mm < 0) {
            //     // next.GetComponent<Rigidbody2D>().MovePosition((Vector2)transform.position-(nv*(float).03));
            //     for(int i = tongueSegs.Count-1; i >= 0; i--)
            //         tongueSegs[i].GetComponent<Rigidbody2D>().AddForce((force*mod)*((i+1)/tongueSegs.Count));
            // }

            //a few lines to try and damper orbital movement
            // Debug.Log(last.transform.InverseTransformDirection(lv));
            last.GetComponent<Rigidbody2D>().velocity = last.transform.TransformDirection(new Vector2(last.transform.InverseTransformDirection(lv).x*.6F,0));
            Vector2 nlv = next.transform.InverseTransformDirection(next.GetComponent<Rigidbody2D>().velocity);
            next.GetComponent<Rigidbody2D>().velocity = next.transform.TransformDirection(new Vector2(nlv.x*.8F,nlv.y/4));
            // Debug.Log(last.transform.InverseTransformDirection(last.GetComponent<Rigidbody2D>().velocity));

            last.GetComponent<Rigidbody2D>().AddForce(force*mod);
            someStuck = false;
            foreach(Sticky s in GetComponentsInChildren<Sticky>(true))
                if(s.stucksCount > 0){
                    someStuck = true;
                    break;
                }
            if(someStuck)
                holderBody.AddForce(-force*mod);
            else
                holderBody.AddForce(-force*mod*.15F);
        }
    }

    //destroy the segment in mouth. separate deconstruct section fixes decon happening without actually being "in mouth"
    private void eatInMouth(){
        Vector2 lPos = tongueSegs[tongueSegs.Count-1].transform.localPosition;
        if(lPos.x < width*inScale && lPos.x > -width)
            if(lPos.y < width && lPos.y > -width){
                putAwayNeat();
                // headDists[0].distance = width*((1+outScale)+(tongueSegs.Count*outScale*lengthMod));
            }
    }
    private void putAwayNeat(){
        last = tongueSegs[tongueSegs.Count-1];
        Sticky s = last.GetComponent<Sticky>();
        s.clearSticks();
        s.defStick();
        decon(last);
        headDists[0].connectedBody = getNext().GetComponent<Rigidbody2D>();
        headSprings[0].connectedBody = getNext().GetComponent<Rigidbody2D>();
    }
    private void decon(GameObject del){
        tongueSegs.Remove(del);
        del.SetActive(false);
        del.transform.position = transform.position;
        del.transform.rotation = transform.rotation;
        del.GetComponent<Rigidbody2D>().velocity = transform.parent.InverseTransformDirection(Vector2.zero);
        tip.GetComponent<Rigidbody2D>().angularVelocity = 0;
        del.GetComponent<SliderJoint2D>().enabled = true;
    }
    
    //pull the tip back into position in mouth
    private void slurp(){
        tip.position = transform.position;
        tip.rotation = transform.rotation;
        Sticky s = tip.GetComponent<Sticky>();
        s.clearSticks();
        s.defStick(false);
        tip.GetComponent<Rigidbody2D>().velocity = transform.parent.InverseTransformDirection(Vector2.zero);
        tip.GetComponent<Rigidbody2D>().angularVelocity = 0;
        tip.GetComponent<SliderJoint2D>().enabled = true;
        tip.GetComponent<FixedJoint2D>().enabled = true;
        headDists[0].distance = 3;
        // headDists[1].distance = 3;
        // headSprings[1].enabled = false;
        // tip.GetComponent<Info>().flags.Remove("ignoreBounce");
        peaked = false;
        frFrames = -1;
        tLitToggle();

        //close the mouth
        JointMotor2D mtr = jawHinge.motor;
        mtr.motorSpeed = -1*Mathf.Abs(mtr.motorSpeed);
        jawHinge.motor = mtr;
    }
    public bool mRadiate(object[] args){
        ((Effect)args[2]).doIt((GameObject)args[0]);

        fCol = (Color)((object[])args[1])[1];
        tLit = true;
        return true;
    }
    public bool unRadiate(object[] args){
        ((Effect)args[2]).unDoIt(new object[]{args[0], args[1]});
        // Debug.Log(li.name);
        
        //code to manage the special conditions for the tip
        tLit = (GetComponent<Status>().getEffect(Radiate.Name).Count > 0);
        if(tLit) fCol = (Color)((object[])args[1])[1];
        else{
            fCol = tCol;
            tLitToggle();
        }
        return true;
    }
    void tLitToggle(){
        if(tLit) tip.GetComponent<Light2D>().color = fCol;
        else tip.GetComponent<Light2D>().color = tCol;
        tip.GetComponent<Light2D>().enabled = tLit;
    }
    public void reflec(object[] args){Debug.Log("got method");}
}