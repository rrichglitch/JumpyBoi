using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class Bump : MonoBehaviour
{
    [SerializeField] private float unPhase = 0;
    private static float started = -1;
    private int frog;
    private int phase;
    private Transform body;
    private BodInfo bInf;
    private TongueManager tm;
    private Vector3 oldScale;

    //cache fields
    void Start(){
        frog = LayerMask.NameToLayer("Frog");
        phase = LayerMask.NameToLayer("Phase");
        body = Commons.Instance.body.transform;
        bInf = body.GetComponent<BodInfo>();
        tm = body.Find("Head").GetChild(0).GetComponent<TongueManager>();
    }

    // Update is called once per frame
    void Update(){
        if(started >= 0){
            //reset the timer if the body hasnt fallen lower yet
            if(body.transform.position.y > transform.position.y) started = Time.time;
            if(Time.time - started >= unPhase){
                // Commons.Instance.notify("now!");
                if(!bInf.isOverlapping){
                    //change layers back to frog
                    foreach (SpriteRenderer sr in body.GetComponentsInChildren<SpriteRenderer>(true)){
                        sr.transform.gameObject.layer = frog;
                        // sr.sortingLayerID = 0;
                    }
                    // body.localScale = oldScale;
                    started = -1;
                    Commons.useTongue = true;
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D colis){
        //if the collider is a frog part but not tongue
        if(colis.collider.gameObject.layer == frog && colis.collider.transform.parent.name != "Tongue"){
            // Debug.Log("exit");
            //change the physics layer to phase through things
            foreach (SpriteRenderer sr in body.GetComponentsInChildren<SpriteRenderer>(true)){
                sr.transform.gameObject.layer = phase;
                // sr.sortingLayerName = "FG";
            }
            // oldScale = body.localScale;
            // body.localScale *= 1.2F;
            started = Time.time;
            Commons.useTongue = false;
            //force the tongue to retract so that it doesnt cause issues with un-phasing
            tm.ForceRetract();
        }
    }
}
