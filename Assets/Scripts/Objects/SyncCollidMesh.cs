using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class SyncCollidMesh : MonoBehaviour
{
    private Transform parts;
    private Mesh mesh;
    private Vector3[] vertices;

    // Start is called before the first frame update
    void Start(){
        parts = transform.GetChild(0);
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
    }

    // Update is called once per frame
    void Update(){
        for(int i = 0; i < parts.childCount; i++){
            vertices[i] = parts.GetChild(i).localPosition;
            // Vector2 backThirdDrirect = new Vector2(Mathf.Cos(Mathf.Deg2Rad*(parts.GetChild(i).localEulerAngles.z+60)),Mathf.Sin(Mathf.Deg2Rad*(parts.GetChild(i).localEulerAngles.z+60)));
            // Debug.DrawRay(parts.GetChild(i).position, backThirdDrirect, Color.red);
        }
        mesh.vertices = vertices;
    }
}
