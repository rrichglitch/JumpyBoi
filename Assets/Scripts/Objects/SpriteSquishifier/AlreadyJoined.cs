using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AlreadyJoined : MonoBehaviour
{
    public List<int> list = new List<int>();
    void Update(){ DestroyImmediate(this); }
}
