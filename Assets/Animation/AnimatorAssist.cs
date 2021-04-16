using UnityEngine;

public class AnimatorAssist : MonoBehaviour
{
    private Animator animator;
    void Start(){
        animator = GetComponent<Animator>();
    }
    public void AnimExit(){
        animator.enabled = false;
    }
}
