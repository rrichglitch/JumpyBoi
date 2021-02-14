using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class NGenMeshColliders : MonoBehaviour
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material mat;
    [SerializeField] private string Name = "_";
    [SerializeField] private bool internalFriction = false;
    [SerializeField] private float frictionStrength = 1;
    [SerializeField] private float partMass = 1;
    [SerializeField] private float springStrength = 3;
    [SerializeField] private float bounceDampening = .9F;
    [SerializeField] private bool solidSpine = true;
    [SerializeField] private float vertebraeMass = 1;
    [SerializeField] private float spineStrength = 5;
    [SerializeField] private float spineDampening = .9F;
    [SerializeField] private int consideredIn = 4;
    [SerializeField] private bool useSquareColliders = true;
    [SerializeField] private float skinThickness = .3F;
    [SerializeField] private PhysicsMaterial2D physMat;
    
    [ContextMenu("Generate Colliders")]
    void GenerateColliders(){
        float squarEdgeRad = skinThickness/4;
        if(mesh is null && sprite is null){
            Debug.Log("mesh is null!");
        }
        else{
            Vector3[] vertices;
            int[] triangles;
            Vector2 picDims;

            //if a sprite has been set then override the mesh with the sprite data
            if(sprite != null){
                vertices = new Vector3[sprite.vertices.Length];
                for(int i = 0; i < sprite.vertices.Length; i++)
                    vertices[i] = sprite.vertices[i];
                triangles = new int[sprite.triangles.Length];
                for(int i = 0; i < sprite.triangles.Length; i++){
                    triangles[i] = sprite.triangles[i];}

                mesh = new Mesh();
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.uv = sprite.uv;
            }
            else{
                vertices = mesh.vertices;
                triangles = mesh.triangles;
            }
            picDims = mesh.bounds.size;
 
            //scan the triangles to find which vertices are inside the larger geometry(based on connection count)
            //O(n)
            List<int> getInners(){
                int[] includedIn = new int[vertices.Length];
                List<int> innerVertices  = new List<int>();
                List<int> sorted  = new List<int>();

                for(int i = 0; i < triangles.Length; i++){
                    includedIn[triangles[i]]++;
                    if(includedIn[triangles[i]] > consideredIn)
                        if(!innerVertices.Contains(triangles[i])){
                            innerVertices.Add(triangles[i]);}
                }
                foreach(int ind in innerVertices){
                    if(sorted.Count == 0) sorted.Add(ind);
                    else{
                        int insert = sorted.Count;
                        while(insert > 0 && sorted[insert-1] > ind){
                            insert--;
                        }
                        sorted.Insert(insert, ind);
                    }
                }
                return sorted;
            }
            List<int> innerVerts = getInners();

            GameObject container = new GameObject(Name, typeof(Rigidbody2D));
            GameObject picture;
            Bounds rederBounds;

            picture = new GameObject("picture", typeof(MeshFilter), typeof(MeshRenderer), typeof(SyncCollidMesh));
            picture.GetComponent<MeshFilter>().mesh = mesh;
            rederBounds = picture.GetComponent<MeshRenderer>().bounds;

            Shader shader = null;
            //scan for which rendering pipeline is in use to select a default shader
            if (GraphicsSettings.currentRenderPipeline){
                string currentPipe = GraphicsSettings.currentRenderPipeline.GetType().ToString();
                if (currentPipe.Contains("HighDefinition")){
                    shader = Shader.Find("High Definition Render Pipeline/2D/Sprite-Lit-Default");
                    if(shader == null) shader = Shader.Find("HD Render Pipeline/2D/Sprite-Lit-Default");
                }
                else if(currentPipe.Contains("URP") || currentPipe.Contains("Universal"))
                    shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
            }
            if(shader == null) shader = Shader.Find("Sprites/Default");
            
            //set the material and prefer the sprite
            //override the default shader if a material is set without a sprite
            if(sprite != null){
                mat = new Material(shader);
                mat.mainTexture = sprite.texture;
            }
            else if(mat == null)
                mat = new Material(shader);
            else
                shader = mat.shader;

            picture.GetComponent<MeshRenderer>().material = mat;
            picture.transform.parent = container.transform;

            GameObject scaler = new GameObject("parts");
            scaler.transform.parent = picture.transform;

            GameObject[] children = new GameObject[vertices.Length];
            GameObject curChild;
            Collider2D curCollid;
            AnchoredJoint2D curJoint;

            int third = Mathf.RoundToInt((vertices.Length-innerVerts.Count)/3);
            
            // Vector2 normNextDirect;
            // Vector2 surfNormal;

            //create the collider objects for vertebrae
            void createVertebrae(){
                for(int i = 0; i < innerVerts.Count; i++){
                    curChild = new GameObject("part "+innerVerts[i], typeof(Rigidbody2D));
                    curChild.transform.parent = scaler.transform;
                    curChild.transform.localScale = new Vector3(1,1,1);
                    children[innerVerts[i]] = curChild;
                    curChild.transform.localPosition = vertices[innerVerts[i]];
                    curChild.GetComponent<Rigidbody2D>().mass = vertebraeMass;
                    curCollid = curChild.AddComponent<CircleCollider2D>();
                    ((CircleCollider2D)curCollid).radius = skinThickness/2;

                    //configure joints
                    if(i==0){
                        curJoint = curChild.AddComponent<FixedJoint2D>();
                        curJoint.connectedBody = container.GetComponent<Rigidbody2D>();
                    }
                    else{
                        if(solidSpine)
                            curJoint = curChild.AddComponent<FixedJoint2D>();
                        else{
                            curJoint = curChild.AddComponent<SpringJoint2D>();
                            ((SpringJoint2D)curJoint).dampingRatio = spineDampening;
                            ((SpringJoint2D)curJoint).frequency = spineStrength*4;
                        }
                        curJoint.connectedBody = children[innerVerts[i-1]].GetComponent<Rigidbody2D>();
                    }
                }
            }
            createVertebrae();



            //a small(but really hard to think about) helper function to get the index of the skin vertex the correct count away
            int neighborSkin(int ind, int skinDist){
                int dir = skinDist/Mathf.Abs(skinDist);
                int ret = ind;
                for(int skinCount = 0; skinCount != skinDist && Mathf.Abs(skinCount) < vertices.Length-1; skinCount += dir){
                    ret = (ret + dir)%vertices.Length;
                    if(ret + dir < 0)
                        ret = vertices.Length+dir;
                    while(innerVerts.Contains(ret) && Mathf.Abs(ret) < vertices.Length){
                        ret = (ret + dir)%vertices.Length;
                        if(ret + dir < 0)
                            ret = vertices.Length+dir;
                    }
                }
                return ret;
            }

            int start = neighborSkin(vertices.Length-1,1);
            //create and joint the skin colliders
            //this assumes the vertices have been indexed in a clockwise order
            void createSkin(){

                int lastInd = start;
                Vector2 mid;
                int nextInd;
                float skinGap;
                Vector2 toward;
                Vector2 frontAnchor;
                Vector2 backAnchor;
                Vector2 frontConnect;
                Vector2 backConnect;

                //find the closest vertebrae to attach to
                int closestVertebrae(int index){
                    int closest = 0;
                    for(int a = 1; a < innerVerts.Count; a++)
                        if(Vector2.Distance(vertices[index], vertices[innerVerts[a]]) < Vector2.Distance(vertices[index], vertices[innerVerts[closest]]))
                            closest = a;
                    return innerVerts[closest];
                }

                //calculate whether the vertices are indexed clockwise or counter
                nextInd = neighborSkin(start, 1);
                toward = (vertices[nextInd] - vertices[start]).normalized;
                Vector2 toVertebrae = (vertices[closestVertebrae(start)] - vertices[start]).normalized;
                Vector2 difFromPerp = Vector2.Perpendicular(toVertebrae) - toward;
                bool clockwise = (difFromPerp.magnitude < 1);
                // Debug.Log("toward: "+toward);
                // Debug.Log("toVertebrae: "+toVertebrae);
                // Debug.Log("difference between perpendicular tovert and toward: "+difFromPerp);
                Debug.Log("this mesh is clockwise?..."+clockwise);

                //create the collider objects for the surface
                for(int i = start; i < vertices.Length && lastInd <= i; i = nextInd){
                    
                    curChild = new GameObject("part "+i, typeof(Rigidbody2D));
                    curChild.transform.parent = scaler.transform;
                    curChild.transform.localScale = new Vector3(1,1,1);
                    children[i] = curChild;
                    curChild.transform.localPosition = vertices[i];
                    curChild.GetComponent<Rigidbody2D>().mass = partMass;

                    lastInd = neighborSkin(i,-1);
                    nextInd = neighborSkin(i, 1);
                    skinGap = Vector2.Distance(vertices[i], vertices[nextInd]);
                    mid = curChild.transform.parent.TransformPoint((vertices[i]+vertices[nextInd])/2);
                    toward = scaler.transform.TransformDirection(vertices[nextInd] - vertices[i]);

                    if(useSquareColliders){
                        curCollid = curChild.AddComponent<BoxCollider2D>();
                        if(skinGap > squarEdgeRad*2 && skinThickness > squarEdgeRad*2){
                            ((BoxCollider2D)curCollid).size = new Vector2(skinGap-(squarEdgeRad*2), skinThickness-(squarEdgeRad*2));
                            ((BoxCollider2D)curCollid).edgeRadius = squarEdgeRad;
                        }
                        else
                            ((BoxCollider2D)curCollid).size = new Vector2(skinGap, skinThickness);
                        Vector3 curRot = curChild.transform.localEulerAngles;
                        curChild.transform.localEulerAngles += new Vector3(0,0, curRot.z +Vector2.SignedAngle(Vector2.right, toward));
                        curCollid.offset = curChild.transform.InverseTransformPoint(mid);
                    }
                    else{
                        curCollid = curChild.AddComponent<CircleCollider2D>();
                        ((CircleCollider2D)curCollid).radius = skinGap/2;
                    }
                    if(physMat != null) curCollid.sharedMaterial = physMat;

                    //find the closest vertebrae to attach to
                    int closest = closestVertebrae(i);

                    backAnchor = new Vector2(curCollid.bounds.center.x-(curCollid.bounds.extents.x*.9F), curCollid.bounds.center.y);
                    frontAnchor = new Vector2(curCollid.bounds.center.x+(curCollid.bounds.extents.x*.9F), curCollid.bounds.center.y);
                    backConnect = curChild.transform.InverseTransformPoint(children[closest].transform.position);
                    frontConnect = new Vector2((backConnect.x+frontAnchor.x)/2,backConnect.y);
                    backConnect = new Vector2((backConnect.x+backAnchor.x)/2,backConnect.y);
                    frontConnect = curChild.transform.TransformPoint(frontConnect);
                    backConnect = curChild.transform.TransformPoint(backConnect);
                    frontConnect = children[closest].transform.InverseTransformPoint(frontConnect);
                    backConnect = children[closest].transform.InverseTransformPoint(backConnect);

                    //configure joints
                    //connect curChild to the spine
                    curJoint = curChild.AddComponent<SpringJoint2D>();
                    curJoint.connectedBody = children[closest].GetComponent<Rigidbody2D>();
                    ((SpringJoint2D)curJoint).dampingRatio = bounceDampening;
                    ((SpringJoint2D)curJoint).frequency = springStrength;
                    curJoint.anchor = backAnchor;
                    curJoint.connectedAnchor = backConnect;

                    curJoint = curChild.AddComponent<SpringJoint2D>();
                    curJoint.connectedBody = children[closest].GetComponent<Rigidbody2D>();
                    ((SpringJoint2D)curJoint).dampingRatio = bounceDampening;
                    ((SpringJoint2D)curJoint).frequency = springStrength;
                    curJoint.anchor = frontAnchor;
                    curJoint.connectedAnchor = frontConnect;

                    if(internalFriction){
                        curJoint = curChild.AddComponent<FrictionJoint2D>();
                        curJoint.connectedBody = children[closest].GetComponent<Rigidbody2D>();
                        ((FrictionJoint2D)curJoint).maxForce = frictionStrength;
                        ((FrictionJoint2D)curJoint).maxTorque = frictionStrength*2;
                        curJoint.anchor = curCollid.bounds.center;
                    }
                    
                    lastInd = i;
                }
                
                Bounds oBounds;
                int twoBehind;
                float hypot = Mathf.Sqrt(Mathf.Pow(mesh.bounds.size.x, 2) + Mathf.Pow(mesh.bounds.size.y, 2));
                Debug.Log("hypot is: "+hypot);
                lastInd = start;
                //loop through the vertices again to finish configuring skin joints to eachother now that the colliders have been made
                for(int i = start; i < vertices.Length && lastInd <= i; i = nextInd){

                    lastInd = neighborSkin(i,-1);
                    nextInd = neighborSkin(i, 1);
                    curCollid = children[i].transform.GetComponent<Collider2D>();
                    oBounds = children[nextInd].transform.GetComponent<Collider2D>().bounds;

                    backAnchor = new Vector2(curCollid.bounds.center.x-(curCollid.bounds.extents.x*.9F), curCollid.bounds.center.y);
                    frontAnchor = new Vector2(curCollid.bounds.center.x+(curCollid.bounds.extents.x*.9F), curCollid.bounds.center.y);
                    // distConnect = new Vector2(oBounds.center.x-(oBounds.extents.x*.9F), oBounds.center.y);

                    //make distance joints so colliders dont fall out the skin 
                    curJoint = children[i].AddComponent<DistanceJoint2D>();
                    curJoint.connectedBody = children[nextInd].GetComponent<Rigidbody2D>();
                    curJoint.anchor = new Vector2(curCollid.bounds.max.x, curCollid.bounds.center.y);
                    // curJoint.connectedAnchor = distConnect;

                    //connect curChild to the skin collider that is 2 behind
                    twoBehind = neighborSkin(i, -2);
                    curJoint = children[i].AddComponent<SpringJoint2D>();
                    ((SpringJoint2D)curJoint).dampingRatio = bounceDampening;
                    ((SpringJoint2D)curJoint).frequency = springStrength;
                    curJoint.connectedBody = children[twoBehind].GetComponent<Rigidbody2D>();
                    curJoint.anchor = backAnchor;
                    curJoint.connectedAnchor = children[twoBehind].GetComponent<Collider2D>().bounds.center;

                    List<int> rayExclude = innerVerts.GetRange(0,innerVerts.Count);
                    rayExclude.Add(lastInd);
                    rayExclude.Add(nextInd);
                    children[i].AddComponent<RayJoin>().Setup(clockwise, rayExclude, hypot,spineStrength, spineDampening);
                
                    lastInd = i;
                }
            }
            createSkin();

            //re order the children in the scaler accoring to vertex index
            for(int i = 0; i < children.Length; i++)
                children[i].transform.SetSiblingIndex(i);

            //scale the scaler to fit the texture now that all the children are in
            float xScale = picDims.x/(picDims.x+skinThickness);
            float yScale = picDims.y/(picDims.y+skinThickness);
            scaler.transform.localScale = new Vector3(xScale, yScale, 1);
            
            picture.transform.localPosition = new Vector3(0,0);
            scaler.transform.localPosition = new Vector3(0,0);
            // scaler.transform.position = picture.transform.TransformPoint(rederBounds.center) - GetMaxBounds(scaler, true).center;
        }
    }

    //get the total bounds of an object and its children in world space
    //super useful for scaling purposes
    //pass a second parameter of true to base the bounds on colliders instead of renderers
    static public Bounds GetMaxBounds(GameObject g, bool colliderBased = false){
        Bounds b = new Bounds(g.transform.position, Vector3.zero);
        Bounds temp;
        if(colliderBased)
            foreach (Collider2D c in g.GetComponentsInChildren<Collider2D>()){
                temp = new Bounds(c.transform.TransformPoint(c.bounds.center), c.bounds.size);
                b.Encapsulate(temp);
            }
        else
            foreach (Renderer r in g.GetComponentsInChildren<Renderer>()){
                temp = new Bounds(r.transform.TransformPoint(r.bounds.center), r.bounds.size);
                b.Encapsulate(temp);
            }
        return b;
    }
}
