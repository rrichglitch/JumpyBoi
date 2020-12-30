using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnTongu : MonoBehaviour
{
    public GameObject prefab;
    public float outScale;
    public float inScale;
    public float speed;
    public float tol;
    private bool th = false;
    private bool physd;
    private float width;
    private Vector2 startPos;
    public float retractGap;
    private float lastRetract = 0;
    private Transform tip;
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
            if(tongueSegs.Count == 0){
                tip.GetComponent<FixedJoint2D>().enabled = false;
                Vector2 vel = tip.InverseTransformDirection(tip.GetComponent<Rigidbody2D>().velocity);
                tip.GetComponent<Rigidbody2D>().velocity = tip.TransformDirection(new Vector2(vel.x+speed,vel.y));
            }
            else{
                FixedJoint2D[] fjs = tip.GetComponents<FixedJoint2D>();
                for(int i = 1;i<fjs.Length;i++)
                    Destroy(fjs[i]);
                tip.GetComponentInChildren<Sticky>().stickOn = false;
                StartCoroutine(unStick());
            }
        }
    }
    IEnumerator unStick(){yield return new WaitForSeconds(1); GetComponentInChildren<Sticky>().stickOn = true;}

    // Update is called once per frame
    void Update(){
        if(th){
            //bind tip of tongue and start the body
            if(tongueSegs.Count < 1){
                if(tip.localPosition.magnitude > width*inScale){
                    // Debug.Log("in");
                    Vector3 spawnSpot = tip.localPosition;
                    spawnSpot.x -= width*inScale;
                    GameObject first = Instantiate(prefab, transform);
                    first.transform.localPosition = spawnSpot;
                    tip.GetComponent<SliderJoint2D>().enabled = false;
                    first.GetComponent<SliderJoint2D>().connectedBody = transform.parent.GetComponent<Rigidbody2D>();
                    first.GetComponent<HingeJoint2D>().connectedBody = tip.GetComponent<Rigidbody2D>();
                    tongueSegs.Add(first);
                }
            }
            
            //handle extending main body of tongue
            /*new idea:
            going beyond a simple if, spawn a number of segments appropriate for the current gap.
            while doing this there still needs to be some kind of implementation for presering orientation...'
            to acheive this maybe, simply spawn at the postion of the last to begin with but then give the move command to the last segment back to mouth*/
            else{
                GameObject next;
                //wee helper method to set the segment second to last as "next", use it before ever using "next"
                void setNext(){
                    if(tongueSegs.Count > 1) next = tongueSegs[tongueSegs.Count-2];
                    else next = tip.gameObject;
                }
                Vector2 lPos = tongueSegs[tongueSegs.Count-1].transform.localPosition;
                for(int lc = 0;lPos.magnitude > width*outScale && tongueSegs.Count < 100 && lc <100;lc++){
                    Vector3 spawnSpot =lPos;
                    if(lPos.x > lPos.y)
                        spawnSpot.x -= width*outScale;
                    else
                        spawnSpot.y -= width*outScale;
                    GameObject last = Instantiate(prefab, transform);
                    last.transform.localPosition = spawnSpot;
                    tongueSegs.Add(last);
                    setNext();
                    last.GetComponent<HingeJoint2D>().connectedBody = next.GetComponent<Rigidbody2D>();
                    if(tongueSegs.Count >= 100){
                        tongueSegs[tongueSegs.Count-1].GetComponent<FixedJoint2D>().enabled = true;
                        tongueSegs[tongueSegs.Count-1].GetComponent<FixedJoint2D>().connectedBody = transform.parent.GetComponent<Rigidbody2D>();
                    }
                    else{
                        last.GetComponent<SliderJoint2D>().connectedBody = transform.GetComponent<Rigidbody2D>();
                        if(next.GetComponent<SliderJoint2D>())
                            next.GetComponent<SliderJoint2D>().enabled = false;
                    }
                    //re adjust loop condition
                    lPos = tongueSegs[tongueSegs.Count-1].transform.localPosition;
                }
                //hold from swallowing tongue
                if(lPos.x < -inScale*width || lPos.y < -inScale*width){
                    tongueSegs[tongueSegs.Count-1].GetComponent<FixedJoint2D>().enabled = true;
                    tongueSegs[tongueSegs.Count-1].GetComponent<FixedJoint2D>().connectedBody = transform.parent.GetComponent<Rigidbody2D>();
                    tongueSegs[tongueSegs.Count-1].GetComponent<Rigidbody2D>().MovePosition(transform.position);
                }
            }
        }

        //retract tonue
        else{
            if(physd){
                if(tongueSegs.Count >= 1){
                    GameObject last = tongueSegs[tongueSegs.Count-1];
                    Vector2 lPos = last.transform.localPosition;
                    if(Time.time - lastRetract >= retractGap){
                        if(tongueSegs.Count > 1){
                            GameObject next = tongueSegs[tongueSegs.Count-2];
                            Vector2 spot = transform.position;
                            // for(int i = tongueSegs.Count-2; i > tongueSegs.Count-4 && i > 0; i--)
                            //     tongueSegs[i].GetComponent<SliderJoint2D>().enabled = true;
                            //find alternative method that actually exerts force from the head upon the tongue
                            // simply calculate proper force vector and then use addForce on the tongue and add the inverse to the head
                            next.GetComponent<Rigidbody2D>().MovePosition(spot);
                            lastRetract = Time.time;
                        }
                        else last.transform.position = transform.position;
                    }
                    //separate deconstruct section fixes decon happening without actually being "in mouth"
                    if(lPos.x < width*inScale)
                        if(lPos.y < width && lPos.y > -width){ decon(last); if(tongueSegs.Count==1) slurp();}
                }
                physd = false;
            }
        }
    }

    void slurp(){
        tip.position = transform.position;
        tip.GetComponent<SliderJoint2D>().enabled = true;
        tip.GetComponent<FixedJoint2D>().enabled = true;
    }
    void decon(GameObject del){
        Destroy(del);
        tongueSegs.Remove(del);
    }
    void FixedUpdate(){physd=true;}
}