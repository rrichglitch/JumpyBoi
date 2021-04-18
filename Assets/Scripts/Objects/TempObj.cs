using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempObj : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3;
    // Start is called before the first frame update
    void Start(){
        if(Application.isPlaying){
            StartCoroutine(SelfDestruct());
        }
    }
    IEnumerator SelfDestruct(){
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
}
