using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour
{
    public int speed;
    private HingeJoint2D[] HJs;
    private bool jumping = false;
    void OnJump(){jumping = !jumping;}

    // Start is called before the first frame update
    void Start()
    {
        int index = 0;
        HJs = new HingeJoint2D[3];
        foreach(HingeJoint2D hj in GetComponentsInChildren<HingeJoint2D>(false)){
            if(hj.name == "Arm"||hj.name == "Calf"||hj.name == "Foot"){
                HJs[index] = hj;
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
                if(HJs[i].name == "calf")save.maxMotorTorque = 2000;
                if(HJs[i].name == "Foot") save.motorSpeed = speed;
                else save.motorSpeed = -speed;
                HJs[i].motor = save;
            }
        }else{
            for(int i = 0;i<HJs.Length;i++){
                save = HJs[i].motor;
                if(HJs[i].name == "calf")save.maxMotorTorque = 5;
                if(HJs[i].name == "Foot") save.motorSpeed = speed*(float)-.5;
                else save.motorSpeed = speed*(float).5;
                HJs[i].motor = save;
            }
        }
    }
}