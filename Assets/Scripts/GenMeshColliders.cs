using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GenMeshColliders : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material mat;
    [SerializeField] private string Name = "_";
    [SerializeField] private float partMass = 1;
    [SerializeField] private float springStrength = 3;
    [SerializeField] private float bounceDampening = .9F;
    [SerializeField] private float vertebraeMass = 1;
    [SerializeField] private bool solidSpine = true;
    [SerializeField] private float spineStrength = 5;
    [SerializeField] private float spineDampening = .9F;
    [SerializeField] private bool useSquareColliders = true;
    [SerializeField] private float squarEdgeRad = .1F;
    
    // void Awake(){
    //     if (Application.isEditor && !Application.isPlaying){
    //         GenerateColliders();
    //     }
    // }

    [ContextMenu("Generate Colliders")]
    void GenerateColliders(){
        if(mesh != null){

            Vector3[] vertices = mesh.vertices;

            Vector2 meshDims = mesh.bounds.size;

            //these will need to be specially calculated
            float collidWidth;
            float collidHeight;
            
            bool tall;
            float tsRatio;
            int tsRoundRat;

            tall = meshDims.x <= meshDims.y;

            if(tall) tsRatio = meshDims.y / meshDims.x;
            else tsRatio = meshDims.x / meshDims.y;

            tsRoundRat = Mathf.RoundToInt(tsRatio);

            int vertebraeCount = tsRoundRat-1;
            int xVertCount;
            int yVertCount;

            if(tall){
                xVertCount = ((vertices.Length-vertebraeCount)/2)/(tsRoundRat+1);
                yVertCount = xVertCount*tsRoundRat;
            }
            else{
                yVertCount = ((vertices.Length-vertebraeCount)/2)/(tsRoundRat+1);
                xVertCount = yVertCount*tsRoundRat;
            }

            collidWidth = meshDims.x/xVertCount;
            collidHeight = meshDims.y/yVertCount;
            Debug.Log(collidWidth+", "+collidHeight);



            GameObject container = new GameObject(Name, typeof(Rigidbody2D));
            GameObject picture = new GameObject("picture", typeof(MeshFilter), typeof(MeshRenderer), typeof(SyncCollidMesh));
            picture.GetComponent<MeshFilter>().mesh = mesh;
            if(mat != null) picture.GetComponent<MeshRenderer>().material = mat;
            picture.transform.parent = container.transform;
            picture.transform.localPosition = new Vector3(meshDims.x/-2, meshDims.y/-2, 1);

            GameObject scaler = new GameObject("parts");
            scaler.transform.parent = picture.transform;

            float xScale = meshDims.x/(meshDims.x + collidWidth);
            float yScale = meshDims.y/(meshDims.y + collidHeight);
            scaler.transform.localScale = new Vector3(xScale, yScale, 1);
            
            scaler.transform.localPosition = new Vector3((collidWidth/2)*xScale, (collidHeight/2)*yScale);

            List<GameObject> children = new List<GameObject>();
            GameObject curChild;
            Collider2D curCollid;
            Joint2D curJoint;

            int third = Mathf.RoundToInt((vertices.Length-vertebraeCount)/3);
            
            // Vector2 normNextDirect;
            // Vector2 surfNormal;

            //create the collider objects for vertebrae
            for(int i = 0; i < vertebraeCount; i++){
                curChild = new GameObject("part "+i, typeof(Rigidbody2D));
                curChild.transform.parent = scaler.transform;
                curChild.transform.localScale = new Vector3(1,1,1);
                children.Add(curChild);
                curChild.transform.localPosition = vertices[i];
                curChild.GetComponent<Rigidbody2D>().mass = vertebraeMass;
                curCollid = curChild.AddComponent<CircleCollider2D>();
                ((CircleCollider2D)curCollid).radius = collidWidth/2;

                //configure joints
                curJoint = curChild.AddComponent<FixedJoint2D>();
                curJoint.connectedBody = container.GetComponent<Rigidbody2D>();
                if(i>0){
                    if(solidSpine)
                        curJoint = curChild.AddComponent<FixedJoint2D>();
                    else{
                        curJoint = curChild.AddComponent<SpringJoint2D>();
                        ((SpringJoint2D)curJoint).dampingRatio = spineDampening;
                        ((SpringJoint2D)curJoint).frequency = spineStrength;
                    }
                    curJoint.connectedBody = children[i-1].GetComponent<Rigidbody2D>();
                }

            }

            float anchorMod = .75F;
            Vector2 diff;
            Vector2 lDirect;

            //create the collider objects for the surface
            for(int i = vertebraeCount; i < vertices.Length; i++){
                curChild = new GameObject("part "+i, typeof(Rigidbody2D));
                curChild.transform.parent = scaler.transform;
                curChild.transform.localScale = new Vector3(1,1,1);
                children.Add(curChild);
                curChild.transform.localPosition = vertices[i];
                curChild.GetComponent<Rigidbody2D>().mass = partMass;
                if(useSquareColliders){
                    curCollid = curChild.AddComponent<BoxCollider2D>();
                    ((BoxCollider2D)curCollid).size = new Vector2(collidWidth-(squarEdgeRad*2), collidHeight-(squarEdgeRad*2));
                    ((BoxCollider2D)curCollid).edgeRadius = squarEdgeRad;
                }
                else{
                    curCollid = curChild.AddComponent<CircleCollider2D>();
                    ((CircleCollider2D)curCollid).radius = collidWidth/2;
                }

                //find the closest vertebrae to attach to
                int closest = 0;
                for(int a = 1; a < vertebraeCount; a++)
                    if(Vector2.Distance(vertices[i], vertices[a]) < Vector2.Distance(vertices[i], vertices[closest]))
                        closest = a;

                //configure joints
                //connect curChild to the spine
                curJoint = curChild.AddComponent<SpringJoint2D>();
                curJoint.connectedBody = children[closest].GetComponent<Rigidbody2D>();
                ((SpringJoint2D)curJoint).dampingRatio = bounceDampening;
                ((SpringJoint2D)curJoint).frequency = springStrength;

                //make "skin" so colliders dont fall out
                if(i > vertebraeCount){
                    //find the direction of the last skin collider
                    diff = (vertices[i-1] - vertices[i]);
                    lDirect = diff/diff.magnitude;

                    curJoint = curChild.AddComponent<DistanceJoint2D>();
                    curJoint.connectedBody = children[i-1].GetComponent<Rigidbody2D>();
                    ((DistanceJoint2D)curJoint).anchor = lDirect*((diff.magnitude/2)*anchorMod);
                    ((DistanceJoint2D)curJoint).connectedAnchor = lDirect*((diff.magnitude/2)*-anchorMod);

                    if(i == vertices.Length-1){
                        diff = (vertices[vertebraeCount] - vertices[i]);
                        lDirect = diff/diff.magnitude;

                        curJoint = curChild.AddComponent<DistanceJoint2D>();
                        curJoint.connectedBody = children[vertebraeCount].GetComponent<Rigidbody2D>();
                        ((DistanceJoint2D)curJoint).anchor = lDirect*((diff.magnitude/2)*anchorMod);
                        ((DistanceJoint2D)curJoint).connectedAnchor = lDirect*((diff.magnitude/2)*-anchorMod);
                    }
                }

                //connect curChild to the skin collider that is 2 behind
                if(i > vertebraeCount+1){
                    curJoint = curChild.AddComponent<SpringJoint2D>();
                    ((SpringJoint2D)curJoint).dampingRatio = bounceDampening;
                    ((SpringJoint2D)curJoint).frequency = springStrength;
                    curJoint.connectedBody = children[i-2].GetComponent<Rigidbody2D>();

                    if(i >= vertices.Length-2){
                        curJoint = curChild.AddComponent<SpringJoint2D>();
                        ((SpringJoint2D)curJoint).dampingRatio = bounceDampening;
                        ((SpringJoint2D)curJoint).frequency = springStrength;
                        if(i == vertices.Length-2)
                            curJoint.connectedBody = children[vertebraeCount].GetComponent<Rigidbody2D>();
                        else
                            curJoint.connectedBody = children[vertebraeCount+1].GetComponent<Rigidbody2D>();
                    }
                }
            }

            //loop through the vertices again to finish configuring joints
            for(int i = vertebraeCount; i < vertices.Length; i++){

                curJoint = children[i].AddComponent<SpringJoint2D>();
                ((SpringJoint2D)curJoint).dampingRatio = bounceDampening;
                ((SpringJoint2D)curJoint).frequency = springStrength;
                if(i - third < vertebraeCount){
                    curJoint.connectedBody = children[vertices.Length-1+(i-third-vertebraeCount)].GetComponent<Rigidbody2D>();
                }
                else curJoint.connectedBody = children[i - third].GetComponent<Rigidbody2D>();

                curJoint = children[i].AddComponent<SpringJoint2D>();
                ((SpringJoint2D)curJoint).dampingRatio = bounceDampening;
                ((SpringJoint2D)curJoint).frequency = springStrength;
                if(i + third > vertices.Length-1){
                    curJoint.connectedBody = children[vertebraeCount+((i + third) - vertices.Length-1)].GetComponent<Rigidbody2D>();
                }
                else curJoint.connectedBody = children[i + third].GetComponent<Rigidbody2D>();
            }
        }
    }
}
