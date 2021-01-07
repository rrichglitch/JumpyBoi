using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("start");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("update");
        
    }
    void FixedUpdate(){Debug.Log("fixed");}
    void LateUpdate(){Debug.Log("late");}
}
