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

    public static object mSendMessage(GameObject receiver, string methodName, object value = null){
        object toRet = null;
        object[] val;
        if(value is object[])
            val = (object[])value;
        else
            val = new object[]{value};
        Listener[] listens = (Listener[])receiver.GetComponents(typeof(Listener));
        foreach(Listener c in listens){
            MethodInfo meth = c.GetType().GetMethod(methodName);
            if(meth != null){
                //run and save the return of the function
                object save = meth.Invoke(c, val);
                if(save != null) toRet = save;
            }
        }
        return toRet;
    }
}
