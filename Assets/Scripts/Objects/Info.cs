using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A class to hold general info about the gameobject
public class Info : MonoBehaviour
{
    //owner is the ulitmate parent of this object and should sometimes receive physics directly
    [SerializeField] private Rigidbody2D owner;
    public Rigidbody2D whole {get{return owner;}}
    [SerializeField]private bool initRStat = false;
    [SerializeField] private Status rstat;
    public Status rStat{get{ return rstat; }}
    public List<string> flags = new List<string>();
    void Start(){
        if(initRStat && rstat == null){
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
