using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnTongu : MonoBehaviour
{
    public GameObject prefab;
    public float outScale;
    public float inScale;
    private bool th = false;
    private bool physd;
    private float width;
    private Vector2 startPos;
    public float retractGap;
    private float lastRetract = 0;
    private List<GameObject> tongueSegs = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        width = prefab.GetComponent<SpriteRenderer>().bounds.size.x;
    }
    void OnTongue(InputValue value){th=value.isPressed;}

    // Update is called once per frame
    void Update()
    {
        if(th){
            //bind tip of tongue and start the body
            if(tongueSegs.Count < 1){
                if(transform.Find("Tongue_Tip").localPosition.magnitude > width*inScale){
                    // Debug.Log("in");
                    Vector3 spawnSpot = transform.Find("Tongue_Tip").localPosition;
                    spawnSpot.x -= width*inScale;
                    GameObject first = Instantiate(prefab, transform);
                    first.transform.localPosition = spawnSpot;
                    transform.Find("Tongue_Tip").GetComponent<SliderJoint2D>().enabled = false;
                    first.GetComponent<SliderJoint2D>().connectedBody = transform.parent.GetComponent<Rigidbody2D>();
                    first.GetComponent<HingeJoint2D>().connectedBody = transform.Find("Tongue_Tip").GetComponent<Rigidbody2D>();
                    tongueSegs.Add(first);
                }
            }
            
            //handle main body of tongue
            /*new idea:
            going beyond a simple if, spawn a number of segments appropriate for the current gap.
            while doing this there still needs to be some kind of implementation for presering orientation...'
            to acheive this maybe, simply spawn at the postion of the last to begin with but then give the move command to the last segment back to mouth*/
            else{
                GameObject next;
                //wee helper method to set the segment second to last as "next", use it before ever using "next"
                void setNext(){
                    if(tongueSegs.Count > 1) next = tongueSegs[tongueSegs.Count-2];
                    else next = transform.Find("Tongue_Tip").gameObject;
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
                    startPos = spawnSpot;
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


        else{
            if(physd){
                if(tongueSegs.Count > 1){
                    GameObject last = tongueSegs[tongueSegs.Count-1];
                    Vector2 lPos = last.transform.localPosition;
                    if(Time.time - lastRetract >= retractGap){
                        GameObject next = tongueSegs[tongueSegs.Count-2];
                        Vector2 spot = transform.position;
                        // for(int i = tongueSegs.Count-2; i > tongueSegs.Count-4 && i > 0; i--)
                        //     tongueSegs[i].GetComponent<SliderJoint2D>().enabled = true;
                        //find alternative method that actually exerts force from the head upon the tongue
                        // simply calculate proper force vector and then use addForce on the tongue and add the inverse to the head
                        next.GetComponent<Rigidbody2D>().MovePosition(spot);
                        lastRetract = Time.time;
                    }
                    Debug.Log("should pull");
                    //separate deconstruct section fixes decon happening prematurely but halts tongue after the shoot button is pressed again?
                    if(lPos.x < width*-inScale && lPos.x > width*-outScale)
                        if(lPos.y < width*outScale && lPos.y > width*-outScale) decon(last);
                }
                else if(tongueSegs.Count==1){
                    GameObject last = tongueSegs[tongueSegs.Count-1];
                    Vector2 lPos = last.transform.localPosition;
                    if(Time.time - lastRetract >= retractGap){
                        last.transform.position = transform.position;
                        lastRetract = Time.time;
                    }
                    if(lPos.x < width*-inScale && lPos.x > width*-outScale)
                        if(lPos.y < width*outScale && lPos.y > width*-outScale){ decon(last); slurp();}
                }
                physd = false;
            }
        }
    }

    void slurp(){
        Transform tip = transform.Find("Tongue_Tip");
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