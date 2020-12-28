using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShootTongue : MonoBehaviour
{
    public float speed;
    public float tol;
    private Vector2 startPos;
    void Awake(){startPos = transform.localPosition;}
    void OnTongue(InputValue value){
        if(value.isPressed){
            if(transform.localPosition.magnitude < startPos.magnitude+tol){
                GetComponent<FixedJoint2D>().enabled = false;
                Vector2 vel = transform.InverseTransformDirection(GetComponent<Rigidbody2D>().velocity);
                GetComponent<Rigidbody2D>().velocity = transform.TransformDirection(new Vector2(vel.x+speed,vel.y));
            }
            else{
                FixedJoint2D[] fjs = GetComponents<FixedJoint2D>();
                for(int i = 1;i<fjs.Length;i++)
                    Destroy(fjs[i]);
                GetComponentInChildren<Sticky>().stickOn = false;
                StartCoroutine(unStick());
            }
        }
    }
    IEnumerator unStick(){yield return new WaitForSeconds(1); GetComponentInChildren<Sticky>().stickOn = true;}
    
}
