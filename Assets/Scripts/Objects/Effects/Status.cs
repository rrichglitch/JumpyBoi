using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Status : MonoBehaviour
{
    //this class will hold any status effects of an object
    //and if I go deep enough the check method will hold immersive sim type interactions
    private Dictionary<string, Dictionary<StringWrapper, GameObject>> effects = new Dictionary<string, Dictionary<StringWrapper, GameObject>>();

    //add a new entry to the dictionary of the "type" effects
    //"cas" is the current state of the effect "type" from the source "from"
    public bool addEffect(Effect eff, StringWrapper cas, GameObject from){
        // Debug.Log(type);
        if(!effects.ContainsKey(eff.name))
            effects.Add(eff.name, new Dictionary<StringWrapper, GameObject>());
        // Debug.Log("number of effects: "+effects.Count);
        if(!effects[eff.name].ContainsKey(cas)){
            cas.set("0");
            effects[eff.name].Add(cas, from);
            Check(eff.name, cas);
            // Debug.Log(effects["radiate"].Count);
            return true;
        }
        else return false;
    }

    //remove the entry with the key "cas" in the dictionary of the "type" effects
    public bool removeEffect(Effect eff, StringWrapper cas){
        if(effects[eff.name].ContainsKey(cas)){
            StringWrapper save = cas;
            effects[eff.name].Remove(cas);
            Check(eff.name, save);
            // Debug.Log(effects["radiate"].Count);
            return true;
        }
        else return false;
    }

    //check active effects on this gameObject and adjust their functioning states
    void Check(string type, StringWrapper cas){
        switch(type){
            case ChangeCollidedMat.Name:
                break;
            case Radiate.Name:
                break;
        }
    }

    public Dictionary<StringWrapper, GameObject> getEffect(string type){
        return effects[type];
    }

}
