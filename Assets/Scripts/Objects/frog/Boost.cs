using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Boost : MonoBehaviour
{
    public float strength = 10;
    public float coolDown = 3;
    public Vector2 up;
    public Vector2 forward;
    private float lastFire = 0;
    private Rigidbody2D bod;
    void Start(){
        bod = GetComponent<Rigidbody2D>();
    }
    public void OnBoost(InputAction.CallbackContext ctx){
        if(ctx.performed){
            float curTime = Time.time;
            if(curTime - lastFire >= coolDown){
                Vector2 force = Vector2.zero;
                if(ctx.action.name == "BoostUp") force = up*strength;
                else if(ctx.action.name == "BoostFor") force = forward*strength;
                bod.AddRelativeForce(force);
                lastFire = curTime;
            }
        }
    }
}
