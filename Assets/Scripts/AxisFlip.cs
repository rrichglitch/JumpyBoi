using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisFlip : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        JointMotor2D save;
        foreach(HingeJoint2D hj in GetComponentsInChildren<HingeJoint2D>()){
            save = hj.motor;
            save.motorSpeed *= transform.localScale.x;
            hj.motor = save;
        }
    }
}
