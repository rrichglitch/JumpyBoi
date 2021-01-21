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
    
    void OnTriggerEnter2D(Collider2D oColid){
        GameObject other = oColid.gameObject;
        if(eff.check(other)){
            StringWrapper cas = new StringWrapper("");
            if(!timers.ContainsKey(other)){
                object[] nl = new object[]{null, cas};
                timers.Add(other, new object[][]{nl, eff.getTimerInf(other)});

                Status sta = oColid.gameObject.GetComponent<Status>();
                // Debug.Log(sta);
                if(!sta){
                    StatChild staCh = oColid.gameObject.GetComponent<StatChild>();
                    if(staCh) sta = staCh.rStat;
                }
                
                if(sta){
                    sta.addEffect(eff, cas, gameObject);
                    Commons.mSendMessage(sta.gameObject, "m"+eff.name, new object[]{other, timers[other][1], eff});
                    // catch(Exception e){ eff.doIt(other); Debug.Log("caught: "+eff.name);}
                }
                else eff.doIt(other);
            }
            else timers[other][0][0] = null;
        }
    }
    void OnTriggerExit2D(Collider2D oColid){
        GameObject other = oColid.gameObject;
        if(eff.check(other) && timers.ContainsKey(other)){
            // Debug.Log(timers[li][1]);
            timers[other][0][0] = Time.time;
            StartCoroutine(changeBack(other));
        }
        // else Debug.Log(oColid.name);
    }

    IEnumerator changeBack(GameObject other){
        yield return new WaitForSeconds(wait);
        if(timers.ContainsKey(other)){
            // Debug.Log(timers[li][1]);
            if(timers[other][0][0] != null && Time.time - (float)timers[other][0][0] >= wait){
                if(eff.check(other)){
                    // Debug.Log("time");
                    switch(timers[other][0][1].ToString()){
                        case "":
                            // Debug.Log("norm_end");
                            eff.unDoIt(new object[]{other, timers[other][1]});
                            break;
                        case "0":
                            Status sta = other.GetComponent<Status>();
                            if(!sta) sta = other.GetComponent<StatChild>().rStat;
                            sta.removeEffect(eff, (StringWrapper)timers[other][0][1]);
                            Commons.mSendMessage(sta.gameObject, "un"+eff.name, new object[]{other, timers[other][1], eff});
                            break;
                    }
                }
                timers.Remove(other);
            }
        }
    }
}
