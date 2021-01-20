using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Status : MonoBehaviour
{
    //this class will hold any status effects of an object
    //and if I go deep enough the check method will hold immersive sim type interactions
    private Dictionary<string, Dictionary<StringWrapper, GameObject>> effects = new Dictionary<string, Dictionary<StringWrapper, GameObject>>(){
        {"slips", new Dictionary<StringWrapper, GameObject>()},
        {"radiate", new Dictionary<StringWrapper, GameObject>()}
    };

    //add a new entry to the dictionary of the "type" effects
    //"cas" is the current state of the effect "type" from the source "from"
    public bool addEffect(string type, StringWrapper cas, GameObject from){
        if(!effects[type].ContainsKey(cas)){
            cas.set("0");
            effects[type].Add(cas, from);
            Check(type, cas);
            // Debug.Log(effects["radiate"].Count);
            return true;
        }
        else return false;
    }

    //remove the entry with the key "cas" in the dictionary of the "type" effects
    public bool removeEffect(string type, StringWrapper cas){
        if(effects[type].ContainsKey(cas)){
            StringWrapper save = cas;
            effects[type].Remove(cas);
            Check(type, save);
            // Debug.Log(effects["radiate"].Count);
            return true;
        }
        else return false;
    }

    //check active effects on this gameObject and adjust their functioning states
    void Check(string type, StringWrapper cas){
        switch(type){
            case "slips":
                break;
            case "radiate":
                break;
        }
    }

    public Dictionary<StringWrapper, GameObject> getEffect(string type){
        return effects[type];
    }

}
