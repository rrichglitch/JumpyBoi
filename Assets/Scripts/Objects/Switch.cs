using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Switch : MonoBehaviour
{
    /*this button script needs to be on an object with a rigidbody2D component in no position or rotation relative to its parent to work correctly.
    it only handles the calling of a function when this object is in the down position. Movement needs to be handled
    with other scripts or joints*/
    [SerializeField] private UnityEvent trueFunc;
    [SerializeField] private UnityEvent falseFunc;
    [SerializeField] private float repeatDelay = 0;
    //the distance down the button must be to actually fire its function
    [SerializeField] private float actDist = 0;
    [SerializeField] private Vector2 sPos;
    [SerializeField] private bool autoSPos = true;
    private Rigidbody2D rb2D;
    private float lastCall = 0;
    private bool lastState;
    void Start(){
        if(autoSPos){
            rb2D = GetComponent<Rigidbody2D>();
            sPos = transform.localPosition;
        }
        lastState = (transform.localPosition.y < sPos.y);
    }
    void Update(){
        if(Time.time-lastCall >= repeatDelay && !rb2D.IsSleeping()){
            //check if the switch has moved to true side
            if(transform.localPosition.y < sPos.y - actDist && !lastState){
                trueFunc.Invoke();
                lastCall = Time.time;
                lastState = true;
            }
            //check if the switch has moved to the false side
            else if(transform.localPosition.y > sPos.y + actDist && lastState){
                falseFunc.Invoke();
                lastCall = Time.time;
                lastState = false;
            }
        }
    }
}
