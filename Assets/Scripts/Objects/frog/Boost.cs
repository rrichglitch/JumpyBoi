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
    private Info inf;
    void Start(){
        bod = transform.parent.GetComponent<Rigidbody2D>();
        inf = transform.parent.GetComponent<Info>();
    }
    public void OnBoost(InputAction.CallbackContext ctx){
        if(ctx.performed){
            if(inf != null){
                if(inf.flags.Contains("inWater")) coolDown = .5F;
                else coolDown = 3;
            }
            float curTime = Time.time;
            if(curTime - lastFire >= coolDown){
                float rot = ((transform.eulerAngles.z+90)%360)*Mathf.Deg2Rad;
                Vector2 force = new Vector2(Mathf.Cos(rot), Mathf.Sin(rot));
                force *= strength;
                bod.AddForce(force);
                lastFire = curTime;
            }
        }
    }
}
