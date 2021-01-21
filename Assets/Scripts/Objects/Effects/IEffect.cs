using UnityEngine;

public abstract class Effect : MonoBehaviour
{
    public abstract new string name {get;}
    public abstract bool check(GameObject other);
    public abstract object[] getTimerInf(GameObject other);
    public abstract void doIt(GameObject other);
    public abstract void unDoIt(object[] args);
    public override string ToString(){ return name; }
}
