using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Spawning : MonoBehaviour
{
    public int TeamIndex;
    public GameObject inFront;
    public DistanceJoint2D DJ2D;
    private PlayerInputManager PIM;
    void Awake(){
        GameObject[] root = SceneManager.GetActiveScene().GetRootGameObjects();
        PIM = PlayerInputManager.instance;
        int pc = PIM.playerCount;
        DJ2D = GetComponent<DistanceJoint2D>();
        if(pc > 1){
            for (int i = root[TeamIndex].transform.childCount-1; i >= 0 ; i--){
                if(root[TeamIndex].transform.GetChild(i).gameObject.activeSelf == true){
                        inFront = root[TeamIndex].transform.GetChild(i).gameObject;
                        break;
                }
            }
        }
        if(root[TeamIndex].name != name){
            transform.parent = root[TeamIndex].transform;
            Vector3 pPos = root[TeamIndex].transform.position;
            transform.position = new Vector3(pPos.x+(float)-.8*(pc-1), pPos.y+(float)-1.5, pPos.z);
            if(inFront){
                DJ2D.connectedBody = inFront.GetComponent<Rigidbody2D>();
                DJ2D.enabled = true;
            }
        }
    }
}