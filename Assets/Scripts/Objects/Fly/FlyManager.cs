using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyManager : MonoBehaviour
{
    public SpeechManager sm;
    private SpeechLines cheeseLines;
    private Animator bodAnim;
    public List<SpeechLines> dodgeLines = new List<SpeechLines>();

    // Start is called before the first frame update
    void Start()
    {
        SpeechLines[] speeches = GetComponents<SpeechLines>();
        foreach(SpeechLines sl in speeches){
            if(sl.speechTopic == "Cheese0"){
                cheeseLines = sl;
                break;
            }
        }
        bodAnim = transform.GetChild(0).GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update(){

    }

    [ContextMenu("Cheese")]
    public void Cheese(){
        transform.GetChild(0).GetComponent<MoveBish>().Move(transform.position,()=>{
            bodAnim.Play("Still");
            sm.StartSpeech(cheeseLines,() => { bodAnim.Play("Sway"); } );
        });
    }

    public void Dodge(){
        if(dodgeLines.Count > 0){
            sm.StartSpeech(dodgeLines[Random.Range(0,dodgeLines.Count)]);
        }
    }
}
