using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Thruster : MonoBehaviour
{
    public float strength = 10;
    public float coolDown = 3;
    public Vector2 direction;
    private float lastFire = 0;
    private Rigidbody2D bod;
    private Vector2 force;
    void Start(){
        bod = GetComponent<Rigidbody2D>();
        force = direction*strength;
    }
    public void OnThrust(InputAction.CallbackContext ctx){
        if(ctx.performed){
            float curTime = Time.time;
            if(curTime - lastFire >= coolDown){
                bod.AddRelativeForce(force);
                lastFire = curTime;
                Debug.Log("booost!");
            }
        }
    }
}
