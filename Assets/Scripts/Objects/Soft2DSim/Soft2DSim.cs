using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//this class gives the gameobject its put on the ability to simulate softness that is visible in an assigned texture
//it does this by making physical children-gameobjects which translate their movement to the parents mesh vertices
//generateParts must be run for the children to be constructed and it assumes that the vertices that make up the mesh's perimeter are indexed sequentially
[ExecuteInEditMode]
public class Soft2DSim : MonoBehaviour
{
    public Sprite sprite;
    public Mesh mesh; //only necessary if no sprite has been set
    public Material mat; //this is material that holds the right shader and texture for the mesh assigned
    [Range(3,9)] public int consideredIn = 4; //this is the threshhold of number of triangles a vertex must be in the be considered an inner vertex
    public bool solidSpine = true; //are the spine segments in a fixed place
    [MinAttribute(0)] public float vertebraeMass = 1;
    [MinAttribute(0)] public float spineStrength = 5;
    [Range(0,1)] public float spineDampening = .9F;
    public bool internalFriction = false; //should energy be held in the soft object?(should deformations be more permanent?)
    [MinAttribute(0)] public float frictionStrength = 1;
    [MinAttribute(0)] public float skinThickness = .3F;
    private float squarEdgeRad{get{return skinThickness/4;}}
    public PhysicsMaterial2D physMat; //set this should the skin use a speciific physics material
    public bool impenetrable = true; //if true the skin uses distance joints to stay together otherwise it uses maxed spring joints. toggle if theres jitter
    [MinAttribute(0)] public float partMass = 1; //the mass of the skin segments
    [MinAttribute(0)] public float springStrength = 3; //the strength of the springs holding the  skin in place
    [Range(0,1)] public float bounceDampening = .9F; //how un-bouncy is the skin
    public bool updateVariablesLive; //enable this when fiddling with different values on joints and such but disable for actual play
    [SerializeField, HideInInspector] private GameObject[] children;
    [SerializeField, HideInInspector] private List<int> innerVerts;
    [SerializeField, HideInInspector] private Vector3[] vertices;
    
    
    [ContextMenu("Generate Parts")]
    public void GenerateParts(){
        //either the sprite or the mesh must be set to generate parts for
        if(mesh is null && sprite is null)
            Debug.Log("no mesh has been assigned");
        else if(transform.Find("parts") == null){
            Debug.Log("generating parts...");
            int[] triangles;
            Vector2 picDims;

            //if a sprite has been set then override the mesh with the sprite data
            //either way get the verticies and triangles from whatsbeen assigned
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


        //set up this gameobject to use a rigidbody and display the appropriate texture
        #region mainObj setup
            GameObject mainObj = gameObject;
            // mainObj.transform.localEulerAngles = Vector3.zero;

            //ensure this game object has all the necessary components
            //ensure a rigidbody
            Rigidbody2D contBody = mainObj.GetComponent<Rigidbody2D>();
            if(contBody == null) contBody = mainObj.AddComponent<Rigidbody2D>();
            //ensure a meshFilter
            MeshFilter mf = mainObj.GetComponent<MeshFilter>();
            if(mf == null) mf = mainObj.AddComponent<MeshFilter>();
            //ensure a meshRenderer
            MeshRenderer mr = mainObj.GetComponent<MeshRenderer>();
            if(mr == null) mr = mainObj.AddComponent<MeshRenderer>();

            mf.mesh = mesh;
            Bounds rederBounds = mr.bounds;

            Shader shader;
            Shader getDefaultShader(){
                Shader _shader = null;
                //scan for which rendering pipeline is in use to select a default shader
                if (GraphicsSettings.currentRenderPipeline){
                    string currentPipe = GraphicsSettings.currentRenderPipeline.GetType().ToString();
                    if (currentPipe.Contains("HighDefinition")){
                        _shader = Shader.Find("High Definition Render Pipeline/2D/Sprite-Lit-Default");
                        if(_shader == null) _shader = Shader.Find("HD Render Pipeline/2D/Sprite-Lit-Default");
                    }
                    else if(currentPipe.Contains("URP") || currentPipe.Contains("Universal"))
                        _shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
                }
                if(_shader == null) _shader = Shader.Find("Sprites/Default");

                return _shader;
            }
            
            //if a sprite has been set then make a material with the sprites texture and the rendering pipelines default shader
            //otherwise use the material thats been set if there is one or make one with the default shader if there isnt
            if(sprite != null){
                mat = new Material(getDefaultShader());
                mat.mainTexture = sprite.texture;
            }
            else if(mat == null)
                mat = new Material(getDefaultShader());
            else
                shader = mat.shader;

            mr.material = mat;
        #endregion


            //make the part scaler to use later
            GameObject scaler = new GameObject("parts");
            scaler.transform.parent = mainObj.transform;
            scaler.transform.localPosition = Vector3.zero;
            scaler.transform.localEulerAngles = Vector3.zero;


        #region create parts
        #region create spine
            //scan the triangles to find which vertices are inside the larger geometry(based on connection count)
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
            innerVerts = getInners();

            //set upvariables that are used often henceforth
            children = new GameObject[vertices.Length];
            GameObject curChild;
            Collider2D curCollid;
            AnchoredJoint2D curJoint;
        
            //create the collider objects for vertebrae
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
                if(solidSpine){
                    curJoint = curChild.AddComponent<FixedJoint2D>();
                }
                else{
                    curJoint = children[innerVerts[i]].AddComponent<SpringJoint2D>();
                    ((SpringJoint2D)curJoint).dampingRatio = spineDampening;
                    ((SpringJoint2D)curJoint).frequency = spineStrength*4;
                    curJoint.autoConfigureConnectedAnchor = true;
                }
                curJoint.connectedBody = contBody;
            }
            
            //find the closest vertebrae to attach to
            int closestVertebrae(int index){
                int? closest = null;
                for(int a = 0; a < innerVerts.Count; a++)
                    if(innerVerts[a] != index){
                        if(closest == null) closest = a;
                        else if(Vector2.Distance(vertices[index], vertices[innerVerts[a]]) < Vector2.Distance(vertices[index], vertices[innerVerts[(int)closest]]))
                            closest = a;
                    }
                return innerVerts[(int)closest];
            }
        #endregion

        #region create skin
            
            
            //initialize the sorted skin index array for later use
            int[] sortedSkinInds = new int[vertices.Length - innerVerts.Count];
            int skinInd = 0;

            List<int> nonConform = new List<int>();
            List<int> getExclusions(){
                List<int> retList = new List<int>();
                retList.AddRange(innerVerts);
                retList.AddRange(nonConform);
                return retList;
            }

            //I dont feel like rewriting all the calls for the newer method implimentation so Ill just use this wrapper
            int neighborSkin(int ind, int skinDist){
                return GetValidIndex(vertices.Length, ind, skinDist, getExclusions());
            }

            int start = neighborSkin(vertices.Length-1,1);

            //set up variables that are used often henceforth
            int lastInd = start;
            int nextInd;
            Vector2 toward;
            
            Vector2 toVertebrae;
            Vector2 difFromPerp;
            //calculate whether the vertices are indexed clockwise or counter based off the first sorted vertices
            toward = (vertices[neighborSkin(start, 1)] - vertices[start]).normalized;
            toVertebrae = (vertices[closestVertebrae(start)] - vertices[start]).normalized;
            difFromPerp = Vector2.Perpendicular(toVertebrae) - toward;
            bool clockwise = (difFromPerp.magnitude < 1);
            Debug.Log("this mesh is clockwise?..."+clockwise);

            //create the skin colliders
            float skinGap;
            Vector2 mid;
            Vector2 frontAnchor;
            Vector2 backAnchor;
            Vector2 frontConnect;
            Vector2 backConnect;

            //separate the creation of the parts and inital transformations from anything else so the transforms can be synced
            for(int i = start; i < vertices.Length && lastInd <= i; i = nextInd){
                
                lastInd = neighborSkin(i,-1);
                nextInd = neighborSkin(i, 1);
                
                //create the actual gameobject and rigidbody for this skin segment
                curChild = new GameObject("part "+i, typeof(Rigidbody2D));
                curChild.transform.parent = scaler.transform;
                curChild.transform.localScale = new Vector3(1,1,1);
                curChild.transform.localPosition = vertices[i];
                children[i] = curChild;
                //apparently after doing these manipulations on the transform I need to sync it with the physics system else it gets squirrely
                curChild.GetComponent<Rigidbody2D>().mass = partMass;

                lastInd = i;
            }

            Physics2D.SyncTransforms();

            //make a collider from i to nextInd and check if it overlaps any previously made colliders
            //destroys the newly made collider and returns false if collision was detected otherwise returns true
            bool createCollider(int i, int nextInd, int lastInd){

                curChild = children[i];
                //set the variables for the current segment
                skinGap = Vector2.Distance(vertices[i], vertices[nextInd]);
                mid = curChild.transform.parent.TransformPoint((vertices[i]+vertices[nextInd])/2);
                toward = scaler.transform.TransformDirection(vertices[nextInd] - vertices[i]);


                //make the collider for this segment
                curCollid = curChild.AddComponent<BoxCollider2D>();
                if(skinGap > squarEdgeRad*2 && skinThickness > squarEdgeRad*2){
                    ((BoxCollider2D)curCollid).size = new Vector2(skinGap-(squarEdgeRad*2), skinThickness/2);
                    ((BoxCollider2D)curCollid).edgeRadius = squarEdgeRad;
                }
                else
                    ((BoxCollider2D)curCollid).size = new Vector2(skinGap, skinThickness);
                curChild.transform.localEulerAngles = new Vector3(0,0, Vector2.SignedAngle(scaler.transform.TransformDirection(Vector2.right), toward));
                curCollid.offset = curChild.transform.InverseTransformPoint(mid);
                if(physMat != null) curCollid.sharedMaterial = physMat;

                //I belive I have to sync the transforms again after the rotation for the following collision check to work properly
                Physics2D.SyncTransforms();

                
                //calculate if this collider is an outer corner to shrink the collider
                int back;
                if(lastInd< 0) back = sortedSkinInds[GetValidIndex(skinInd,Array.IndexOf(sortedSkinInds, nextInd),-1)];
                else back = lastInd;
                Vector2 fromBack = scaler.transform.TransformDirection(vertices[i] - vertices[back]);
                Vector2 toNext = scaler.transform.TransformDirection(vertices[neighborSkin(nextInd,1)] - vertices[nextInd]);
                float ang = Vector2.SignedAngle(fromBack,toNext);
                if(Mathf.Abs(ang) > 54){
                    Vector2 oldSize = ((BoxCollider2D)curCollid).size;
                    if(((BoxCollider2D)curCollid).edgeRadius > 0)
                        oldSize.y *= .1F;
                    else oldSize.y *= .49F;
                    ((BoxCollider2D)curCollid).size = oldSize;
                    if((clockwise == (ang < 0))){
                        // Debug.Log("offsetting "+i+" with ang "+ang);
                        Vector2 oldOff = curCollid.offset;
                        oldOff.y += (skinThickness/5) * (clockwise? 1: -1);
                        curCollid.offset = oldOff;
                    }
                }

                
                //set the range of the scan to exclude indices not to scan
                //use this to skip the collision check all together
                if(lastInd != -1){
                    

                    //first scan through the innerVerts to make sure this collider isnt just cutting through the middle
                    for(int a = 0; a < innerVerts.Count; a++){
                        // if(i == 14 && nextInd != 15) Debug.Log("scanning "+a);
                        if(collidersIntersect(children[innerVerts[a]].GetComponent<Collider2D>(), curCollid)){
                            DestroyImmediate(curCollid);
                            // Debug.Log(i+"->"+nextInd+" failed collision check at "+sortedSkinInds[a]);
                            return false;
                        }
                    }

                    int startOfScan;
                    int endOfScan;
                    //separate scans for original construction and nonConformity construction. May merge later
                    if(lastInd == -2){
                        endOfScan = Array.IndexOf(sortedSkinInds, nextInd);
                        startOfScan = GetValidIndex(skinInd, endOfScan, -2);

                        // if(i == 25 || i == 46) Debug.Log("start is "+startOfScan+" and the end is "+ endOfScan+" in the sorted indexes");

                        //then scan through the indexes that have already been sorted
                        for(int a = startOfScan; a != endOfScan; a = GetValidIndex(skinInd,a,-1)){
                            // if(i == 14 && nextInd != 15) Debug.Log("scanning "+a);
                            if(collidersIntersect(children[sortedSkinInds[a]].GetComponent<Collider2D>(), curCollid)){
                                DestroyImmediate(curCollid);
                                // Debug.Log(i+"->"+nextInd+" failed collision check at "+sortedSkinInds[a]);
                                return false;
                            }
                        }
                    }
                    //pretty sloppy I should redo this at some point
                    else{
                        
                        if(lastInd != -2 && neighborSkin(nextInd, -4) >= i){
                            // Debug.Log("scan on "+i+" was abandoned due to receiving bad indexes");
                            return true;
                        }
                        endOfScan = GetValidIndex(vertices.Length, 0, -1, nonConform);
                        if(i == neighborSkin(0, -1)) endOfScan = nextInd;
                        startOfScan = neighborSkin(nextInd, -4);
                        
                        // if(i == 33) Debug.Log("start scan at "+ startOfScan+" and end at "+endOfScan);

                        for(int a = startOfScan; a != endOfScan; a = GetValidIndex(vertices.Length,a,-1, nonConform)){
                            // if(i == 14 && nextInd != 15) Debug.Log("scanning "+a);
                            if(collidersIntersect(children[a].GetComponent<Collider2D>(), curCollid)){
                                DestroyImmediate(curCollid);
                                if(i == 33) Debug.Log(i+"->"+nextInd+" failed collision check at "+a);
                                return false;
                            }
                        }
                    }

                }
                return true;
            }
            
            //loop through and create all the colliders that fit as indexed in the vertices array
            List<int> tempNonConforms = new List<int>();
            lastInd = 0;
            for(int i = start; i < vertices.Length && lastInd <= i; i = nextInd){
                
                lastInd = neighborSkin(i,-1);
                nextInd = neighborSkin(i, 1);
                if(lastInd < 0 || nextInd < 0) break;

                // Debug.Log("create segment "+ i+" with next of "+ nextInd+" and last of "+ lastInd);
                bool fit = createCollider(i, nextInd, lastInd);
                // Debug.Log(i+" fits with "+nextInd+"?..."+ fit);

                //if this skin segment conforms then add it to the sorted skin Inds
                if(fit){
                    sortedSkinInds[skinInd] = i;
                    skinInd++;
                    //if the tempNonConforms arent clean then re-create the last segment so angle scaling is right
                    if(tempNonConforms.Count > 0){
                        DestroyImmediate(children[sortedSkinInds[skinInd-2]].GetComponent<Collider2D>());
                        createCollider(sortedSkinInds[skinInd-2], sortedSkinInds[skinInd-1], -1);
                    }
                    tempNonConforms.Clear();

                }
                else{
                    //if the loop hasnt gone halfway through the following vertexes
                    if(tempNonConforms.Count < (vertices.Length-(skinInd+innerVerts.Count))/2){
                    // if(nextInd != i){
                        // Debug.Log("would have hung");
                        tempNonConforms.Add(nextInd);
                        nonConform.Add(nextInd);
                        nextInd = i;
                        // continue;
                    }
                    else{
                        // Debug.Log("skipped backwards vertex");
                        nonConform.RemoveAll(toRem => tempNonConforms.Contains(toRem));
                        tempNonConforms.Clear();
                        
                        nonConform.Add(i);
                        nextInd = neighborSkin(i,1);
                        Debug.Log(i+" has been declared a noncomformity");
                    }
                }

                lastInd = i;
            }
            // Debug.Log("here");
            //check why there seem to be occasional false positives for nonconformity
            //I think it has something to do where the scan starts and ends in the nonConform scans as opposed the initial creation scans

            // foreach(int num in nonConform) Debug.Log(num);
            List<int> unFit = new List<int>(nonConform);
            //loop through the nonconformities to find where they fit in the skin
            for(int a = 0; a < nonConform.Count; a++){
                // Debug.Log("nonConformity: "+nonConform[a]);

                //if this nonConform was in the position behind first
                if(neighborSkin(nonConform[a],1) == start){
                    Debug.Log("remade the first skin at index " + neighborSkin(nonConform[a],1));
                    DestroyImmediate(children[sortedSkinInds[0]].GetComponent<Collider2D>());
                    createCollider(sortedSkinInds[0], sortedSkinInds[1], sortedSkinInds[skinInd-1]);
                }

                //loop through the possible positions in the sorted array this deformity could go
                // int conformStart = Array.IndexOf(skinIndSorted, neighborSkin(nonConform[a], 1));
                int conformStart = 0;
                for(int i = conformStart; i < skinInd; i++){

                    int nextNext = GetValidIndex(skinInd, i, 1);
                    bool fit = createCollider(nonConform[a], sortedSkinInds[i], -2);
                    if(fit){
                        //reconstruct one further because the scan direction comes from beginning but we want it to point to end
                        // Debug.Log("nonConform "+nonConform[a]+" fits behind "+sortedSkinInds[i]);
                        DestroyImmediate(curCollid); //this is only legit bc I just made this one
                        fit = createCollider(nonConform[a], sortedSkinInds[nextNext], -2);
                        if(fit){
                            Debug.Log("nonConform "+nonConform[a]+" fits in "+sortedSkinInds[nextNext]+" slot");
                            insertInArray(sortedSkinInds, nextNext, nonConform[a]); // test if this method is working as expected
                            skinInd++;
                            unFit.Remove(nonConform[a]);

                            //recreate the collider that leads to the new one
                            DestroyImmediate(children[sortedSkinInds[i]].GetComponent<Collider2D>());
                            createCollider(sortedSkinInds[i], nonConform[a], -1);

                            break;
                        }
                    }
                }
            }
            // Debug.Log("beginning of sorted: "+sortedSkinInds[0]+", "+sortedSkinInds[1]+", "+ sortedSkinInds[2]);
            
            Physics2D.SyncTransforms();

            int closest;

            //make regular circle colliders for the unFit and create the few joints that are independent of other skin
            for(int i = 0; i < unFit.Count; i++){
                curCollid = children[unFit[i]].AddComponent<CircleCollider2D>();
                ((CircleCollider2D)curCollid).radius = skinThickness/2;

                Vector2 transCenter = children[unFit[i]].transform.InverseTransformPoint(curCollid.bounds.center);
                closest = closestVertebrae(unFit[i]);

                curJoint = children[sortedSkinInds[i]].AddComponent<SpringJoint2D>();
                curJoint.connectedBody = children[closest].GetComponent<Rigidbody2D>();
                ((SpringJoint2D)curJoint).dampingRatio = bounceDampening;
                ((SpringJoint2D)curJoint).frequency = springStrength;
                curJoint.anchor = transCenter;

                //create the friction for this segment if its enabled
                if(internalFriction){
                    curJoint = children[sortedSkinInds[i]].AddComponent<FrictionJoint2D>();
                    curJoint.connectedBody = children[closest].GetComponent<Rigidbody2D>();
                    ((FrictionJoint2D)curJoint).maxForce = frictionStrength;
                    ((FrictionJoint2D)curJoint).maxTorque = frictionStrength*2;
                    curJoint.anchor = transCenter;
                }
            }
            

            //loop through the vertices again to configure skin joints to eachother now that the colliders have been made
            Bounds oBounds;
            int twoBehind;
            //the longer this is the more likely jitter is to occur it seems
            float hypot = Mathf.Sqrt(Mathf.Pow(mesh.bounds.size.x, 2) + Mathf.Pow(mesh.bounds.size.y, 2))/2;
            // Debug.Log("hypot is: "+hypot);
            lastInd = start;
            for(int i = 0; i < skinInd; i++){

                //set the variables for the current segment
                lastInd = sortedSkinInds[GetValidIndex(skinInd, i, -1)];
                nextInd = sortedSkinInds[GetValidIndex(skinInd, i, 1)];
                curCollid = children[sortedSkinInds[i]].transform.GetComponent<Collider2D>();
                oBounds = children[nextInd].transform.GetComponent<Collider2D>().bounds;

                //set the front and back anchor
                Vector2 transCenter = children[sortedSkinInds[i]].transform.InverseTransformPoint(curCollid.bounds.center);
                Vector2 transExtents = children[sortedSkinInds[i]].transform.InverseTransformDirection(curCollid.bounds.extents);
                backAnchor = new Vector2(transCenter.x-(Mathf.Abs(transExtents.x)*.95F), transCenter.y);
                frontAnchor = new Vector2(transCenter.x+(Mathf.Abs(transExtents.x)*.95F), transCenter.y);

                // Vector2 transMax = children[sortedSkinInds[i]].transform.InverseTransformPoint(curCollid.bounds.max);
                // Vector2 transMin = children[sortedSkinInds[i]].transform.InverseTransformPoint(curCollid.bounds.min);
                // backAnchor = new Vector2(transMin.x*.95F, transCenter.y);
                // frontAnchor = new Vector2(transMax.x*.95F, transCenter.y);

                // backAnchor = new Vector2(curCollid.bounds.center.x-(curCollid.bounds.extents.x*.95F), curCollid.bounds.center.y);
                // frontAnchor = new Vector2(curCollid.bounds.center.x+(curCollid.bounds.extents.x*.95F), curCollid.bounds.center.y);
                // backAnchor = children[sortedSkinInds[i]].transform.InverseTransformPoint(backAnchor);
                // frontAnchor = children[sortedSkinInds[i]].transform.InverseTransformPoint(frontAnchor);

                //make distance joints so colliders dont fall out the skin
                if(impenetrable)
                    curJoint = children[sortedSkinInds[i]].AddComponent<DistanceJoint2D>();
                else{
                    curJoint = children[sortedSkinInds[i]].AddComponent<SpringJoint2D>();
                    ((SpringJoint2D)curJoint).dampingRatio = 1;
                    ((SpringJoint2D)curJoint).frequency = 0; // zero goes to max
                }
                curJoint.connectedBody = children[nextInd].GetComponent<Rigidbody2D>();
                curJoint.anchor = frontAnchor;

                //connect curChild to the skin collider that is 2 behind
                twoBehind = sortedSkinInds[GetValidIndex(skinInd, i, -2)];
                curJoint = children[sortedSkinInds[i]].AddComponent<SpringJoint2D>();
                ((SpringJoint2D)curJoint).dampingRatio = bounceDampening;
                ((SpringJoint2D)curJoint).frequency = springStrength;
                curJoint.connectedBody = children[twoBehind].GetComponent<Rigidbody2D>();
                curJoint.anchor = backAnchor;
                curJoint.connectedAnchor = children[twoBehind].transform.InverseTransformPoint(children[twoBehind].GetComponent<Collider2D>().bounds.center);

                //find the closest vertebrae to attach to
                closest = closestVertebrae(sortedSkinInds[i]);
                
                //create the friction for this segment if its enabled
                if(internalFriction){
                    curJoint = children[sortedSkinInds[i]].AddComponent<FrictionJoint2D>();
                    curJoint.connectedBody = children[closest].GetComponent<Rigidbody2D>();
                    ((FrictionJoint2D)curJoint).maxForce = frictionStrength;
                    ((FrictionJoint2D)curJoint).maxTorque = frictionStrength*2;
                    curJoint.anchor = transCenter;
                }

                //set up the Rayjoin class on the segment so it can run on its own
                List<GameObject> rayExclude = new List<GameObject>();
                foreach(int num in innerVerts)
                    rayExclude.Add(children[num]);
                rayExclude.Add(children[lastInd]);
                rayExclude.Add(children[nextInd]);
                children[sortedSkinInds[i]].AddComponent<RayJoin>().Setup(clockwise, rayExclude, hypot,spineStrength, spineDampening);


                //do some vector math so calculate how this segment should join to the spine
                backConnect = children[sortedSkinInds[i]].transform.InverseTransformPoint(children[closest].transform.position);
                frontConnect = new Vector2((backConnect.x+frontAnchor.x)/2,backConnect.y);
                backConnect = new Vector2((backConnect.x+backAnchor.x)/2,backConnect.y);
                frontConnect = children[sortedSkinInds[i]].transform.TransformPoint(frontConnect);
                backConnect = children[sortedSkinInds[i]].transform.TransformPoint(backConnect);
                frontConnect = children[closest].transform.InverseTransformPoint(frontConnect);
                backConnect = children[closest].transform.InverseTransformPoint(backConnect);

                //configure joints
                //connect curChild to the spine
                curJoint = children[sortedSkinInds[i]].AddComponent<SpringJoint2D>();
                curJoint.connectedBody = children[closest].GetComponent<Rigidbody2D>();
                ((SpringJoint2D)curJoint).dampingRatio = bounceDampening;
                ((SpringJoint2D)curJoint).frequency = springStrength;
                curJoint.anchor = backAnchor;
                curJoint.connectedAnchor = backConnect;

                curJoint = children[sortedSkinInds[i]].AddComponent<SpringJoint2D>();
                curJoint.connectedBody = children[closest].GetComponent<Rigidbody2D>();
                ((SpringJoint2D)curJoint).dampingRatio = bounceDampening;
                ((SpringJoint2D)curJoint).frequency = springStrength;
                curJoint.anchor = frontAnchor;
                curJoint.connectedAnchor = frontConnect;

            
                lastInd = sortedSkinInds[i];
            }
        #endregion
        #endregion

            //re order the children in the scaler accoring to vertex index
            int edInd = 0;
            for(int i = 0; i < sortedSkinInds.Length; i++){
                // Debug.Log("set "+ i);
                children[sortedSkinInds[i]].transform.SetSiblingIndex(edInd);
                edInd++;
            }
            for(int i = 0; i < innerVerts.Count; i++){
                children[innerVerts[i]].transform.SetSiblingIndex(edInd);
                edInd++;
            }

            //scale the scaler to fit the texture now that all the children are in
            float xScale = picDims.x/(picDims.x+skinThickness);
            float yScale = picDims.y/(picDims.y+skinThickness);
            scaler.transform.localScale = new Vector3(xScale, yScale, 1);
            
            scaler.transform.localPosition = new Vector3(0,0);
            // scaler.transform.position = picture.transform.TransformPoint(rederBounds.center) - GetMaxBounds(scaler, true).center;
        }
    }


    //a method to get the an element the appropriate count away from an index while excluding certain indices
    //pretty hard to think about imo
    int GetValidIndex(int totalLength, int startInd, int endDist, List<int> exlusions){
        int dir = endDist/Mathf.Abs(endDist);
        int ret = startInd;
        int skinCount = 0;

        //check to make sure the total has atleast enough more than the exclusions to complete the travel distance
        if((totalLength-exlusions.Count) <= endDist) return -1;

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

    int GetValidIndex(int totalLength, int startInd, int endDist){
        int dir = endDist/Mathf.Abs(endDist);
        int ret = startInd;
        for(int skinCount = 0; skinCount != endDist && Mathf.Abs(skinCount) < totalLength-1; skinCount += dir){
            if(ret + dir < 0)
                ret = totalLength+dir;
            else ret = (ret + dir)%totalLength;
        }
        return ret;
    }

    T[] insertInArray<T>(T[] arr, int index, T toInsert){
        for(int i = arr.Length-1; i > index; i--){
            arr[i] = arr[i-1];
        }
        arr[index] = toInsert;
        return arr;
    }


    bool collidersIntersect(Collider2D collidA, Collider2D collidB){
        Vector2 bCloseToA = collidB.ClosestPoint(collidA.transform.TransformPoint(collidA.offset));
        Vector2 aCloseToB = collidA.ClosestPoint(bCloseToA);
        // Instance.spawnDot(aCloseToB);
        // Instance.spawnDot(bCloseToA);
        bCloseToA = collidB.ClosestPoint(aCloseToB);
        if(aCloseToB == bCloseToA) return true;
        //switch the colliders names so I can just copy and paste the above
        Collider2D save = collidA;
        collidA = collidB;
        collidB = save;
        bCloseToA = collidB.ClosestPoint(collidA.transform.TransformPoint(collidA.offset));
        aCloseToB = collidA.ClosestPoint(bCloseToA);
        bCloseToA = collidB.ClosestPoint(aCloseToB);
        return aCloseToB == bCloseToA;
    }


    [ContextMenu("Clear")]
    void Clear(){
        // Debug.ClearDeveloperConsole();
        if(sprite != null){
            mesh = null;
            mat = null;
        }
        Transform parts = transform.Find("parts");
        if(parts != null)
            DestroyImmediate(parts.gameObject);
    }

    void Update(){
        if(transform.Find("parts") != null){
            //this small bit just keeps the mesh stretched to fit the colliders as they move
            for(int i = 0; i < children.Length; i++){
                vertices[i] = children[i].transform.localPosition;
                // Vector2 backThirdDrirect = new Vector2(Mathf.Cos(Mathf.Deg2Rad*(parts.GetChild(i).localEulerAngles.z+60)),Mathf.Sin(Mathf.Deg2Rad*(parts.GetChild(i).localEulerAngles.z+60)));
                // Debug.DrawRay(parts.GetChild(i).position, backThirdDrirect, Color.red);
            }
            mesh.vertices = vertices; 

            //this is the section that keeps the inspector changes synced up with the actual gameobjects after theyve been made
            if(updateVariablesLive){
                GetComponent<Rigidbody2D>().WakeUp();

                //sync the values for the spine
                Rigidbody2D bod;
                SpringJoint2D[] springs;
                Collider2D collid;
                for(int a = 0; a < innerVerts.Count; a++){

                    //rigidbody values
                    bod = children[innerVerts[a]].GetComponent<Rigidbody2D>();
                    if(bod != null){
                        bod.mass = vertebraeMass;
                        bod.WakeUp();
                    }

                    //spring values
                    springs = children[innerVerts[a]].GetComponents<SpringJoint2D>();
                    if(springs.Length > 0){
                        springs[0].frequency = spineStrength;
                        springs[0].dampingRatio = spineDampening;
                    }

                    //collider values
                    collid = children[innerVerts[a]].GetComponent<Collider2D>();
                    if(collid != null){
                        ((CircleCollider2D)collid).radius = skinThickness/2;
                    }
                }

                //sync the values for the skin
                FrictionJoint2D frict;
                for(int i = 0; i < children.Length; i++){

                    //rigidbody values
                    bod = children[i].GetComponent<Rigidbody2D>();
                    if(bod != null){
                        bod.mass = partMass;
                        bod.WakeUp();
                    }

                    //spring values
                    springs = children[i].GetComponents<SpringJoint2D>();
                    for(int b = 0; b < springs.Length; b++){
                        springs[b].frequency = springStrength;
                        springs[b].dampingRatio = bounceDampening;
                    }

                    //collider values
                    collid = children[i].GetComponent<BoxCollider2D>();
                    if(collid != null){
                        Vector2 dimSave = ((BoxCollider2D)collid).size;
                        if(dimSave.y < skinThickness/2){
                            if(((BoxCollider2D)collid).edgeRadius > 0)
                                ((BoxCollider2D)collid).edgeRadius = squarEdgeRad;
                        }
                        else{
                            if(((BoxCollider2D)collid).edgeRadius > 0){
                                ((BoxCollider2D)collid).size = new Vector2(dimSave.x, skinThickness/2);
                                ((BoxCollider2D)collid).edgeRadius = squarEdgeRad;
                            }
                            else ((BoxCollider2D)collid).size = new Vector2(dimSave.x, skinThickness);
                        }

                        if(physMat != null) collid.sharedMaterial = physMat;
                    }

                    //friction values
                    frict = children[i].GetComponent<FrictionJoint2D>();
                    if(frict != null){
                        frict.maxForce = frictionStrength;
                        frict.maxTorque = frictionStrength*2;
                    }
                }
            }
        }
    }
}
