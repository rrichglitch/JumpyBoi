using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraAnimControl : MonoBehaviour
{
    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = transform.parent.GetComponent<Animator>();
    }

    [ContextMenu("Play")]
    void Play(){
        anim.Play("MoveDozer");
        anim.Play("SpinWheels");
    }
    void OnTriggerEnter2D(Collider2D oCollid){
        if(oCollid.CompareTag("Player")){
            Play();
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
