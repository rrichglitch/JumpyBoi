using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Radiation : MonoBehaviour
{
    public Color newCol;
    public float wait = 5;
    public string effectName = "";
    public Dictionary<Light2D, object[]> timers = new Dictionary<Light2D, object[]>();
    
    void OnTriggerEnter2D(Collider2D oColid){
        Light2D li = oColid.gameObject.GetComponent<Light2D>();
        if(li){
            bool wasOn = li.enabled;
            Color oldCol = li.color;
            StringWrapper cas = new StringWrapper("");
            if(!timers.ContainsKey(li)){
                timers.Add(li, new object[]{null, cas, wasOn, oldCol});

                Status sta = oColid.gameObject.GetComponent<Status>();
                // Debug.Log(sta);
                if(!sta){
                    StatChild staCh = oColid.gameObject.GetComponent<StatChild>();
                    if(staCh) sta = staCh.rStat;
                }
                
                if(sta){
                    sta.addEffect(effectName, cas, gameObject);
                    sta.gameObject.SendMessage("rad", new object[]{li, newCol});
                }
                else{
                    li.color = newCol;
                    li.enabled = true;
                }
            }
            else timers[li][0] = null;
        }
    }
    void OnTriggerExit2D(Collider2D oColid){
        Light2D li = oColid.gameObject.GetComponent<Light2D>();
        if(li && timers.ContainsKey(li)){
            // Debug.Log(timers[li][1]);
            timers[li][0] = Time.time;
            StartCoroutine(changeBack(li));
        }
        // else Debug.Log(oColid.name);
    }

    IEnumerator changeBack(Light2D li){
        yield return new WaitForSeconds(wait);
        if(timers.ContainsKey(li)){
            // Debug.Log(timers[li][1]);
            if(timers[li][0] != null && Time.time - (float)timers[li][0] >= wait){
                if(li){
                    // Debug.Log(timers[li][1]);
                    switch(timers[li][1].ToString()){
                        case "":
                            li.enabled = (bool)timers[li][2];
                            li.color = (Color)timers[li][3];
                            break;
                        case "0":
                            Status sta = li.gameObject.GetComponent<Status>();
                            if(!sta) sta = li.gameObject.GetComponent<StatChild>().rStat;
                            sta.removeEffect(effectName, (StringWrapper)timers[li][1]);
                            sta.SendMessage("unRad", new object[]{li, timers[li][2], timers[li][3]});
                            break;
                    }
                }
                timers.Remove(li);
            }
        }
    }
}
