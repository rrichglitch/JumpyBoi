using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overlap : MonoBehaviour
{
    private Collider2D[] overs;
    private Vector2 bounds;
    // Start is called before the first frame update
    void Start()
    {
        float bound = GetComponent<SpriteRenderer>().bounds.size.x;
        bounds = new Vector2(bound/2,bound/2);
    }

    // check if this object is overlapping anothers collider
    public bool CheckOverlap()
    {
        overs = Physics2D.OverlapAreaAll((Vector2)transform.position - bounds, (Vector2)transform.position + bounds, LayerMask.GetMask("Default"));
        return (overs.Length > 0);
    }
}
