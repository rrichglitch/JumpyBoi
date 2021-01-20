using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatChild : MonoBehaviour
{
    [SerializeField] Status rstat;
    public Status rStat{get{ return rstat; }}
    void Start(){
        if(rstat == null){
            if(transform.parent != null){
                Status sta = transform.parent.GetComponent<Status>();
                if(sta != null)
                    rstat = sta;
                else if(transform.parent.parent != null){
                    sta = transform.parent.parent.GetComponent<Status>();
                    if(sta != null)
                        rstat = sta;
                }
            }
        }
    }
}
