using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class MakeEffect : MonoBehaviour
{
    [SerializeField] private Effect eff;
    public float wait = 5;
    public Dictionary<GameObject, object[][]> timers = new Dictionary<GameObject, object[][]>();

    void OnEnable(){
        if(eff == null)
            eff = GetComponent<Effect>();
    }
    

    //check if the collided object is valid for this effect and if it is:
    //add it to the list of effected, store its oreiginal info, and apply the effect
    void OnTriggerEnter2D(Collider2D oColid){
        GameObject other = oColid.gameObject;
        //check if the collided object is valid
        if(eff.check(other)){
            StringWrapper cas = new StringWrapper("");
            if(!timers.ContainsKey(other)){
                object[] nl = new object[]{null, cas};
                //get its info and add this object to the list of effected
                timers.Add(other, new object[][]{nl, eff.getTimerInf(other)});

                Status sta = oColid.gameObject.GetComponent<Status>();
                // Debug.Log(sta);
                if(!sta){
                    Info staCh = oColid.gameObject.GetComponent<Info>();
                    if(staCh && staCh.rStat) sta = staCh.rStat;
                }
                
                //if the collided object is managed then try to have it apply the effect itself
                if(sta){
                    sta.addEffect(eff, cas, gameObject);
                    if(Commons.mSendMessage(sta.gameObject, "m"+eff.name, new object[]{other, timers[other][1], eff}) == null)
                        eff.doIt(other);
                    // catch(Exception e){ eff.doIt(other); Debug.Log("caught: "+eff.name);}
                }
                else eff.doIt(other);
            }
            else timers[other][0][0] = null;
        }
    }

    //start the real timer after the object stops making contact with the source
    void OnTriggerExit2D(Collider2D oColid){
        GameObject other = oColid.gameObject;
        if(eff.check(other) && timers.ContainsKey(other)){
            // Debug.Log(timers[li][1]);
            timers[other][0][0] = Time.time;
            StartCoroutine(changeBack(other));
        }
        // else Debug.Log(oColid.name);
    }


    //after the delay use the stored info to reset the collided objects conditions and remove its entry from storage
    IEnumerator changeBack(GameObject other){
        yield return new WaitForSeconds(wait);
        if(timers.ContainsKey(other)){
            // Debug.Log(timers[li][1]);
            if(timers[other][0][0] != null && Time.time - (float)timers[other][0][0] >= wait){
                // Debug.Log("time");
                switch(timers[other][0][1].ToString()){
                    //if the object is unmanaged then undo the effect from here
                    case "":
                        // Debug.Log("norm_end");
                        eff.unDoIt(new object[]{other, timers[other][1]});
                        break;
                    //if the effect is managed then try to have the object itself undo the effect
                    case "0":
                        Status sta = other.GetComponent<Status>();
                        if(!sta) sta = other.GetComponent<Info>().rStat;
                        sta.removeEffect(eff, (StringWrapper)timers[other][0][1]);
                        if(Commons.mSendMessage(sta.gameObject, "un"+eff.name, new object[]{other, timers[other][1], eff}) == null)
                            eff.unDoIt(new object[]{other, timers[other][1]});
                        break;
                }
                timers.Remove(other);
            }
        }
    }
}
