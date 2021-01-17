using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCollidedMat : MonoBehaviour
{
    public PhysicsMaterial2D changeTo;
    public float wait = 5;
    public string effectName = "";
    private Dictionary<Collider2D, object[]> timers = new Dictionary<Collider2D, object[]>();
    
    void OnTriggerEnter2D(Collider2D oColid){
        if(!timers.ContainsKey(oColid)){
            PhysicsMaterial2D old = oColid.sharedMaterial;
            object cas = "";
            timers.Add(oColid, new object[]{old,null, cas});
            oColid.sharedMaterial = changeTo;
            Status sta = oColid.gameObject.GetComponent<Status>();
            if(sta){
                timers[oColid][2] = cas;
                sta.addEffect(effectName, cas, gameObject);
            }
        }
    }
    void OnTriggerExit2D(Collider2D oColid){
        if(timers.ContainsKey(oColid)){
            timers[oColid][1] = Time.time;
            StartCoroutine(changeBack(oColid));
        }
    }

    IEnumerator changeBack(Collider2D oColid){
        yield return new WaitForSeconds(wait);
        if(oColid && timers.ContainsKey(oColid)){
            switch(timers[oColid][2]){
                case "":
                    Debug.Log("cas is empty");
                    if(Time.time - (float)timers[oColid][1] >= wait){
                        oColid.sharedMaterial = (PhysicsMaterial2D)timers[oColid][0];
                        timers.Remove(oColid);
                    }
                    break;
                case "0":
                    if(Time.time - (float)timers[oColid][1] >= wait){
                        oColid.sharedMaterial = (PhysicsMaterial2D)timers[oColid][0];
                        Status sta = oColid.gameObject.GetComponent<Status>();
                        if(sta) sta.removeEffect(effectName, timers[oColid][2]);
                        timers.Remove(oColid);
                    }
                    break;
            }
        }
        else timers.Remove(oColid);
    }
}
