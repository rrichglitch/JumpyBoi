using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Boost : MonoBehaviour
{
    public float strength = 10;
    public float coolDown = 3;
    private float lastFire = 0;
    private Rigidbody2D bod;
    private Vector2 force;
    void Start(){
        bod = GetComponent<Rigidbody2D>();
        force = new Vector2(.866F * strength, .5F * strength);
    }
    public void OnBoost(InputAction.CallbackContext ctx){
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
