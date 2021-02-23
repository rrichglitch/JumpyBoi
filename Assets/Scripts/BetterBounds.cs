using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetterBounds
{
    public GameObject holder{ get; }
    public Bounds _old{ get; }

    public BetterBounds(GameObject go, Bounds bounds){

        holder = go;
        _old = bounds;
    }

    
}
