using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBody : MonoBehaviour
{
    public int speed;
    private HingeJoint2D hj2D;
    private JointMotor2D motor;
    private bool rotating = false;
    void OnRotateBod(){rotating = !rotating;}
    void Start()
    {
        hj2D = GetComponent<HingeJoint2D>();
        motor = hj2D.motor;
    }
    void Update()
    {
        if(rotating){
            motor.motorSpeed = speed;
        }else{
            motor.motorSpeed = speed*(float)-.5;
        }
        hj2D.motor = motor;
    }
}