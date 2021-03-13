using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Commons : Singleton<Commons>
{
    // public static float weak;
    public float weak;
    public float buttl;
    public GameObject body;
    public static bool useTongue = true;
    [SerializeField] private GameObject notifation;
    [SerializeField] private GameObject dot_prefab;
    private Animator anim;
    private TMP_Text notiText;
    void Start(){
        anim = notifation.GetComponent<Animator>();
        notiText = notifation.transform.GetChild(0).GetComponent<TMP_Text>();
    }

    //sends a notification to the screen in-game
    public void notify(string text){
        notifation.SetActive(true);
        notiText.text = text;
        anim.SetTrigger("pop");
    }

    //methods to get invoked by this should return a value so it can be listened for by the invoker, otherwise it may be assumed the operation has failed
    public static object mSendMessage(GameObject receiver, string methodName, object[] value = null){
        object toRet = null;
        Component[] listens = receiver.GetComponents(typeof(Component));
        // Debug.Log("list: "+ listens);
        foreach(Component c in listens){
            // if(c is Listener){
                MethodInfo meth = c.GetType().GetMethod(methodName, new Type[]{typeof(object[])});
                if(meth != null){
                    //run and save the return of the function
                    object save = meth.Invoke(c, new object[]{value});
                    if(save != null) toRet = save;
                }
            // }
        }
        return toRet;
    }

    //get the total bounds of an object and its children in world space
    //super useful for scaling purposes
    //pass a second parameter of true to base the bounds on colliders instead of renderers
    static public Bounds GetWholeBounds(GameObject g, bool colliderBased = false){
        Bounds b;
        if(colliderBased){
            Collider2D[] collids = g.GetComponentsInChildren<Collider2D>();
            if(collids.Length > 0){
                b = new Bounds(collids[0].bounds.center, Vector3.zero);
                foreach (Collider2D c in collids){
                    b.Encapsulate(c.bounds);
                }
                return b;
            }
        }
        else{
            Renderer[] renders = g.GetComponentsInChildren<Renderer>();
            if(renders.Length > 0){
                b = new Bounds(renders[0].bounds.center, Vector3.zero);
                foreach (Renderer r in renders){
                    b.Encapsulate(r.bounds);
                }
                return b;
            }
        }

        return new Bounds(Vector3.zero, Vector3.zero);
    }

    //a method to get the an element the appropriate count away from an index while excluding certain indices
    //pretty hard to think about imo
    static public int GetValidIndex(int totalLength, int startInd, int endDist){
        int dir = endDist/Mathf.Abs(endDist);
        int ret = startInd;
        for(int skinCount = 0; skinCount != endDist && Mathf.Abs(skinCount) < totalLength-1; skinCount += dir){
            ret = (ret + dir)%totalLength;
            if(ret + dir < 0)
                ret = totalLength+dir;
        }
        return ret;
    }
    static public int GetValidIndex(int totalLength, int startInd, int endDist, List<int> exlusions){
        int dir = endDist/Mathf.Abs(endDist);
        int ret = startInd;
        int skinCount = 0;
        //account for starting from an invalid index
        while(exlusions.Contains(ret) && Mathf.Abs(ret) < totalLength){
            if(ret + dir < 0)
                ret = totalLength+dir;
            else ret = (ret + dir)%totalLength;
            skinCount = dir;
        }

        for(; skinCount != endDist && Mathf.Abs(skinCount) < totalLength-1; skinCount += dir){
            if(ret + dir < 0)
                ret = totalLength+dir;
            else ret = (ret + dir)%totalLength; 
            while(exlusions.Contains(ret) && Mathf.Abs(ret) < totalLength){
                if(ret + dir < 0)
                    ret = totalLength+dir;
                else ret = (ret + dir)%totalLength;
            }
        }
        return ret;
    }

    //Is a triangle in 2d space oriented clockwise or counter-clockwise
    //https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
    //https://en.wikipedia.org/wiki/Curve_orientation
    public static bool IsTriangleClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        bool isClockWise = true;

        float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

        if (determinant > 0f)
        {
            isClockWise = false;
        }

        return isClockWise;
    }

    public static T[] insertInArray<T>(T[] arr, int index, T toInsert){
        for(int i = arr.Length-1; i > index; i--){
            arr[i] = arr[i-1];
        }
        arr[index] = toInsert;
        return arr;
    }

    public static bool IsInside(Collider c, Vector3 point){
        Vector3 closest = c.ClosestPoint(point);
        // Because closest=point if point is inside - not clear from docs I feel
        return closest == point;
    }

    //more erroneous the more un-spherical a collider is
    //there seems to be some kind of weirdness with closestPoints in and out coordinates in certain conditions
    public static bool collidersIntersect(Collider2D collidA, Collider2D collidB, bool drawDots = false){
        Vector2 bCloseToA = collidB.ClosestPoint(collidA.transform.TransformPoint(collidA.offset));
        Vector2 aCloseToB = collidA.ClosestPoint(bCloseToA);
        if(drawDots){
            Instance.spawnDot(aCloseToB);
            Instance.spawnDot(bCloseToA);
        }
        bCloseToA = collidB.ClosestPoint(aCloseToB);
        if(aCloseToB == bCloseToA) return true;
        //switch the colliders names so I can just copy and paste the above
        Collider2D save = collidA;
        collidA = collidB;
        collidB = save;
        bCloseToA = collidB.ClosestPoint(collidA.transform.TransformPoint(collidA.offset));
        aCloseToB = collidA.ClosestPoint(bCloseToA);
        if(drawDots){
            Instance.spawnDot(aCloseToB);
            Instance.spawnDot(bCloseToA);
        }
        bCloseToA = collidB.ClosestPoint(aCloseToB);
        return aCloseToB == bCloseToA;
    }

    public void spawnDot(Vector3 location){
        Instantiate(dot_prefab, location, Quaternion.identity);
    }
}
// public interface Listener{}