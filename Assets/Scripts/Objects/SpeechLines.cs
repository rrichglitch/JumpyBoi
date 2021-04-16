using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LineAndLength{
    public string words = "";
    public float length = 5;
}

public class SpeechLines : MonoBehaviour
{
    public string speechTopic = "";
    public List<LineAndLength> lines = new List<LineAndLength>();
}
