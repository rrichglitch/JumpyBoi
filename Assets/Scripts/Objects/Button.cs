using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Button : MonoBehaviour
{
    /*this button script needs to be on an object with a rigidbody2D component in no position or rotation relative to its parent to work correctly.
    it only handles the calling of a function when this object is in the down position. Movement needs to be handled
    with other scripts or joints*/
    [SerializeField] private UnityEvent chooseFunction;
    [SerializeField] private float repeatDelay = 0;
    //the distance down the button must be to actually fire its function
    [SerializeField] private float actDist = 0;
    private Rigidbody2D rb2D;
    private Vector2 sPos;
    private float lastCall = 0;
    void Start(){
        rb2D = GetComponent<Rigidbody2D>();
        sPos = transform.localPosition;
    }
    void Update(){
        if(Time.time-lastCall >= repeatDelay && !rb2D.IsSleeping()){
            if(transform.localPosition.y < sPos.y - actDist)
            chooseFunction.Invoke();
            lastCall = Time.time;
        }
    }
}
