using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class SoftSprite : MonoBehaviour
{
    [SerializeField] private SpriteShapeController spriteShape;
    private Spline spline;
    [SerializeField] private Transform[] points;
    [SerializeField] private bool box = false;

    void Start(){
        spline = spriteShape.spline;
        spriteShape.transform.position = points[0].parent.position;
        Update();
    }

    void Update(){
        Vector2 vert;
        Vector2 direct;
        Vector2 tan;
        float dist;
        for(int i = 0; i<points.Length; i++){

            vert = points[i].localPosition;
            direct = vert.normalized;

            if(box) dist = points[i].GetComponent<BoxCollider2D>().size.x;
            else dist = points[i].GetComponent<CircleCollider2D>().radius;

            spline.SetPosition(i, (Vector3)(vert+(direct*dist)));

            tan = Vector2.Perpendicular(direct)*spline.GetLeftTangent(i).magnitude;

            spline.SetLeftTangent(i, tan);
            spline.SetRightTangent(i, -tan);
        }
    }
}