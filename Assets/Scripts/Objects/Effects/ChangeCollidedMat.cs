using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCollidedMat : Effect
{
    [SerializeField] private PhysicsMaterial2D newMat;
    public const string Name = "ChangeCollidedMat";
    public override string name {get{return Name;}}
    public override bool check(GameObject other){
        return true;
    }

    //provide the original material so it can be stored
    public override object[] getTimerInf(GameObject other){
        return new object[]{other.GetComponent<Collider2D>().sharedMaterial};
    }

    //change the material to the neww one
    public override void doIt(GameObject other){
        other.GetComponent<Collider2D>().sharedMaterial = newMat;
    }

    //change the material back to the old one provided from storage
    public override void unDoIt(object[] args){
        ((GameObject)args[0]).GetComponent<Collider2D>().sharedMaterial = (PhysicsMaterial2D)((object[])args[1])[0];
    }
}
