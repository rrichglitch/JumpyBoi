using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeechManager : MonoBehaviour
{
    private Transform SpeechBubble;
    private Animator speechAnim;
    private TextMeshProUGUI tmp;
    private SpeechLines curLines;
    private int speechState = 0;
    private int speechInState = 0;
    private int speechEnd = 0;
    public delegate void Callback();
    private Callback cb;
    // Start is called before the first frame update
    void Start()
    {
        speechAnim =  GetComponent<Animator>();
        tmp = transform.Find("Canvas").GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update(){
        if(speechState < speechEnd && speechState >= speechInState){
            StartCoroutine(Say(curLines.lines[speechState].words, curLines.lines[speechState].length));
        }
    }

    public void StartSpeech(SpeechLines sl, Callback callback = null){
        speechState = 0;
        speechInState = 0;
        curLines = sl;
        speechEnd = sl.lines.Count;
        cb = callback;
    }

    public IEnumerator Say(string words, float displayTime = 3){
        speechInState++;
        tmp.text = words;
        speechAnim.Play("SpeechOut");
        yield return new WaitForSeconds(displayTime);
        speechAnim.Play("SpeechIn");
    }

    public void finishLine(){
        speechState++;
        if(speechState >= speechEnd){
            if(cb != null) cb();
        }
    }
}
