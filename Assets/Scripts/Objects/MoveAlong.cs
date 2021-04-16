using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAlong : MonoBehaviour
{
    private Transform target;
    public string animationName = "Auto";
    [SerializeField] private GameObject trigerer;
    [SerializeField] private GameObject toMove;
    Animator animator;

    void Start(){
        target = transform.GetChild(0);
        animator = toMove.GetComponent<Animator>();
    }
    
    void OnTriggerEnter2D(Collider2D collid){
        if(collid.gameObject == trigerer){
            MoveBish mb = toMove.GetComponent<MoveBish>();
            if(animationName == "Auto"){
                if(transform.parent.childCount != transform.GetSiblingIndex()+1){
                    Transform nextMove = transform.parent.GetChild(transform.GetSiblingIndex()+1);
                    mb.Move(target.position,()=>{
                        mb.Move(nextMove.position);
                    });
                    return;
                }
                else{
                    mb.Move(target.position);
                }
            }
            mb.Move(target.position,()=>{
                animator.enabled = true;
                animator.Play(animationName);
            });
        }
    }
}
