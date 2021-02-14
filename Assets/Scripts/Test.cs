using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Start(){
    }
    void Update(){
        
    }

    [ContextMenu("run test")]
    void runTest(){
        CircleCollider2D ogCollid = GetComponent<CircleCollider2D>();
        GameObject bro = new GameObject("bro", typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(RayJoin));
        bro.transform.position = transform.position;
        bro.transform.parent = transform.parent;
        CircleCollider2D outerCircle = bro.GetComponent<CircleCollider2D>();
        outerCircle.radius = ogCollid.radius+.5F;

        // Debug.Log(transform.localEulerAngles.z);
        // Vector2 direct = new Vector2(Mathf.Cos(Mathf.Deg2Rad*(240)),Mathf.Sin(Mathf.Deg2Rad*(240)));
        // Vector2 direct = new Vector2(Mathf.Cos(Mathf.Deg2Rad*(transform.localEulerAngles.z+240)),Mathf.Sin(Mathf.Deg2Rad*(transform.localEulerAngles.z+240)));
        // Debug.DrawRay(bro.transform.position, direct, Color.red);
        // RaycastHit2D[] results = new RaycastHit2D[10];
        // int hits = outerCircle.Raycast(direct, results, 1);
        // for(int i = 0; i < hits; i++)
        //     if(results[i].collider != null)
        //         Debug.Log(results[i].collider.name);
        // results = new RaycastHit2D[10];
    }

}
