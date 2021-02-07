using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class SyncCollidMesh : MonoBehaviour
{
    private Mesh mesh;
    private Transform parts;

    // Start is called before the first frame update
    void Start(){
        mesh = GetComponent<MeshFilter>().mesh;
        parts = transform.GetChild(0);
    }

    // Update is called once per frame
    void Update(){
        Vector3[] vertices = mesh.vertices;
        for(int i = 0; i < parts.childCount; i++){
            vertices[i] = parts.GetChild(i).localPosition;
        }
        mesh.vertices = vertices;
    }
}
