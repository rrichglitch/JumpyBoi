using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glider : MonoBehaviour
{
    //disable this component all together when you dont want gliding
    [Range(0,1)] public float efficiency = .1F;
    [Range(0,1)] public float tumbleSlow = .1F;
    private Rigidbody2D bod;
    // Start is called before the first frame update
    void Start(){
        bod = transform.parent.GetComponent<Rigidbody2D>();
    }

    //curv the bodies velocity to be in-line with the glider
    void FixedUpdate(){
        Vector2 curVel = bod.velocity;
        float rot = (transform.eulerAngles.z+90)%360*Mathf.Deg2Rad;
        Vector2 closeEnd = new Vector2(Mathf.Cos(rot), Mathf.Sin(rot));
        //make a new modded velocity by stepping from the current velocity towards the direction the glider end is pointing
        //use lerp to acheive this?
        //set the body velocity to the new curved vector
        bod.velocity = Vector2.Lerp(curVel, closeEnd * curVel.magnitude, efficiency);

        // slow the bodies head over heels rotation
        bod.angularVelocity = Mathf.Lerp(bod.angularVelocity, 0, tumbleSlow);
    }
}
