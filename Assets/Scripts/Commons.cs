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
    [SerializeField] private GameObject notifation;
    private Animator anim;
    private TMP_Text notiText;
    void Start(){
        anim = notifation.GetComponent<Animator>();
        notiText = notifation.transform.GetChild(0).GetComponent<TMP_Text>();
    }

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
            if(c is Listener){
                MethodInfo meth = c.GetType().GetMethod(methodName, new Type[]{typeof(object[])});
                if(meth != null){
                    //run and save the return of the function
                    object save = meth.Invoke(c, new object[]{value});
                    if(save != null) toRet = save;
                }
            }
        }
        return toRet;
    }
}
