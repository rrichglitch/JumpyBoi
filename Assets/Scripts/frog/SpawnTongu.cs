using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnTongu : MonoBehaviour
{
    //prefab of the tongue segments
    public GameObject prefab;
    public float outScale;
    public float inScale;
    public float speed;
    public int max;
    public float tongueForce;
    //th is true when the tongue button is being held
    private bool th = false;
    private bool physd;
    private float width;
    private Vector2 startPos;
    //retractGap is the amount of time between pulling segments of tongue back into the mouth, while the tongue button is not being held
    public float retractGap;
    public float tlMod;
    private float lastRetract = 0;
    private Transform tip;
    private DistanceJoint2D[] headDists;
    private bool peaked = false;
    public bool someStuck = false;
    public List<GameObject> tongueSegs = new List<GameObject>();


    // Start is called before the first frame update
    void Start(){
        width = prefab.GetComponent<SpriteRenderer>().bounds.size.x;
        startPos = transform.localPosition;
        tip = transform.Find("Tongue_Tip");
        headDists = transform.parent.GetComponents<DistanceJoint2D>();
    }

    //handles shooting the tongue and then deactivating the stickiness
    void OnTongue(InputValue value){
        th=value.isPressed;
        if(value.isPressed){
            //shoot the tip of the tongue
            if(tip.GetComponent<FixedJoint2D>().enabled){
                tip.GetComponent<FixedJoint2D>().enabled = false;
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
    IEnumerator reStick(){
        yield return new WaitForSeconds(3);
        foreach(Sticky s in GetComponentsInChildren<Sticky>())
            s.defStick();
    }

    // Update is called once per frame
    void Update(){
        GameObject last;
        GameObject next;
        Vector2 lPos;


        //loop to build segments back from the part of the tongue out of the mouth
        next = getNext();
        Vector2 spawnSpot = next.transform.localPosition;
        for(int lc = 0; spawnSpot.x > width*outScale && tongueSegs.Count < max && lc <max; lc++){
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
            headDists[0].distance = (width*(1+outScale))+(tongueSegs.Count*width*outScale*tlMod);

            //prepare conditions for next loop
            next = getNext();
            spawnSpot = next.transform.localPosition;
        }


        //stuff to run when the tongue is out
        if(tongueSegs.Count > 0){
            last = tongueSegs[tongueSegs.Count-1];
            if(!th || tongueSegs.Count > max-3 || transform.InverseTransformDirection(last.GetComponent<Rigidbody2D>().velocity).x < -.1) headDists[1].distance = .12F;
            
            if(!th){
                //retract tongue
                if(physd){
                    peaked = true;
                    if(Time.time - lastRetract >= retractGap){
                        //move the second to last segment into the mouth. should actually pull
                        if(tongueSegs.Count > 1){
                            next = tongueSegs[tongueSegs.Count-2];
                            //vector math to approximate appropriate force on tongue by head
                            Vector2 force = transform.position - next.transform.position;
                            float mod = tongueForce/force.magnitude;
                            Vector2 nv = next.GetComponent<Rigidbody2D>().velocity;
                            Vector2 mush = nv/force;
                            float mm = mush.x+mush.y;
                            // if(mm < 0) {
                            //     // next.GetComponent<Rigidbody2D>().MovePosition((Vector2)transform.position-(nv*(float).03));
                            //     for(int i = tongueSegs.Count-1; i >= 0; i--)
                            //         tongueSegs[i].GetComponent<Rigidbody2D>().AddForce((force*mod)*((i+1)/tongueSegs.Count));
                            // }
                            // next.GetComponent<Rigidbody2D>().MovePosition(transform.position);
                            next.GetComponent<Rigidbody2D>().AddForce(force*mod);
                            someStuck = false;
                            foreach(Sticky s in GetComponentsInChildren<Sticky>())
                                if(s.stucks.Count > 0){
                                    someStuck = true;
                                    break;
                                }
                            if(!someStuck)
                                transform.parent.GetComponent<Rigidbody2D>().AddForce(-force*mod*.1F);
                            else
                                transform.parent.GetComponent<Rigidbody2D>().AddForce(-force*mod*2);
                            lastRetract = Time.time;
                        }
                        //covers the case when there is only one segment and its not moving in
                        else if(peaked){
                            decon(last);
                            headDists[1].connectedBody = getNext().GetComponent<Rigidbody2D>();
                            slurp();
                        }
                    }
                    lPos = last.transform.localPosition;
                    //destroy the segment in mouth. separate deconstruct section fixes decon happening without actually being "in mouth"
                    if(peaked)
                        if(lPos.x < width*inScale && lPos.x > -width)
                            if(lPos.y < width && lPos.y > -width){
                                decon(last);
                                // getNext().GetComponent<SliderJoint2D>().enabled = true;
                                headDists[0].distance = (width*(1+outScale))+(tongueSegs.Count*width*outScale*tlMod);
                                headDists[1].connectedBody = getNext().GetComponent<Rigidbody2D>();
                            }
                    // else force++;
                }
            }
        }
        else if(peaked) slurp();
        physd = false;
        // Debug.Log(tongueSegs.Count);
    }

    //wee helper method to set the segment second to last as "next", make sure to set next BEFORE adding "last"
    GameObject getNext(){
        if(tongueSegs.Count > 0) return tongueSegs[tongueSegs.Count-1];
        return tip.gameObject;
    }

    //set the component constraints on the parameters
    void setConstraints(GameObject last, GameObject next){
        headDists[1].connectedBody = last.GetComponent<Rigidbody2D>();
        last.GetComponent<SliderJoint2D>().connectedBody = transform.parent.GetComponent<Rigidbody2D>();
        last.GetComponent<HingeJoint2D>().connectedBody = next.GetComponent<Rigidbody2D>();
        last.GetComponent<DistanceJoint2D>().connectedBody = next.GetComponent<Rigidbody2D>();
        next.GetComponent<SliderJoint2D>().enabled = false;
        if(next.name == "Tongue_Tip") last.GetComponent<DistanceJoint2D>().autoConfigureConnectedAnchor = true;
        if(tongueSegs.Count < 10) last.GetComponent<Sticky>().breakForce += tip.GetComponent<Sticky>().breakForce/(tongueSegs.Count+1);
    }

    //pull the tip back into position in mouth
    void slurp(){
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
    void decon(GameObject del){
        tongueSegs.Remove(del);
        Destroy(del);
    }
    void FixedUpdate(){physd=true;}
}