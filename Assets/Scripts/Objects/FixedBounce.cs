using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedBounce : MonoBehaviour
{
    [SerializeField] private float Force = 0;
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
                inf.whole.AddForce((acpn*-Force)-Physics2D.gravity); //minus gravity to counter gravity?
                lastUse = Time.time;
            }
            else{
                colis.rigidbody.AddForce((acpn*-Force)-Physics2D.gravity); //minus gravity to counter gravity?
                lastUse = Time.time;
            }
            // Debug.Log(acpn);
            // Debug.Log(colis.rigidbody.name);
        }
    }
}
