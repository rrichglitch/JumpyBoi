using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InteractByLayer : MonoBehaviour
{
    public int groundLayer = 20;
    // Start is called before the first frame update
    void Start(){
        Collider2D collid = GetComponent<Collider2D>();
        if(collid != null){
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if(sr != null){
                collid.enabled = sr.sortingOrder > groundLayer;
            }
        }
        
        if(Application.isEditor) DestroyImmediate(this);
        else Destroy(this);
    }
}
