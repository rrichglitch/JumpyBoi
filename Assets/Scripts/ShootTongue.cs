using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShootTongue : MonoBehaviour
{
    public float speed;
    public float tol;
    public GameObject test;
    private bool th = false;
    private float startPosX;
    public SpawnTongue st;
    private bool moved;
    void Awake(){startPosX = transform.localPosition.x;}
    void OnTongue(InputValue value){
        if(value.isPressed && transform.localPosition.x <= startPosX+tol){
            th = true;
            GetComponents<FixedJoint2D>()[0].enabled = false;
            Vector2 vel = GetComponent<Rigidbody2D>().velocity;
            vel = transform.TransformDirection(new Vector2(vel.x+speed,vel.y));
            GetComponent<Rigidbody2D>().velocity = vel;
            // Debug.Log(GetComponent<Rigidbody2D>().velocity);
        }
        else th = false;
    }
    
}
