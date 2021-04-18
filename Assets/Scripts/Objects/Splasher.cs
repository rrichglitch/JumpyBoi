using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splasher : MonoBehaviour
{
    [SerializeField] private float spawnHeight = 1;
    private float sizeFactor = .8F;
    private float speedFactor = .05F;
    private float shGlobal;
    [SerializeField] private GameObject prefab;
    // Start is called before the first frame update
    void Start(){
        shGlobal = transform.TransformPoint(new Vector2(0,spawnHeight)).y;
    }

    void OnTriggerEnter2D(Collider2D collid){
        GameObject nSplash = Instantiate(prefab, new Vector2(collid.transform.position.x, shGlobal), Quaternion.identity);
        nSplash.transform.parent = transform;

        //calculate the rough size of the entering collider using its bounding box
        float size = collid.bounds.size.x *  collid.bounds.size.y;
        //scale the splash by the size and speed of the entering collider
        nSplash.transform.localScale *= size*sizeFactor;

        Rigidbody2D rb = collid.GetComponent<Rigidbody2D>();
        if(rb != null) nSplash.transform.localScale *= rb.velocity.magnitude * speedFactor;
    }
}
