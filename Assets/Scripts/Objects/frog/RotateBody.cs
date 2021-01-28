using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotateBody : MonoBehaviour
{
    public int speed;
    private HingeJoint2D hj2D;
    private JointMotor2D motor;
    private bool rotating = false;
    private float startRot;
    // private HingeJoint2D arm;
    // private JointAngleLimits2D lims;
    // private float initMax;
    private float initTorq;
    public void OnRotateBod(InputAction.CallbackContext ctx){rotating = ctx.performed;}
    void Start()
    {
        hj2D = GetComponent<HingeJoint2D>();
        motor = hj2D.motor;
        startRot = hj2D.jointAngle;
        initTorq = motor.maxMotorTorque;
        // arm = transform.parent.Find("Arm").GetComponent<HingeJoint2D>();
        // lims = arm.limits;
        // initMax = lims.max;
    }
    void Update()
    {
        //if the rotate button is held then apply a torque on the waist
        if(rotating){
            motor.motorSpeed = speed;
            motor.maxMotorTorque = initTorq;
        }
        //otherwise apply a lesser toque in the opposite direction towards the base state
        else{
            motor.motorSpeed = speed*(float)-.5;
            if(transform.GetComponent<Rigidbody2D>().velocity.magnitude > 5) motor.maxMotorTorque = Commons.Instance.weak;
            else motor.maxMotorTorque = initTorq;
        }
        hj2D.motor = motor;

        //use limmits to keep the arm somewhat inline with the rotation at the waist
        // lims.max = initMax - (float)((startRot-hj2D.jointAngle)*.6);
        // //cap the min and max values
        // if(lims.max > lims.min + 190) lims.max = lims.min + 190;
        // if(lims.max < lims.min + 95) lims.max = lims.min + 95;
        // arm.limits = lims;
    }
}