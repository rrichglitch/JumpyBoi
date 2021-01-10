using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Jump : MonoBehaviour
{
    public int speed;
    private HingeJoint2D[] HJs = new HingeJoint2D[3];
    private bool jumping = false;
    private float[] initTorqs = new float[3];

    void OnJump(InputValue val){jumping = val.isPressed;}

    // Start is called before the first frame update
    void Start()
    {
        int index = 0;
        foreach(HingeJoint2D hj in GetComponentsInChildren<HingeJoint2D>(false)){
            if(hj.name == "Arm"||hj.name == "Calf"||hj.name == "Foot"){
                HJs[index] = hj;
                initTorqs[index] = hj.motor.maxMotorTorque;
                index++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        JointMotor2D save;
        if(jumping){
            for(int i = 0;i<HJs.Length;i++){
                save = HJs[i].motor;
                save.maxMotorTorque = initTorqs[i];

                if(HJs[i].name == "Calf") save.motorSpeed = -speed;
                else if(HJs[i].name == "Foot") save.motorSpeed = speed;
                else save.motorSpeed = -speed;

                HJs[i].motor = save;
            }
        }else{
            for(int i = 0;i<HJs.Length;i++){
                save = HJs[i].motor;

                if(transform.Find("Thigh").GetComponent<Rigidbody2D>().velocity.magnitude > 5) save.maxMotorTorque = Commons.Instance.weak;
                else save.maxMotorTorque = initTorqs[i];

                if(HJs[i].name == "Calf") save.motorSpeed = speed * (float).5;
                else if(HJs[i].name == "Foot") save.motorSpeed = speed*(float)-.5;
                else save.motorSpeed = 100;

                HJs[i].motor = save;
            }
        }
    }
}