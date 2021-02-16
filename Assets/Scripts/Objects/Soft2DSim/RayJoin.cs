using System.Collections.Generic;
using UnityEngine;

//this is a small class just to help Soft2DSim stay in sync with the monobehaviours of parts it makes
//it uses raycasts to join parts of the skin that are across from eachother
[ExecuteInEditMode]
public class RayJoin : MonoBehaviour
{
    private bool setup = false;
    private bool clockwise;
    private List<int> exclude;
    private float dist;
    private float springStrength;
    private float bounceDampening;
    void Awake(){
        Update();
    }
    public void Setup(bool _clockwise, List<int> _exclude, float _dist, float _springStrength, float _bounceDampening){

        clockwise = _clockwise;
        exclude = _exclude;
        dist = _dist;
        springStrength = _springStrength;
        bounceDampening = _bounceDampening;

        setup = true;
    }

    //actually carry out the purpose of this class
    void Update(){
        if(setup){
            List<GameObject> siblings = new List<GameObject>();
            foreach(Transform child in transform.parent)
                if(child != transform.parent)
                    siblings.Add(child.gameObject);

            Collider2D curCollid = GetComponent<Collider2D>();
            Vector2 backAnchor;
            Vector2 frontAnchor;
            Vector2 backThirdDrirect;
            Vector2 frontThirdDrirect;
            int resultLen;
            RaycastHit2D[] results = new RaycastHit2D[10];
            Collider2D beamedCollider = null;
            AnchoredJoint2D curJoint;

            Vector2 transCenter = transform.InverseTransformPoint(curCollid.bounds.center);
            Vector2 transExtents = transform.InverseTransformVector(curCollid.bounds.extents);
            backAnchor = new Vector2(transCenter.x-(transExtents.x*.9F), transCenter.y);
            frontAnchor = new Vector2(transCenter.x+(transExtents.x*.9F), transCenter.y);
            float rot = transform.eulerAngles.z;

            if(clockwise){
                backThirdDrirect = new Vector2(Mathf.Cos(Mathf.Deg2Rad*(rot+240)),Mathf.Sin(Mathf.Deg2Rad*(rot+240)));
                frontThirdDrirect = new Vector2(Mathf.Cos(Mathf.Deg2Rad*(rot+300)),Mathf.Sin(Mathf.Deg2Rad*(rot+300)));
            }
            else{
                frontThirdDrirect = new Vector2(Mathf.Cos(Mathf.Deg2Rad*(rot+60)),Mathf.Sin(Mathf.Deg2Rad*(rot+60)));
                backThirdDrirect = new Vector2(Mathf.Cos(Mathf.Deg2Rad*(rot+120)),Mathf.Sin(Mathf.Deg2Rad*(rot+120)));
            }

            resultLen = curCollid.Raycast(backThirdDrirect, results, dist);
            // Debug.Log("resultLen for "+name+" is: "+resultLen);
            Collider2D findBeamedCollid(){
                for(int a = 0; a < resultLen; a++){
                    Collider2D ret;
                    if(results[a].collider != null){
                        int childSpot = siblings.IndexOf(results[a].collider.gameObject);//changed to work with list
                        if(childSpot >= 0)
                            if(!exclude.Contains(childSpot)){
                                // Debug.Log(name+" points at "+results[a].collider.name);
                                setup = false;
                                ret = results[a].collider;
                                results = new RaycastHit2D[10];
                                return ret;
                            }
                    }
                }
                // Debug.Log("no good hits??");
                results = new RaycastHit2D[10];
                return null;
            }
            beamedCollider = findBeamedCollid();

            if(beamedCollider != null){
                curJoint = gameObject.AddComponent<SpringJoint2D>();
                ((SpringJoint2D)curJoint).dampingRatio = bounceDampening;
                ((SpringJoint2D)curJoint).frequency = springStrength;
                curJoint.connectedBody = beamedCollider.gameObject.GetComponent<Rigidbody2D>();
                curJoint.anchor = transform.localEulerAngles.z < 180 ? backAnchor : frontAnchor;
                curJoint.connectedAnchor = beamedCollider.transform.InverseTransformPoint(beamedCollider.bounds.center);
            }
            beamedCollider = null;

            resultLen = curCollid.Raycast(frontThirdDrirect, results, dist);
            beamedCollider = findBeamedCollid();

            if(beamedCollider != null){
                curJoint = gameObject.AddComponent<SpringJoint2D>();
                ((SpringJoint2D)curJoint).dampingRatio = bounceDampening;
                ((SpringJoint2D)curJoint).frequency = springStrength;
                curJoint.connectedBody = beamedCollider.gameObject.GetComponent<Rigidbody2D>();
                curJoint.anchor = transform.localEulerAngles.z < 180 ? frontAnchor : backAnchor;
                curJoint.connectedAnchor = beamedCollider.transform.InverseTransformPoint(beamedCollider.bounds.center);
            }
            beamedCollider = null;

            // Debug.DrawRay(transform.position, backThirdDrirect, Color.red);
            DestroyImmediate(this);
        }
    }
}
