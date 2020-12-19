using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CommonTools : MonoBehaviour
{
    public static Transform getLead(int TeamIndex = 2)
    {
        Transform current;
        GameObject[] root = SceneManager.GetActiveScene().GetRootGameObjects();
        if(root[TeamIndex].transform.childCount > 0){
            for (int i = root[TeamIndex].transform.childCount-1; i >= 0 ; i--){
                current = root[TeamIndex].transform.GetChild(i);
                if(current.gameObject.activeSelf == true){
                    if(current.name != "Tail" || current.name != "Head")
                        return current;
                }
            }
        }
        return null;
    }
    public static Transform getButt(int TeamIndex = 2)
    {
        Transform current;
        GameObject[] root = SceneManager.GetActiveScene().GetRootGameObjects();
        if(root[TeamIndex].transform.childCount > 0){
            for (int i = 0; i < root[TeamIndex].transform.childCount ; i++){
                current = root[TeamIndex].transform.GetChild(i);
                if(current.gameObject.activeSelf == true){
                    if(current.name != "Tail" && current.name != "Head")
                        return current;
                }
            }
        }
        return null;
    }
}
