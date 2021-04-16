using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedBounce : MonoBehaviour
{
    [SerializeField] private float outVel = 50;
    private float lastUse = 0;
    void OnCollisionEnter2D(Collision2D colis){
        
        //if the collided object ignores bounces then just return
        Info inf = colis.collider.transform.GetComponent<Info>();
        if(inf && inf.flags.Contains("ignoreBounce")){
            return;
        }

        List<ContactPoint2D> cps = new List<ContactPoint2D>();
        // private Vector2 acp = new Vector2();
        Vector2 acpn = new Vector2();
        //get the average normal of this surface
        colis.otherCollider.GetContacts(cps);
        int divBy = 0;
        for(int i = 0; i < cps.Count; i++){
            if(cps[i].normal.magnitude > .1){
                acpn += cps[i].normal;
                divBy++;
            }
        }
        if(divBy != 0) acpn /= divBy;

        // one bounce per .4 seconds to prevent oscillation or constructive interference
        if(Time.time - lastUse >= .4){
            // Debug.Log(inf);
            if(inf && inf.whole != null){
                propegateMomentum(inf.whole, -acpn * outVel);
            }
            else{
                propegateMomentum(colis.rigidbody, -acpn * outVel);
            }
            lastUse = Time.time;
        }
    }

    void propegateMomentum(Rigidbody2D main, Vector2 vel){
        foreach(Rigidbody2D rb in main.GetComponentsInChildren<Rigidbody2D>()){
            rb.angularVelocity = 0;
            rb.velocity = vel;
            // Debug.Log(rb.name);
        }
    }
}
