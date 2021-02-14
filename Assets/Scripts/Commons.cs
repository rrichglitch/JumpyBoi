using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Commons : Singleton<Commons>
{
    // public static float weak;
    public float weak;
    public float buttl;
    public GameObject body;
    public static bool useTongue = true;
    [SerializeField] private GameObject notifation;
    private Animator anim;
    private TMP_Text notiText;
    void Start(){
        anim = notifation.GetComponent<Animator>();
        notiText = notifation.transform.GetChild(0).GetComponent<TMP_Text>();
    }

    //sends a notification to the screen in-game
    public void notify(string text){
        notifation.SetActive(true);
        notiText.text = text;
        anim.SetTrigger("pop");
    }

    //methods to get invoked by this should return a value so it can be listened for by the invoker, otherwise it may be assumed the operation has failed
    public static object mSendMessage(GameObject receiver, string methodName, object[] value = null){
        object toRet = null;
        Component[] listens = receiver.GetComponents(typeof(Component));
        // Debug.Log("list: "+ listens);
        foreach(Component c in listens){
            // if(c is Listener){
                MethodInfo meth = c.GetType().GetMethod(methodName, new Type[]{typeof(object[])});
                if(meth != null){
                    //run and save the return of the function
                    object save = meth.Invoke(c, new object[]{value});
                    if(save != null) toRet = save;
                }
            // }
        }
        return toRet;
    }

    //get the total bounds of an object and its children in world space
    //super useful for scaling purposes
    //pass a second parameter of true to base the bounds on colliders instead of renderers
    static public Bounds GetMaxBounds(GameObject g, bool colliderBased = false){
        Bounds b = new Bounds(g.transform.position, Vector3.zero);
        Bounds temp;
        if(colliderBased)
            foreach (Collider2D c in g.GetComponentsInChildren<Collider2D>()){
                temp = new Bounds(c.transform.TransformPoint(c.bounds.center), c.bounds.size);
                b.Encapsulate(temp);
            }
        else
            foreach (Renderer r in g.GetComponentsInChildren<Renderer>()){
                temp = new Bounds(r.transform.TransformPoint(r.bounds.center), r.bounds.size);
                b.Encapsulate(temp);
            }
        return b;
    }

    //a method to get the an element the appropriate count away from an index while excluding certain indices
    //pretty hard to think about imo
    static public int GetValidIndex(int totalLength, int startInd, int endDist){
        int dir = endDist/Mathf.Abs(endDist);
        int ret = startInd;
        for(int skinCount = 0; skinCount != endDist && Mathf.Abs(skinCount) < totalLength-1; skinCount += dir){
            ret = (ret + dir)%totalLength;
            if(ret + dir < 0)
                ret = totalLength+dir;
        }
        return ret;
    }
    static public int GetValidIndex(int totalLength, int startInd, int endDist, List<int> exlusions){
        int dir = endDist/Mathf.Abs(endDist);
        int ret = startInd;
        for(int skinCount = 0; skinCount != endDist && Mathf.Abs(skinCount) < totalLength-1; skinCount += dir){
            ret = (ret + dir)%totalLength;
            if(ret + dir < 0)
                ret = totalLength+dir;
            while(exlusions.Contains(ret) && Mathf.Abs(ret) < totalLength){
                ret = (ret + dir)%totalLength;
                if(ret + dir < 0)
                    ret = totalLength+dir;
            }
        }
        return ret;
    }
}
// public interface Listener{}