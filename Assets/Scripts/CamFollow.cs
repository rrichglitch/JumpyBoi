using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    public int TeamIndex;
    public Vector3 offset;
    private Transform target;
    
    // Update is called once per frame
    void LateUpdate(){
        // target = CommonTools.getLead();
        // if(target)
        //     transform.position = new Vector3(target.position.x, target.position.y,-10);
    }
}
