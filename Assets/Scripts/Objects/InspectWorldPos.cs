using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InspectWorldPos : MonoBehaviour
{
    #if UNITY_EDITOR
        [SerializeField] private Vector3 worldPos;
        [SerializeField] private Transform tran;
        [SerializeField] private Vector3 localToTran;
        void Update(){
            worldPos = transform.position;
            if(tran != null) localToTran = tran.InverseTransformPoint(worldPos);
        }
    #endif
}
