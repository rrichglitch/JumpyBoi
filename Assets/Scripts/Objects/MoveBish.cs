using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBish : MonoBehaviour
{
    public float speed = 0;
    private float activeSpeed = 0;
    public float duration = 0;
    public float endZone = .3F;
    private Vector3 movePerFrame = Vector3.zero;
    private Vector3? destination = null;
    public delegate void Callback();
    private Callback cb = null;
    public Transform bishTarget = null;

    // Update is called once per frame
    void Update()
    {
        if(destination != null){
            if(((Vector3)destination - transform.position).magnitude > endZone){
                transform.Translate(movePerFrame*Time.deltaTime);
            }
            else{
                destination = null;
                activeSpeed = 0;
                movePerFrame = Vector3.zero;
                if(cb != null) cb();
            }
        }
    }

    public void Move(Vector3 moveTo, Callback cbp = null){
        cb = cbp;
        destination = moveTo;
        movePerFrame = moveTo - transform.position;
        if(speed == 0){
            activeSpeed = movePerFrame.magnitude/duration;
        }
        else{
            activeSpeed = speed;
        }
        movePerFrame = movePerFrame.normalized * speed;
    }

    [ContextMenu("Test")]
    void test(){
        Vector3 start = transform.position;
        Move(new Vector3(0,2,0), ()=> { Move(start); });
    }

    public bool moveToBish(Callback callback = null){
        if(bishTarget != null){
            Move(bishTarget.position, callback);
            return true;
        }
        return false;
    }
}
