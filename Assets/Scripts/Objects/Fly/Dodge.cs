using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dodge : MonoBehaviour
{
    public float dashDist = 4;
    public float dashDuration = 3;
    private Vector2 movePerFrame = Vector2.zero;
    private float alreadyMoved = 0;
    private bool tFrame = false;
    private bool gFrame = false;
    private FlyManager flyMan;

    // Start is called before the first frame update
    void Start(){
        flyMan = transform.parent.GetComponent<FlyManager>();
    }

    // Update is called once per frame
    void Update(){
        if(movePerFrame != Vector2.zero){
            if(alreadyMoved >= dashDist){
                movePerFrame = Vector2.zero;
                alreadyMoved = 0;
            }
            else{
                transform.parent.Translate(movePerFrame* Time.deltaTime);
                alreadyMoved += movePerFrame.magnitude* Time.deltaTime;
            }
        }
        tFrame = false;
        gFrame = false;
    }

    void OnTriggerStay2D(Collider2D collid){
        if(!tFrame && collid.name.Contains("Tongue")){
            Vector2 curPos = transform.parent.position;
            Vector2 direction = (Vector2)(collid.transform.position) - curPos;
            Vector2 endPos = (Vector2)(collid.transform.parent.position) + (direction.normalized * dashDist);
            movePerFrame += (curPos-endPos)/dashDuration;
            alreadyMoved = 0;
            tFrame = true;
            flyMan.Dodge();
        }
        else if(!gFrame && collid.CompareTag("Ground")){
            Vector2 curPos = transform.parent.position;
            Vector2 direction = curPos - (Vector2)(collid.ClosestPoint(curPos));
            movePerFrame += (direction.normalized*dashDist)/dashDuration;
            gFrame = true;
        }
    }
}
