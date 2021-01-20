using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Moments;
using System;
using UnityEngine.InputSystem;

public class GenCons : MonoBehaviour
{
    private string m_LastFile = "";
    private Recorder m_Recorder;
    private double frameCount = 0;
    private float frameInterval = 1999;
    private bool run = false;
    // private InValWrap tru = new InValWrap(){isPressed = true};
    // private InValWrap fal = new InValWrap();
    public void OnMenu(){ SceneManager.LoadScene("MainMenu"); }
    public void OnRecord(){
        m_Recorder.Save();
        Commons.Instance.notify("gif saved");
    }
    void OnFileSaved(int id, string filepath){
		// Our file has successfully been compressed & written to disk !
		m_LastFile = filepath;

		// m_IsSaving = false;

		// Let's start recording again (note that we could do that as soon as pre-processing
		// is done and actually save multiple gifs at once, see OnProcessingDone().
		m_Recorder.Record();
	}
    // Start is called before the first frame update
    void Start(){
        m_Recorder = Camera.main.GetComponent<Recorder>();
        if (Application.platform == RuntimePlatform.WindowsPlayer){
            m_Recorder.SaveFolder = Application.dataPath;
            // m_Recorder.SaveFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)+ "/JumpyBoi";
        }
        // Debug.Log(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        m_Recorder.Record();
        m_Recorder.OnFileSaved = OnFileSaved;
    }
    public void OnTest(){
        Commons.Instance.notify(""+(Math.Ceiling(frameCount/frameInterval)*frameInterval));
        run = !run;
        // Debug.Log("");
    }

    // Update is called once per frame
    void Update(){
        // a setup that calls a function every frameInterval frames
        // the function that gets called must be modded to accept the calls first
        // frameCount++;
        // // Debug.Log(frameCount);
        // if(run){
        //      
        // }
    }
}
