using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TongueManager : MonoBehaviour
{
    //prefab of the tongue segments
    public GameObject prefab;
    public float outScale;
    public float inScale;
    public float speed;
    public int maxSegs;
    public float pullForce;
    //th is true when the tongue button is being held
    private bool th = false;
    private bool physd;
    private float width;
    private Vector2 startPos;
    //retractGap is the amount of time between pulling segments of tongue back into the mouth, while the tongue button is not being held
    public float lengthMod;
    private float lastRetract = 0;
    private Transform tip;
    private DistanceJoint2D[] headDists;
    private bool peaked = false;
    public bool someStuck = false;
    private GameObject last;
    private GameObject next;
    private Vector2 lPos;
    public List<GameObject> tongueSegs = new List<GameObject>();


    // Start is called before the first frame update
    void Start(){
        width = prefab.GetComponent<SpriteRenderer>().bounds.size.x;
        startPos = transform.localPosition;
        tip = transform.GetChild(0);
        headDists = transform.parent.GetComponents<DistanceJoint2D>();
    }

    //handles shooting the tongue and then deactivating the stickiness
    void OnTongue(InputValue value){
        th=value.isPressed;
        if(value.isPressed){
            //shoot the tip of the tongue
            if(tip.GetComponent<FixedJoint2D>().enabled){
                tip.GetComponent<FixedJoint2D>().enabled = false;
                // tip.GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(speed,0), ForceMode2D.Impulse);
                Vector2 vel = tip.InverseTransformDirection(tip.GetComponent<Rigidbody2D>().velocity);
                tip.GetComponent<Rigidbody2D>().velocity = tip.TransformDirection(new Vector2(vel.x+speed,vel.y));
            }
            //suppress the stickiness of the tip for a second
            else{
                foreach(Sticky s in GetComponentsInChildren<Sticky>()){
                    for (int i = s.stucks.Count - 1; i >= 0; i--){
                        Destroy(s.stucks[i]);
                        s.stucks.Remove(s.stucks[i]);
                    }
                    s.defStick(false);
                }
                // StartCoroutine(reStick());
            }
        }
    }

    //turns the stickiness back on after a second
    IEnumerator waiter(float fl){
        yield return new WaitForSeconds(fl);
        // foreach(Sticky s in GetComponentsInChildren<Sticky>())
        //     s.defStick();
    }

    // Update is called once per frame
    void Update(){
        //stuff to run when the tongue is out
        if(tongueSegs.Count > 0){
            last = tongueSegs[tongueSegs.Count-1];
            //tighten leash(distance joint) from head to last seg
            if(!th || tongueSegs.Count > maxSegs-3 || transform.InverseTransformDirection(last.GetComponent<Rigidbody2D>().velocity).x < -.1) headDists[1].distance = .12F;
            
            if(!th){
                peaked = true;
                lPos = last.transform.localPosition;
                eatLast();
                last = tongueSegs[tongueSegs.Count-1];
                //covers the case when there is only one segment and its not moving in
                if(tongueSegs.Count < 2){
                    decon(last);
                    headDists[1].connectedBody = getNext().GetComponent<Rigidbody2D>();
                    slurp();
                }                        
            }
        }
        else if(peaked) slurp();
        if(physd){
            
        }
        physd = false;
    }
    void FixedUpdate(){
        physd=true;
        spawn();
        if(tongueSegs.Count > 1 && !th){
            pull();
        }
    }

    //wee helper method to set the segment second to last as "next", make sure to set next BEFORE adding "last"
    private GameObject getNext(){
        if(tongueSegs.Count > 0) return tongueSegs[tongueSegs.Count-1];
        return tip.gameObject;
    }

    //set the component constraints on the parameters
    private void setConstraints(GameObject last, GameObject next){
        headDists[1].connectedBody = last.GetComponent<Rigidbody2D>();
        last.GetComponent<SliderJoint2D>().connectedBody = transform.parent.GetComponent<Rigidbody2D>();
        last.GetComponent<HingeJoint2D>().connectedBody = next.GetComponent<Rigidbody2D>();
        last.GetComponent<DistanceJoint2D>().connectedBody = next.GetComponent<Rigidbody2D>();
        next.GetComponent<SliderJoint2D>().enabled = false;
        if(next.name == "Tongue_Tip") last.GetComponent<DistanceJoint2D>().autoConfigureConnectedAnchor = true;
        if(tongueSegs.Count < 10) last.GetComponent<Sticky>().breakForce += tip.GetComponent<Sticky>().breakForce/(tongueSegs.Count+1);
    }

    //pull the tip back into position in mouth
    private void slurp(){
        tip.position = transform.position;
        tip.rotation = transform.rotation;
        for (int i = tip.GetComponent<Sticky>().stucks.Count - 1; i >= 0; i--){
            Destroy(tip.GetComponent<Sticky>().stucks[i]);
            tip.GetComponent<Sticky>().stucks.Remove(tip.GetComponent<Sticky>().stucks[i]);
        }
        tip.GetComponent<Rigidbody2D>().velocity = transform.InverseTransformDirection(Vector2.zero);
        tip.GetComponent<SliderJoint2D>().enabled = true;
        tip.GetComponent<FixedJoint2D>().enabled = true;
        tip.GetComponent<Sticky>().defStick();
        headDists[0].distance = width*(1+outScale);
        headDists[1].distance = width*1.5F;
        peaked = false;
    }

    private void spawn(){
        //loop to build segments back from the part of the tongue out of the mouth
        next = getNext();
        Vector2 spawnSpot = next.transform.localPosition;
        for(int lc = 0; spawnSpot.x > width*outScale && tongueSegs.Count < maxSegs && lc <maxSegs; lc++){
            //the tip needs to be slid back a bit due to the sprite size
            float mod = width*outScale/spawnSpot.magnitude;
            if(next.name == "Tongue_Tip") spawnSpot = spawnSpot-(spawnSpot * mod*.8F);
            else spawnSpot = spawnSpot-(spawnSpot * mod);
            last = Instantiate(prefab, transform);
            last.transform.localPosition = spawnSpot;
            tongueSegs.Add(last);
            lPos = last.transform.localPosition;
            setConstraints(last, next);
            //this sets the distance joint that keeps the shape of the overall tongue relative to the head
            headDists[0].distance = (width*(1+outScale))+(tongueSegs.Count*width*outScale*lengthMod);

            //prepare conditions for next loop
            next = getNext();
            spawnSpot = next.transform.localPosition;
        }
    }

    private void pull(){
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
        foreach(Sticky s in GetComponentsInChildren<Sticky>())
            if(s.stucks.Count > 0){
                someStuck = true;
                break;
            }
        if(!someStuck)
            transform.parent.GetComponent<Rigidbody2D>().AddForce(-force*mod*.05F);
        else
            transform.parent.GetComponent<Rigidbody2D>().AddForce(-force*mod);
    }

    //destroy the segment in mouth. separate deconstruct section fixes decon happening without actually being "in mouth"
    private void eatLast(){
        if(lPos.x < width*inScale && lPos.x > -width)
            if(lPos.y < width && lPos.y > -width){
                decon(last);
                // getNext().GetComponent<SliderJoint2D>().enabled = true;
                headDists[0].distance = (width*(1+outScale))+(tongueSegs.Count*width*outScale*lengthMod);
                headDists[1].connectedBody = getNext().GetComponent<Rigidbody2D>();
            }
    }
    private void decon(GameObject del){
        tongueSegs.Remove(del);
        Destroy(del);
    }
}