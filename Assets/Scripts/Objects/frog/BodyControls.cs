using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BodyControls : MonoBehaviour
{
    [SerializeField] private float lowAng = 270; //set this to what the threshold should be where the hand curls up
    [SerializeField] private float upAng = 330; //set this to what the threshold should be where the hand curls up
    [SerializeField] private float baseEnd = 80; //set this to what the threshold should be where the hand curls up
    [SerializeField] private float baseStart = 240; //set this to what the threshold should be where the hand curls up
    [SerializeField] private float handPropMod = 5; //how much stronger should the hand be when trying to get off the head or reach out
    [SerializeField] private float revMod = .5F; //set this to how strong motors should be when not actively used
    // [SerializeField] private float airFricThresh = 10; //set this to the threshhold at which air friction kicks in to weaken motor strength
    [SerializeField] private float weakMod = .1F; //set this to how weak motors will be when the body is at high velocity
    // [SerializeField] private float calfRotMod = 1; //adjust this so that the pointing calf rotation feels good
    [SerializeField] private float footRotMod = 1; //adjust this so that the pointing foot rotation feels good
    [SerializeField] private float toeFlickMod = 5; //adjust this so that the pointing foot rotation feels good
    [SerializeField] private float hipExtMod = 1.5F; //adjust this so that the pointing foot rotation feels good
    private bool hipRot = false;
    private bool extend = false;
    private HingeJoint2D thighHJ,calfHJ,footHJ,toeHJ,armHJ,handHJ;
    private JointMotor2D thighMotor,calfMotor,footMotor,toeMotor,armMotor,handMotor;
    private float thighTorq,calfTorq,footTorq,toeTorq,armTorq,handTorq;
    private float thighSpeed,calfSpeed,footSpeed,handSpeed;
    // private RelativeJoint2D armRJ;
    private SpringJoint2D armSJ;
    private Rigidbody2D body;
    public void OnRotateBod(InputAction.CallbackContext ctx){hipRot = ctx.performed;}
    public void OnJump(InputAction.CallbackContext ctx){extend = ctx.performed;}
    
    void Start(){
        //grab the one-offs
        body = GetComponent<Rigidbody2D>();
        // armRJ = transform.Find("Arm").GetComponent<RelativeJoint2D>();
        armSJ = transform.Find("Arm").GetComponent<SpringJoint2D>();

        //grab the hinge joints
        thighHJ = transform.Find("Thigh").GetComponent<HingeJoint2D>();
        calfHJ = transform.Find("Thigh").GetChild(0).GetComponent<HingeJoint2D>();
        footHJ = transform.Find("Thigh").GetChild(0).GetChild(0).GetComponent<HingeJoint2D>();
        toeHJ = transform.Find("Thigh").GetChild(0).GetChild(0).GetChild(0).GetComponent<HingeJoint2D>();

        armHJ = transform.Find("Arm").GetComponent<HingeJoint2D>();
        handHJ = transform.Find("Arm").GetChild(0).GetComponent<HingeJoint2D>();

        //store the motors of the hingejoints so they can be re-applied to the HJs later since
        //they are non-mutable without taking this extra step
        thighMotor = thighHJ.motor;
        calfMotor = calfHJ.motor;
        footMotor = footHJ.motor;
        toeMotor = toeHJ.motor;

        armMotor = armHJ.motor;
        handMotor = handHJ.motor;

        //store the initial torque values so they can be restored later
        thighTorq = thighHJ.motor.maxMotorTorque;
        calfTorq = calfHJ.motor.maxMotorTorque;
        footTorq = footHJ.motor.maxMotorTorque;
        toeTorq = toeHJ.motor.maxMotorTorque;

        armTorq = armHJ.motor.maxMotorTorque;
        handTorq = handHJ.motor.maxMotorTorque;

        //store the initial Speed values so they can be restored later
        thighSpeed = thighHJ.motor.motorSpeed;
        calfSpeed = calfHJ.motor.motorSpeed;
        footSpeed = footHJ.motor.motorSpeed;

        //dont need the arm speed bc I dont mess with it
        handSpeed = handHJ.motor.motorSpeed;
    }

    void Update(){
        
        //control hip and foot rotation to point a direction
        if(hipRot){
            if(extend) thighMotor.motorSpeed = thighSpeed * hipExtMod;
            else thighMotor.motorSpeed = thighSpeed;
            if(transform.localEulerAngles.z > baseEnd && transform.localEulerAngles.z < baseStart)
                footMotor.maxMotorTorque = footTorq * weakMod;
            else footMotor.maxMotorTorque = footTorq;
            // calfMotor.motorSpeed = -thighSpeed * calfRotMod;
            calfMotor.maxMotorTorque = calfTorq * weakMod;
            footMotor.motorSpeed = thighSpeed * footRotMod;
        }
        else{
            thighMotor.motorSpeed = thighSpeed * -revMod;
            footMotor.maxMotorTorque = footTorq;
        }

        //control limb extension
        if(extend){
            // armHJ.useMotor = true;
            calfMotor.maxMotorTorque = calfTorq;
            calfMotor.motorSpeed = calfSpeed;
            footMotor.maxMotorTorque = footTorq;
            footMotor.motorSpeed = footSpeed;
            toeMotor.maxMotorTorque = toeTorq * toeFlickMod;
            handMotor.maxMotorTorque = handTorq * handPropMod;
        }
        else{
            // armHJ.useMotor = false;
            // calfMotor.motorSpeed = calfSpeed * -revMod;
            // footMotor.motorSpeed = footSpeed * -revMod;
            toeMotor.maxMotorTorque = toeTorq;
            // handMotor.maxMotorTorque = handTorq;
        }

        //run if neither the hipRot nor the extension is run
        if(!(hipRot || extend)){
            calfMotor.maxMotorTorque = calfTorq;
            calfMotor.motorSpeed = thighSpeed;
            footMotor.motorSpeed = -thighSpeed;
        }

        //run if the the frogs head is too low
        // Debug.Log(transform.localEulerAngles.z);
        if(transform.localEulerAngles.z > lowAng && transform.localEulerAngles.z < upAng){
            handMotor.maxMotorTorque = handTorq * handPropMod;
            handMotor.motorSpeed = -handSpeed;
        }
        else{
            if(!extend) handMotor.maxMotorTorque = handTorq;
            handMotor.motorSpeed = handSpeed;
        }

        //what to run in the resting state from controls, mostly passive arm control stuff
        if(!hipRot && !extend){ armSJ.enabled = true; }
        else{ armSJ.enabled = false; }

        //what to run if the whole body is currently being whipped around
        //simulates air friction/pressure a bit
        // if(body.velocity.magnitude > airFricThresh){
        //     thighMotor.maxMotorTorque = thighTorq *weakMod;
        //     calfMotor.maxMotorTorque = calfTorq *weakMod;
        //     footMotor.maxMotorTorque = footTorq *weakMod;

        //     armMotor.maxMotorTorque = armTorq *weakMod;
        //     handMotor.maxMotorTorque = handTorq *weakMod;
        // }
        // else{
        //     thighMotor.maxMotorTorque = thighTorq;
        //     calfMotor.maxMotorTorque = calfTorq;
        //     footMotor.maxMotorTorque = footTorq;

        //     armMotor.maxMotorTorque = armTorq;
        //     handMotor.maxMotorTorque = handTorq;
        // }

        //replace the hinge joint motors with the mutated saves
        //they are non-mutable without taking this extra step
        thighHJ.motor = thighMotor;
        calfHJ.motor = calfMotor;
        footHJ.motor = footMotor;
        toeHJ.motor = toeMotor;

        //had to add this condition because it seems that seting a hingejoints motor automatically turns it on
        if(extend){ armHJ.motor = armMotor; armHJ.useMotor = true; } else{ armHJ.useMotor = false; }
        handHJ.motor = handMotor;
    }
}
