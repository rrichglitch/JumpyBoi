using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTongue : MonoBehaviour
{
    public GameObject prefab;
    public float outScale;
    public float inScale;
    private float width;
    private Vector2 savePos;
    private GameObject saveNext;
    private bool slurp;
    private bool movedPos;
    private List<GameObject> tongueSegs = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        width = prefab.GetComponent<SpriteRenderer>().bounds.size.x;
    }
    // Update is called once per frame
    void Update()
    {
        // Debug.Log(nNext);
        if(slurp){
            Transform tip = transform.Find("Tongue_Tip");
            tip.localPosition = new Vector2(0,0);
            tip.GetComponent<FixedJoint2D>().enabled = true;
            slurp = false;
        }
        if(tongueSegs.Count < 1){
            // if(transform.Find("Tongue_Tip").localPosition.x < 0){
            //     transform.Find("Tongue_Tip").GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
            //     transform.Find("Tongue_Tip").localPosition = new Vector2(0,0);
            //     Debug.Log("choke");
            // }
            if(transform.Find("Tongue_Tip").localPosition.x > width*inScale){
                // Debug.Log("in");
                Vector3 spawnSpot = transform.Find("Tongue_Tip").localPosition;
                spawnSpot.x -= width*inScale;
                GameObject first = Instantiate(prefab, transform);
                first.transform.localPosition = spawnSpot;
                transform.Find("Tongue_Tip").GetComponent<SliderJoint2D>().enabled = false;
                first.GetComponent<SliderJoint2D>().connectedBody = transform.parent.GetComponent<Rigidbody2D>();
                // first.GetComponent<FixedJoint2D>().connectedBody = transform.parent.GetComponent<Rigidbody2D>();
                first.GetComponent<HingeJoint2D>().connectedBody = transform.Find("Tongue_Tip").GetComponent<Rigidbody2D>();
                tongueSegs.Add(first);
            }
        }else{
            if(!tongueSegs[tongueSegs.Count-1]) tongueSegs.RemoveAt(tongueSegs.Count-1);
            GameObject next;
            //wee helper method to set the segment second to last as "next", use it before ever using "next"
            void setNext(){
                if(tongueSegs.Count > 1) next = tongueSegs[tongueSegs.Count-2];
                else next = transform.Find("Tongue_Tip").gameObject;
            }
            void decon(){
                setNext();
                // Vector2 avgPos = new Vector2(transform.position.x + next.transform.position.x/2, transform.position.y + next.transform.position.y/2);
                saveNext = next;
                savePos = transform.TransformPoint(new Vector2((next.transform.localPosition.x+width*(float)1.5)/2,0));
                next.GetComponent<Rigidbody2D>().MovePosition(savePos);
                // next.GetComponent<Rigidbody2D>().velocity = savePos;
                next.GetComponent<SliderJoint2D>().enabled = true;
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
            }else if(lPos.x < width*-inScale && lPos.x > width*-outScale){
                if(lPos.y < width*outScale && lPos.y > width*-outScale){
                    if(movedPos){
                        decon();
                        Destroy(tongueSegs[tongueSegs.Count-1]);
                        tongueSegs.Remove(tongueSegs[tongueSegs.Count-1]);
                        if(tongueSegs.Count < 1){ decon(); slurp = true;}
                    }else{
                        setNext();
                        next.GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
                    }
                }
            }
        }
        movedPos = false;
    }
    void FixedUpdate(){
        movedPos = true;
    }
}