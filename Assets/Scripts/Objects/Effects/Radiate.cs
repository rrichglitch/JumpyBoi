using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Radiate : Effect
{
    [SerializeField] private Color newCol;
    public const string Name = "Radiate";
    public override string name {get{return Name;}}

    //check if collided object even has a light to be lit
    public override bool check(GameObject other){
        if(other.GetComponent<Light2D>())
            return true;
        else return false;
    }

    //return the lights old enabled state and color so it can be stored
    public override object[] getTimerInf(GameObject other){
        Light2D li = other.GetComponent<Light2D>();
        bool wasOn = li.enabled;
        Color oldCol = li.color;
        return new object[]{wasOn, oldCol};
    }

    //change the lights color and turn it on
    public override void doIt(GameObject other){
        Light2D li = other.GetComponent<Light2D>();
        li.color = newCol;
        li.enabled = true;
    }

    //take the stored info and use it to reset the light
    public override void unDoIt(object[] args){
        Light2D li = ((GameObject)args[0]).GetComponent<Light2D>();
        // Debug.Log(((object[])args[1])[0]);
        li.color = (Color)((object[])args[1])[1];
        li.enabled = (bool)((object[])args[1])[0];
    }
}