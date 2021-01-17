using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Status : MonoBehaviour
{
    //this class will hold any status effects of an object
    //and if I go deep enough the check method will hold immersive sim type interactions
    private Dictionary<string, Dictionary<object, GameObject>> effects = new Dictionary<string, Dictionary<object, GameObject>>(){
        {"slips", new Dictionary<object, GameObject>()},
        {"sticks", new Dictionary<object, GameObject>()}
    };

    //add a new entry to the dictionary of the "type" effects
    //"cas" is the current state of the effect "type" from the source "from"
    public bool addEffect(string type, object cas, GameObject from){
        if(!effects[type].ContainsKey(cas)){
            effects[type].Add(cas, from);
            cas = "0";
            Check(type, cas);
            return true;
        }
        else return false;
    }

    //remove the entry with the key "cas" in the dictionary of the "type" effects
    public bool removeEffect(string type, object cas){
        if(effects[type].ContainsKey(cas)){
            object save = cas;
            effects[type].Remove(cas);
            Check(type, save);
            return true;
        }
        else return false;
    }

    //check active effects on this gameObject and adjust their functioning states
    void Check(string type, object cas){
        switch(type){
            case "slips":
                break;
            case "sticks":
                break;
        }
    }

}
