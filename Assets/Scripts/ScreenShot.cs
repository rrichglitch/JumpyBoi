using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
// using System.Drawing;

public class ScreenShot : MonoBehaviour
{
    public int backLogSeconds;
    public int fps;
    private Texture2D tex;
    private List<Texture2D> frames = new List<Texture2D>();
    // Start is called before the first frame update
    void Start(){
        
    }

    // Update is called once per frame
    void Update(){
        StartCoroutine(savePNG());
    }
    IEnumerator savePNG(){
        // We should only read the screen after all rendering is complete
        yield return new WaitForEndOfFrame();
        // Create a texture the size of the screen, RGB24 format
        tex = new Texture2D( Screen.width, Screen.height, TextureFormat.RGB24, false );
        // Read screen contents into the texture
        tex.ReadPixels( new Rect(0, 0, Screen.width, Screen.height), 0, 0 );
        tex.Apply();
        // Encode texture into PNG and add it to the cache
        // frames.Add(tex);
    }

    void OnRecord(){
        File.WriteAllBytes("foo.png",tex.EncodeToPNG());
    }
}
