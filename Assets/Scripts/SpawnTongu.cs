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
    private bool moved;
    private float width;
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
        //bind tip of tongue and start the body
        if(tongueSegs.Count < 1){
            if(transform.Find("Tongue_Tip").localPosition.x > width*inScale){
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
        
        //create main body of tongue
        else{
            GameObject next;
            //wee helper method to set the segment second to last as "next", use it before ever using "next"
            void setNext(){
                if(tongueSegs.Count > 1) next = tongueSegs[tongueSegs.Count-2];
                else next = transform.Find("Tongue_Tip").gameObject;
            }
            Vector3 lPos = tongueSegs[tongueSegs.Count-1].transform.localPosition;
            if(lPos.x > width*outScale && tongueSegs.Count < 100){
                Vector3 spawnSpot =lPos;
                spawnSpot.x -= width*outScale;
                GameObject last = Instantiate(prefab, transform);
                last.transform.localPosition = spawnSpot;
                tongueSegs.Add(last);
                setNext();
                last.GetComponent<HingeJoint2D>().connectedBody = next.GetComponent<Rigidbody2D>();
                if(tongueSegs.Count >= 100){
                    FixedJoint2D lfj2d = last.AddComponent<FixedJoint2D>() as FixedJoint2D;
                    lfj2d.connectedBody = transform.parent.GetComponent<Rigidbody2D>();
                }
                else{
                    last.GetComponent<SliderJoint2D>().connectedBody = transform.parent.GetComponent<Rigidbody2D>();
                    next.GetComponent<SliderJoint2D>().enabled = false;
                }
            }
        }

        //destroy!!
        if(!th)
            if(tongueSegs.Count > 1){
                if(moved){
                    GameObject last;
                    GameObject next;
                    last = tongueSegs[tongueSegs.Count-1];
                    next = tongueSegs[tongueSegs.Count-2];
                    // savePos = transform.TransformPoint(new Vector2((next.transform.localPosition.x+width*(float)1.5)/2,0));
                    Vector2 spot = transform.TransformPoint(new Vector2(transform.position.x/3,0));
                    last.transform.localPosition = new Vector3(0,0,0);
                    decon(last);
                    next.GetComponent<SliderJoint2D>().enabled = true;
                    for(int i = tongueSegs.Count-2; i > tongueSegs.Count-6 && i > 0; i--)
                        tongueSegs[i].GetComponent<SliderJoint2D>().enabled = true;
                    moved = false;
                    next.GetComponent<Rigidbody2D>().MovePosition(spot);
                    next.GetComponent<SpriteRenderer>().enabled = false;
                }
            }else if(tongueSegs.Count==1){
                tongueSegs[tongueSegs.Count-1].transform.localPosition = new Vector3(0,0,0);
                decon(tongueSegs[tongueSegs.Count-1]);
                slurp();
            }
        else slurp();
    }

    void slurp(){
        Transform tip = transform.Find("Tongue_Tip");
        tip.localPosition = new Vector2(0,0);
        tip.GetComponent<SliderJoint2D>().enabled = true;
        tip.GetComponent<FixedJoint2D>().enabled = true;
    }
    void decon(GameObject del){
        Destroy(del);
        tongueSegs.Remove(del);
    }
    void FixedUpdate(){moved=true;}
}