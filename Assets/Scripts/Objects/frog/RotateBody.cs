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
    private HingeJoint2D arm;
    private JointAngleLimits2D lims;
    private float initMax;
    private float initTorq;
    void OnRotateBod(InputValue val){rotating = val.isPressed;}
    void Start()
    {
        hj2D = GetComponent<HingeJoint2D>();
        motor = hj2D.motor;
        arm = transform.parent.Find("Arm").GetComponent<HingeJoint2D>();
        startRot = hj2D.jointAngle;
        lims = arm.limits;
        initMax = lims.max;
        initTorq = motor.maxMotorTorque;
    }
    void Update()
    {
        if(rotating){
            motor.motorSpeed = speed;
            motor.maxMotorTorque = initTorq;
        }
        else{
            motor.motorSpeed = speed*(float)-.5;
            if(transform.GetComponent<Rigidbody2D>().velocity.magnitude > 5) motor.maxMotorTorque = Commons.Instance.weak;
            else motor.maxMotorTorque = initTorq;
        }
        hj2D.motor = motor;
        lims.max = initMax - (float)((startRot-hj2D.jointAngle)*.6);
        if(lims.max > lims.min + 190) lims.max = lims.min + 190;
        if(lims.max < lims.min + 95) lims.max = lims.min + 95;
        arm.limits = lims;
    }
}