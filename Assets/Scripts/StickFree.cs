using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickFree : MonoBehaviour
{
    public List<Rigidbody2D> free = new List<Rigidbody2D>();
    private List<Joint2D> tipStucks;
    void Start(){ tipStucks = transform.GetChild(0).Find("Tongue_Tip").GetComponent<Sticky>().stucks; }
    void OnTriggerEnter2D(Collider2D oColid){
        free.Add(oColid.attachedRigidbody);
        foreach(GameObject ts in transform.GetComponentInChildren<SpawnTongu>().tongueSegs)
            for (int i = ts.GetComponent<Sticky>().stucks.Count - 1; i >= 0; i--)
                if(free.Contains(ts.GetComponent<Sticky>().stucks[i].connectedBody)){
                    Destroy(ts.GetComponent<Sticky>().stucks[i]);
                    ts.GetComponent<Sticky>().stucks.Remove(ts.GetComponent<Sticky>().stucks[i]);
                }
        for (int i = tipStucks.Count - 1; i >= 0; i--)
                if(free.Contains(tipStucks[i].connectedBody)){
                    Destroy(tipStucks[i]);
                    tipStucks.Remove(tipStucks[i]);
                }
    }
    void OnTriggerExit2D(Collider2D oColid) {
        if(free.Contains(oColid.attachedRigidbody))
            free.Remove(oColid.attachedRigidbody);
    }
}
