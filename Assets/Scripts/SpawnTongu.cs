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
    public float tol;
    public int max;
    //th is true when the tongue button is being held
    private bool th = false;
    private bool physd;
    private float width;
    private Vector2 startPos;
    //retractGap is the amount of time between pulling segments of tongue back into the mouth, while the tongue button is not being held
    public float retractGap;
    private float lastRetract = 0;
    private Transform tip;
    private bool reset = true;
    private List<GameObject> tongueSegs = new List<GameObject>();

    // Start is called before the first frame update
    void Start(){
        width = prefab.GetComponent<SpriteRenderer>().bounds.size.x;
        startPos = transform.localPosition;
        tip = transform.Find("Tongue_Tip");
    }

    void OnTongue(InputValue value){
        th=value.isPressed;
        if(value.isPressed){
            //shoot the tip of the tongue
            if(tongueSegs.Count == 0){
                tip.GetComponent<FixedJoint2D>().enabled = false;
                Vector2 vel = tip.InverseTransformDirection(tip.GetComponent<Rigidbody2D>().velocity);
                tip.GetComponent<Rigidbody2D>().velocity = tip.TransformDirection(new Vector2(vel.x+speed,vel.y));
            }
            //suppress the stickiness of the tip for a second
            else{
                FixedJoint2D[] fjs = tip.GetComponents<FixedJoint2D>();
                for(int i = 1;i<fjs.Length;i++)
                    Destroy(fjs[i]);
                tip.GetComponentInChildren<Sticky>().stickOn = false;
                StartCoroutine(unStick());
            }
        }
    }

    //turns the tips stickiness back on after a second
    IEnumerator unStick(){yield return new WaitForSeconds(1); GetComponentInChildren<Sticky>().stickOn = true;}

    // Update is called once per frame
    void Update(){
        if(tongueSegs.Count == 0) reset = true;
        if(th){
            if(reset){
                GameObject next;
                //loop to build segments back from the part of the tongue out of the mouth
                next = getNext();
                Vector2 lPos = next.transform.localPosition;
                for(int lc = 0;lPos.x > width*outScale && tongueSegs.Count < max && lc <max;lc++){
                    Vector3 spawnSpot = lPos;
                    //the tip needs to be slid back a bit due to the sprite size
                    if(next.name == "Tongue_Tip") spawnSpot.x -= width*outScale*(float).8;
                    else spawnSpot.x -= width*outScale;
                    GameObject last = Instantiate(prefab, transform);
                    last.transform.localPosition = spawnSpot;
                    tongueSegs.Add(last);
                    setConstraints(last, next);
                    if(tongueSegs.Count >= max)
                        last.GetComponent<FixedJoint2D>().enabled = true;

                    //prepare conditions for next loop
                    next = getNext();
                    lPos = next.transform.localPosition;
                }
            }
            //hold from swallowing tongue or falling off
            if(tongueSegs.Count > 0){
                GameObject last = tongueSegs[tongueSegs.Count-1];
                Vector2 lPos = last.transform.localPosition;
                if(lPos.x < -inScale*width || lPos.y < -inScale*width || lPos.y > inScale*width)
                    last.GetComponent<FixedJoint2D>().enabled = true;
            }
        }

        //retract tongue
        else{
            if(physd){
                if(tongueSegs.Count > 0){
                    GameObject last = tongueSegs[tongueSegs.Count-1];
                    last.GetComponent<SliderJoint2D>().enabled = false;
                    if(Time.time - lastRetract >= retractGap){
                        //move the second to last segment into the mouth. should actually pull
                        if(tongueSegs.Count > 1){
                            GameObject next = tongueSegs[tongueSegs.Count-2];
                            //find alternative method that actually exerts force from the head upon the tongue
                            //simply calculate proper force vector and then use addForce on the tongue and add the inverse to the head
                            //orrrr compare masses and move lighter only?? orrrr move both proportionaly??
                            next.GetComponent<Rigidbody2D>().MovePosition(transform.position);
                            // next.GetComponent<SliderJoint2D>().enabled = true;
                        }
                        else last.transform.position = transform.position;//why is this necessary??? if not present the first segment just dangles around when it should be gone
                        lastRetract = Time.time;
                    }
                    Vector2 lPos = last.transform.localPosition;
                    //destroy the segment in mouth. separate deconstruct section fixes decon happening without actually being "in mouth"
                    if(lPos.x < width*inScale)
                        if(lPos.y < width && lPos.y > -width){
                            decon(last);
                            reset = false;
                        }
                }
                else slurp();
            }
        }
        physd = false;
    }

    //wee helper method to set the segment second to last as "next", make sure to set next BEFORE adding "last"
    GameObject getNext(){
        if(tongueSegs.Count > 0) return tongueSegs[tongueSegs.Count-1];
        return tip.gameObject;
    }

    //set the component constraints on the parameters
    void setConstraints(GameObject last, GameObject next){
        last.GetComponent<SliderJoint2D>().connectedBody = transform.parent.GetComponent<Rigidbody2D>();
        last.GetComponent<FixedJoint2D>().connectedBody = transform.parent.GetComponent<Rigidbody2D>();
        last.GetComponent<HingeJoint2D>().connectedBody = next.GetComponent<Rigidbody2D>();
        next.GetComponent<SliderJoint2D>().enabled = false;
    }

    //pull the tip back into position in mouth
    void slurp(){
        tip.position = transform.position;
        tip.GetComponent<SliderJoint2D>().enabled = true;
        tip.GetComponent<FixedJoint2D>().enabled = true;
        reset = true;
    }
    void decon(GameObject del){
        Destroy(del);
        tongueSegs.Remove(del);
    }
    void FixedUpdate(){physd=true;}
}