using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//this class gives the gameobject its put on the ability to simulate softness that is visible in an assigned texture
//it does this by making physical children-gameobjects which translate their movement to the parents mesh vertices
//generateParts must be run for the children to be constructed
[ExecuteInEditMode]
public class SpriteSquishifier : MonoBehaviour
{
    public Sprite sprite; //the sprite to squishify
    [HideInInspector] public Mesh mesh; //This can be set as an alternative to giving a sprite. Remove "HideInInspector" to set this directly in the inspector
    [HideInInspector] public Material mat; //This can be set to specify a texture when using a mesh instead of a sprite. Remove "HideInInspector" to set this directly in the inspector
    [HideInInspector, Range(0,15)] public float innerTolerance = 1; //the tolerance to account for bad angular math when detecting the inner vertices.  Remove "HideInInspector" to set this directly in the inspector
    [HideInInspector] public bool updateLive = false; //enable this to changevalues without having to regenerate parts. Or disable to improve performance
    [MinAttribute(0.1F)] public float massScalar = 1; //the mass scale to be used when determining part mass so that skin will always be 1/3 the vertebrae
    public float vertebraeMass {get{return massScalar*1.5F;}} //the mass of the vertebrae
    [Range(0.5F,10)] public float spineStrength = 5; //the strength of the springs holding the vertebrae to where they start
    [Range(0,1)] public float spineDampening = 1; //how not-bouncy is the spine
    public float skinSegMass {get{return massScalar*.5F;}} //the mass of the skin segments
    [MinAttribute(0.02F)] public float skinThickness = .1F;
    [Range(0.2F,13)] public float springStrength = 8; //the strength of the springs holding the  skin in place
    [Range(0,1)] public float bounceDampening = 1; //how not-bouncy is the skin
    public PhysicsMaterial2D physicsMaterial; //set this to give the skin a specific physics material
    public float squarEdgeRad{get{return skinThickness/4;}}
    [Range(15,150), HideInInspector] public float angleShrinkThresh = 54; //if a colliders surface creates an angle over this threshold that collider will be shrunk. Remove "HideInInspector" to change this in the inspector
    public bool frictionEnabled{get{return internalFriction > 0;}} //enable to make deformations or dents be more permanent. Or disable to save on performance
    [Range(0,4999)] public float internalFriction = 2;
    [SerializeField, HideInInspector] private GameObject[] children = new GameObject[0];
    [SerializeField, HideInInspector] private List<int> innerVerts;
    [SerializeField, HideInInspector] private int[] sortedSkinInds;
    [SerializeField, HideInInspector] private Vector3[] vertices;
    [SerializeField, HideInInspector] private GameObject picture;
    [SerializeField, HideInInspector] private Transform scaler;
    [SerializeField, HideInInspector] private Rigidbody2D picAnchor;
    [SerializeField, HideInInspector] private bool goodSkin = true;

    
    //generates parts of increasing skinThickness until a segment is forced to shrink or cannot fit
    public void AutoSkin(){
        goodSkin = true;
        skinThickness = .05F;
        for(;goodSkin == true && skinThickness <= 50; skinThickness+=.05F, skinThickness = (float)Math.Round(skinThickness, 2))
            GenerateParts();
        if(skinThickness > .1F){
            skinThickness -= .1F;
            GenerateParts();
        }
    }
    

    //clears old parts and storage fields and then builds new parts with the values specified in public fields
    [ContextMenu("Generate Parts")]
    void GenerateParts(){
        Clear();
        BuildParts();
    }
    private void BuildParts(){
        //either the sprite or the mesh must be set to generate parts for
        if(mesh is null && sprite is null)
            Debug.Log("no mesh has been assigned");
        else if(scaler == null){
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
        #region picture setup

            //make the picture gameobject
            // picture = new GameObject("picture");
            // picture.transform.parent = transform;
            // picture.transform.localPosition = Vector3.zero;
            // picture.transform.localEulerAngles = Vector3.zero;
            picture = gameObject;

            picAnchor = picture.GetComponent<Rigidbody2D>();
            if(picAnchor == null) picAnchor = picture.AddComponent<Rigidbody2D>();
            picAnchor.gravityScale = 0;

            MeshFilter mf = picture.GetComponent<MeshFilter>();
            if(mf == null) mf = picture.AddComponent<MeshFilter>();

            MeshRenderer mr = picture.GetComponent<MeshRenderer>();
            if(mr == null) mr = picture.AddComponent<MeshRenderer>();

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
            scaler = (new GameObject("parts")).transform;
            scaler.parent = transform;
            scaler.localPosition = Vector3.zero;
            scaler.localEulerAngles = Vector3.zero;


        #region create parts
        #region create spine
            //scan the triangles to find which vertices are inside the shape. Any shape with less than 360 degrees or greater of surround by triangles will be considered inside
            List<int> getInners(){
                float[] surroundSum = new float[vertices.Length];
                List<int> innerVertices  = new List<int>();
                List<int> sorted  = new List<int>();

                float getSurround(Vector2 o1, Vector2 between, Vector2 o2){
                    return (float)Math.Round(Vector2.Angle(o1-between, o2-between),1); //seems this angle method has some imprecision
                }
                //go through all the tirangles and add up up the angular surrounding that each triangle provides to each point
                for(int i = 0; i<triangles.Length; i+=3){
                    surroundSum[triangles[i]] += getSurround(vertices[triangles[i+1]], vertices[triangles[i]], vertices[triangles[i+2]]);
                    surroundSum[triangles[i+1]] += getSurround(vertices[triangles[i+2]], vertices[triangles[i+1]], vertices[triangles[i]]);
                    surroundSum[triangles[i+2]] += getSurround(vertices[triangles[i]], vertices[triangles[i+2]], vertices[triangles[i+1]]);
                    for(int a = 0; a<3; a++){
                        if(surroundSum[triangles[i+a]] >= 360-innerTolerance) innerVertices.Add(triangles[i+a]);
                    }
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

            //set up variables that are used often henceforth
            children = new GameObject[vertices.Length];
            GameObject curChild;
            Collider2D curCollid;
            AnchoredJoint2D curJoint;
        
            //create the collider objects for vertebrae
            for(int i = 0; i < innerVerts.Count; i++){
                curChild = new GameObject("iPart "+innerVerts[i], typeof(Rigidbody2D));
                curChild.transform.parent = scaler;
                curChild.transform.localScale = new Vector3(1,1,1);
                children[innerVerts[i]] = curChild;
                curChild.transform.localPosition = vertices[innerVerts[i]];
                curChild.GetComponent<Rigidbody2D>().mass = vertebraeMass;
                curCollid = curChild.AddComponent<CircleCollider2D>();
                ((CircleCollider2D)curCollid).radius = skinThickness/2;

                //configure joints
                curJoint = children[innerVerts[i]].AddComponent<SpringJoint2D>();
                ((SpringJoint2D)curJoint).dampingRatio = spineDampening;
                ((SpringJoint2D)curJoint).frequency = spineStrength;
                curJoint.autoConfigureConnectedAnchor = true;
                curJoint.connectedBody = picAnchor;

                if(frictionEnabled){
                    curJoint = curChild.AddComponent<FrictionJoint2D>();
                    curJoint.connectedBody = picAnchor;
                    ((FrictionJoint2D)curJoint).maxForce = internalFriction;
                    ((FrictionJoint2D)curJoint).maxTorque = internalFriction*2;
                    curJoint.autoConfigureConnectedAnchor = true;
                }
            }

            if(innerVerts.Count == 0){
                Clear();
                Debug.Log("No inner vertices detected!");
                return;
            }
            //join close vertebrae together
            if(innerVerts.Count > 2){

                AlreadyJoined tester;

                void joiner(int i){
                    //find the first close vertebrae thats not already joined
                    AlreadyJoined curJoined = children[innerVerts[i]].GetComponent<AlreadyJoined>();
                    if(curJoined == null) curJoined = children[innerVerts[i]].AddComponent<AlreadyJoined>();
                    int runNum = closestVertebrae(innerVerts[i], curJoined.list);
                    if(runNum == -1) return;

                    //create a joint on the first close vert
                    curJoint = children[innerVerts[i]].AddComponent<SpringJoint2D>();
                    ((SpringJoint2D)curJoint).dampingRatio = spineDampening;
                    ((SpringJoint2D)curJoint).frequency = spineStrength;
                    curJoint.autoConfigureConnectedAnchor = true;
                    curJoint.enableCollision = true;
                    curJoint.connectedBody = children[runNum].GetComponent<Rigidbody2D>();

                    //add this vert to the newly joined verts alreadyjoined
                    tester = children[runNum].GetComponent<AlreadyJoined>();
                    if(tester == null) tester = children[runNum].AddComponent<AlreadyJoined>();
                    tester.list.Add(innerVerts[i]);
                    curJoined.list.Add(runNum);
                }

                for(int i = 0; i < innerVerts.Count; i++){
                    joiner(i);
                    // joiner(i);
                }
            }

        #endregion

        #region create skin
            
            
            //initialize the sorted skin index array for later use
            sortedSkinInds = new int[vertices.Length - innerVerts.Count];
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
            int clockCount = 0;
            int ccCount = 0;
            //calculate whether the vertices are indexed clockwise or counter based off a loop of the skin indices
            for(int i = start; i < vertices.Length && lastInd <= i; i = nextInd){
                
                lastInd = neighborSkin(i,-1);
                nextInd = neighborSkin(i, 1);

                toward = (vertices[nextInd] - vertices[i]).normalized;
                toVertebrae = (vertices[closestVertebrae(i)] - vertices[i]).normalized;
                difFromPerp = Vector2.Perpendicular(toVertebrae) - toward;
                if((difFromPerp.magnitude < 1)) clockCount++;
                else ccCount++;

                lastInd = i;
            }

            bool clockwise = (clockCount >= ccCount);
            Debug.Log("this mesh is clockwise?..."+clockwise);
            

            //create the skin colliders
            float skinGap;
            Vector2 mid;
            Vector2 frontAnchor;
            Vector2 backAnchor;
            Vector2 frontConnect;
            Vector2 backConnect;

            lastInd = start;
            //separate the creation of the outter parts and inital transformations from anything else so the transforms can be synced
            for(int i = start; i < vertices.Length && lastInd <= i; i = nextInd){
                
                lastInd = neighborSkin(i,-1);
                nextInd = neighborSkin(i, 1);
                
                //create the actual gameobject and rigidbody for this skin segment
                curChild = new GameObject("oPart "+i, typeof(Rigidbody2D));
                curChild.transform.parent = scaler;
                curChild.transform.localScale = new Vector3(1,1,1);
                curChild.transform.localPosition = vertices[i];
                children[i] = curChild;
                //apparently after doing these manipulations on the transform I need to sync it with the physics system else it gets squirrely
                curChild.GetComponent<Rigidbody2D>().mass = skinSegMass;

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
                toward = scaler.TransformDirection(vertices[nextInd] - vertices[i]);


                //make the collider for this segment
                curCollid = curChild.AddComponent<BoxCollider2D>();
                if(skinGap > squarEdgeRad*2 && skinThickness > squarEdgeRad*2){
                    ((BoxCollider2D)curCollid).size = new Vector2(skinGap-(squarEdgeRad*2), skinThickness/2);
                    ((BoxCollider2D)curCollid).edgeRadius = squarEdgeRad;
                }
                else
                    ((BoxCollider2D)curCollid).size = new Vector2(skinGap, skinThickness);
                curChild.transform.localEulerAngles = new Vector3(0,0, Vector2.SignedAngle(scaler.TransformDirection(Vector2.right), toward));
                curCollid.offset = curChild.transform.InverseTransformPoint(mid);
                if(physicsMaterial != null) curCollid.sharedMaterial = physicsMaterial;

                //I belive I have to sync the transforms again after the rotation for the following collision check to work properly
                Physics2D.SyncTransforms();

                
                //calculate if this collider is an outer corner to shrink the collider
                if(skinGap < skinThickness*1.5){
                    int back;
                    if(lastInd< 0) back = sortedSkinInds[GetValidIndex(skinInd,Array.IndexOf(sortedSkinInds, nextInd),-1)];
                    else back = lastInd;
                    Vector2 fromBack = scaler.TransformDirection(vertices[i] - vertices[back]);
                    Vector2 toNext = scaler.TransformDirection(vertices[neighborSkin(nextInd,1)] - vertices[nextInd]);
                    float ang = Vector2.SignedAngle(fromBack,toNext);
                    if(Mathf.Abs(ang) > angleShrinkThresh){
                        Vector2 oldSize = ((BoxCollider2D)curCollid).size;
                        if(((BoxCollider2D)curCollid).edgeRadius > 0)
                            oldSize.y *= .1F;
                        else oldSize.y *= .49F;
                        ((BoxCollider2D)curCollid).size = oldSize;
                        // if((clockwise == (ang < 0))){
                        //     // Debug.Log("offsetting "+i+" with ang "+ang);
                        //     Vector2 oldOff = curCollid.offset;
                        //     oldOff.y += (skinThickness/5) * (clockwise? 1: -1);
                        //     curCollid.offset = oldOff;
                        // }
                    }
                }
                // if(i==16|| i==15) return true;

                
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
                        
                        // if(i == 113) Debug.Log("start scan at "+ sortedSkinInds[startOfScan]+" and end at "+sortedSkinInds[endOfScan]);
                        //then scan through the indexes that have already been sorted
                        for(int a = startOfScan; a != endOfScan; a = GetValidIndex(skinInd,a,-1)){
                            // if(i == 15) Debug.Log("scanning "+a);
                            if(collidersIntersect(children[sortedSkinInds[a]].GetComponent<Collider2D>(), curCollid)){
                                DestroyImmediate(curCollid);
                                if(i == 39 && nextInd == 28) Debug.Log(i+"->"+nextInd+" failed collision check at "+sortedSkinInds[a]);
                                return false;
                            }
                        }
                    }
                    //redo this at some point?
                    else{
                        
                        if(lastInd != -2 && neighborSkin(nextInd, -4) >= i){
                            // Debug.Log("scan on "+i+" was abandoned due to receiving bad indexes");
                            return true;
                        }
                        endOfScan = GetValidIndex(vertices.Length, 0, -1, nonConform);
                        if(i == neighborSkin(0, -1)) endOfScan = nextInd;
                        startOfScan = neighborSkin(nextInd, -4);
                        
                        // if(i == 9) Debug.Log("start scan at "+ startOfScan+" and end at "+endOfScan);
                        for(int a = startOfScan; a != endOfScan; a = GetValidIndex(vertices.Length,a,-1, nonConform)){
                            // if(i == 15) Debug.Log("scanning "+a);
                            if(collidersIntersect(children[a].GetComponent<Collider2D>(), curCollid)){
                                DestroyImmediate(curCollid);
                                // if(i == 123) Debug.Log(i+"->"+nextInd+" failed collision check at "+a);
                                return false;
                            }
                        }
                    }

                }
                return true;
            }
            
            //loop through and create all the colliders that fit as indexed in the vertices array
            List<int> tempNonConforms = new List<int>();
            lastInd = start;
            bool ended = false;
            for(int i = start; i < vertices.Length && lastInd <= i; i = nextInd){
                
                lastInd = neighborSkin(i,-1);
                nextInd = neighborSkin(i, 1);
                if(lastInd < 0 || nextInd < 0) break;

                if(ended){
                    nonConform.Add(i);
                    lastInd = i;
                    continue;
                }

                // if(i == 9) Debug.Log("create segment "+ i+" with next of "+ nextInd+" and last of "+ lastInd);

                bool fit = createCollider(i, nextInd, lastInd);
                // Debug.Log(i+" fits with "+nextInd+"?..."+ fit);

                //if this skin segment conforms then add it to the sorted skin Inds
                if(fit){
                    
                    //check the distance to the start if its not still in the beginning quarter
                    //if the start is closer than nextInd then i will be placed as last and the rest after it will be made nonConforms
                    float? distToStart = null;
                    if(skinInd > (vertices.Length-innerVerts.Count)/4){
                        distToStart = Vector2.Distance(vertices[i], vertices[sortedSkinInds[0]]);
                        if(distToStart < Vector2.Distance(vertices[i], vertices[nextInd])){
                            if(createCollider(i, sortedSkinInds[0], lastInd)){
                                DestroyImmediate(children[i].GetComponent<Collider2D>());
                                sortedSkinInds[skinInd] = i;
                                skinInd++;
                                lastInd = i;
                                ended = true;
                                continue;
                            }
                        }
                    }

                    if(skinInd == 0) start = i;
                    sortedSkinInds[skinInd] = i;
                    skinInd++;

                    // if(i==15) return;
                    //if the tempNonConforms arent clean then re-create the last segment so angle scaling is right
                    if(tempNonConforms.Count > 0){
                        DestroyImmediate(children[sortedSkinInds[skinInd-2]].GetComponent<Collider2D>());
                        createCollider(sortedSkinInds[skinInd-2], sortedSkinInds[skinInd-1], -1);
                    }
                    tempNonConforms.Clear();
                }
                else{
                    //if the loop hasnt gone halfway through the legit vertexes
                    if(tempNonConforms.Count <= (vertices.Length-(innerVerts.Count + (nonConform.Count-tempNonConforms.Count))/2) && nextInd != start){
                    // if(nextInd != i){
                        // Debug.Log("would have hung");
                        tempNonConforms.Add(nextInd);
                        nonConform.Add(nextInd);
                        nextInd = i;
                        // continue;
                    }
                    else{
                        Debug.Log("skipped nonConform "+i);
                        nonConform.RemoveAll(toRem => tempNonConforms.Contains(toRem));
                        tempNonConforms.Clear();
                        
                        nonConform.Add(i);
                        // Debug.Log(i+" has been declared a noncomformity");
                    }
                }

                lastInd = i;
            }

            // Debug.Log("here");
            // return;

            //check why there seem to be occasional false positives for nonconformity
            //I think it has something to do where the scan starts and ends in the nonConform scans as opposed the initial creation scans

            // foreach(int num in nonConform) Debug.Log(num);
            List<int> unFit = new List<int>(nonConform);
            List<int> aFits = new List<int>();
            //loop through the nonconformities to find where they fit in the skin
            for(int a = 0; a < nonConform.Count; a++){
                // break;
                // Debug.Log("nonConformity: "+nonConform[a]);

                //if this nonConform was in the position behind first
                if(neighborSkin(nonConform[a],1) == sortedSkinInds[0]){
                    // Debug.Log("remade the first skin at index " + neighborSkin(nonConform[a],1));
                    DestroyImmediate(children[sortedSkinInds[0]].GetComponent<Collider2D>());
                    createCollider(sortedSkinInds[0], sortedSkinInds[1], sortedSkinInds[skinInd-1]);
                }

                //loop through the possible positions in the sorted array this deformity could go
                for(int i = 0; i < skinInd; i++){

                    int nextNext = GetValidIndex(skinInd, i, 1);
                    bool fit = createCollider(nonConform[a], sortedSkinInds[i], -2);
                    // if(nonConform[a]==39 && sortedSkinInds[i] == 28){ Debug.Log("28 initiallly fits?..."+fit); return; }
                    if(fit){
                        //reconstruct one further because the scan direction comes from beginning but we want it to point to end
                        DestroyImmediate(curCollid); //this is only legit bc I just made this one
                        fit = createCollider(nonConform[a], sortedSkinInds[nextNext], -2);
                        if(fit){
                            // Debug.Log("nonConform "+nonConform[a]+" fits at "+sortedSkinInds[nextNext]);

                            aFits.Add(nextNext);
                            DestroyImmediate(curCollid);
                        }
                    }
                }
                
                //find the closest of the slots the deformity fits in
                if(aFits.Count > 0){
                    int closestFit = aFits[0];
                    float closestDist = Vector2.Distance(vertices[sortedSkinInds[closestFit]], vertices[nonConform[a]]);
                    closestDist += Vector2.Distance(vertices[sortedSkinInds[GetValidIndex(skinInd, closestFit, -1)]], vertices[nonConform[a]]);
                    float testDist;
                    for(int fitsInd = 1; fitsInd < aFits.Count; fitsInd++){
                        testDist = Vector2.Distance(vertices[sortedSkinInds[aFits[fitsInd]]], vertices[nonConform[a]]);
                        testDist += Vector2.Distance(vertices[sortedSkinInds[GetValidIndex(skinInd, aFits[fitsInd], -1)]], vertices[nonConform[a]]);
                        if(testDist < closestDist){
                            closestFit = aFits[fitsInd];
                            closestDist = testDist;
                        }
                        // if(nonConform[a] == 39) Debug.Log("closest fit is "+closestFit+"("+sortedSkinInds[closestFit]+" unsorted)");
                    }

                    createCollider(nonConform[a], sortedSkinInds[closestFit], -1);

                    insertInArray(sortedSkinInds, closestFit, nonConform[a]); // test if this method is working as expected
                    skinInd++;
                    unFit.Remove(nonConform[a]);

                    //recreate the collider that leads to the new one
                    int i = GetValidIndex(skinInd, closestFit, -1);
                    DestroyImmediate(children[sortedSkinInds[i]].GetComponent<Collider2D>());
                    createCollider(sortedSkinInds[i], nonConform[a], -1);
                    Debug.Log("nonConform "+nonConform[a]+" fits between "+sortedSkinInds[GetValidIndex(skinInd, closestFit, -1)]+" and "+sortedSkinInds[closestFit+1]);
                }
                aFits.Clear();

                // break;
            }
            // Debug.Log("beginning of sorted: "+sortedSkinInds[0]+", "+sortedSkinInds[1]+", "+ sortedSkinInds[2]);
            
            Physics2D.SyncTransforms();

            int closest;

            //make regular circle colliders for the unFit and create the few joints that are independent of other skin
            for(int i = 0; i < unFit.Count; i++){
                goodSkin = false; //mark goodSkin for the end of autoskinner

                curCollid = children[unFit[i]].AddComponent<CircleCollider2D>();
                ((CircleCollider2D)curCollid).radius = skinThickness/2;

                Vector2 transCenter = children[unFit[i]].transform.InverseTransformPoint(curCollid.bounds.center);
                closest = closestVertebrae(unFit[i]);

                curJoint = children[unFit[i]].AddComponent<SpringJoint2D>();
                curJoint.connectedBody = children[closest].GetComponent<Rigidbody2D>();
                ((SpringJoint2D)curJoint).dampingRatio = bounceDampening;
                ((SpringJoint2D)curJoint).frequency = springStrength*6;
                curJoint.anchor = transCenter;
                curJoint.enableCollision = true;

                //create the friction for this segment if its enabled
                if(frictionEnabled){
                    curJoint = children[unFit[i]].AddComponent<FrictionJoint2D>();
                    curJoint.connectedBody = children[closest].GetComponent<Rigidbody2D>();
                    ((FrictionJoint2D)curJoint).maxForce = internalFriction;
                    ((FrictionJoint2D)curJoint).maxTorque = internalFriction*2;
                    curJoint.anchor = transCenter;
                    curJoint.enableCollision = true;
                    curJoint.autoConfigureConnectedAnchor = true;
                }
                Debug.Log("vertex "+ unFit[i]+" didnt fit anywhere in the skin so it has been created with reduced accuracy");
            }
            

            //loop through the vertices again to configure skin joints to eachother now that the colliders have been made
            Bounds oBounds;
            int twoBehind;
            //the longer this is the more likely jitter is to occur it seems
            float hypot = Mathf.Sqrt(Mathf.Pow(mesh.bounds.size.x, 2) + Mathf.Pow(mesh.bounds.size.y, 2))/2;
            int closestBack;
            int closestFront;
            // Debug.Log("hypot is: "+hypot);
            lastInd = start;
            for(int i = 0; i < skinInd; i++){

                //set the variables for the current segment
                lastInd = sortedSkinInds[GetValidIndex(skinInd, i, -1)];
                nextInd = sortedSkinInds[GetValidIndex(skinInd, i, 1)];
                curCollid = children[sortedSkinInds[i]].transform.GetComponent<Collider2D>();
                oBounds = children[nextInd].transform.GetComponent<Collider2D>().bounds;

                //check if this segment should mark the autoskinner end
                if(((BoxCollider2D)curCollid).size.y+(((BoxCollider2D)curCollid).edgeRadius*2) < skinThickness)
                    goodSkin = false;

                //set the front and back anchor
                Vector2 transCenter = children[sortedSkinInds[i]].transform.InverseTransformPoint(curCollid.bounds.center);
                float width = (((BoxCollider2D)curCollid).size.x+((BoxCollider2D)curCollid).edgeRadius)/2;
                backAnchor = new Vector2(transCenter.x-width, transCenter.y);
                frontAnchor = new Vector2(transCenter.x+width, transCenter.y);

                //make psuedo distance joints so colliders dont fall out the skin
                curJoint = children[sortedSkinInds[i]].AddComponent<SpringJoint2D>();
                ((SpringJoint2D)curJoint).dampingRatio = 1;
                ((SpringJoint2D)curJoint).frequency = 13.01F; //1M is max lowered for rare cases where dampening is not enough
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
                closestBack = closestVertebrae(children[sortedSkinInds[i]].transform.TransformPoint(backAnchor));
                closestFront = closestVertebrae(children[sortedSkinInds[i]].transform.TransformPoint(frontAnchor));
                
                //create the friction for this segment if its enabled
                if(frictionEnabled){
                    curJoint = children[sortedSkinInds[i]].AddComponent<FrictionJoint2D>();
                    curJoint.connectedBody = children[closestBack].GetComponent<Rigidbody2D>();
                    ((FrictionJoint2D)curJoint).maxForce = internalFriction;
                    ((FrictionJoint2D)curJoint).maxTorque = internalFriction*2;
                    curJoint.anchor = transCenter;
                    curJoint.enableCollision = true;
                    curJoint.autoConfigureConnectedAnchor = true;
                }

                //set up the Rayjoin class on the segment so it can run on its own
                List<GameObject> rayExclude = new List<GameObject>();
                foreach(int num in innerVerts)
                    rayExclude.Add(children[num]);
                rayExclude.Add(children[lastInd]);
                rayExclude.Add(children[nextInd]);
                children[sortedSkinInds[i]].AddComponent<RayJoin>().Setup(clockwise, rayExclude, hypot,springStrength, bounceDampening);


                //do some vector math to calculate how this segment should join to the spine
                //back
                backConnect = children[sortedSkinInds[i]].transform.InverseTransformPoint(children[closestBack].transform.position);
                backConnect = new Vector2((backConnect.x+backAnchor.x)/2,backConnect.y);
                backConnect = children[sortedSkinInds[i]].transform.TransformPoint(backConnect);
                backConnect = children[closestBack].transform.InverseTransformPoint(backConnect);
                //front
                frontConnect = children[sortedSkinInds[i]].transform.InverseTransformPoint(children[closestFront].transform.position);
                frontConnect = new Vector2((frontConnect.x+frontAnchor.x)/2,frontConnect.y);
                frontConnect = children[sortedSkinInds[i]].transform.TransformPoint(frontConnect);
                frontConnect = children[closestFront].transform.InverseTransformPoint(frontConnect);

                //configure joints
                //connect curChild to the spine
                curJoint = children[sortedSkinInds[i]].AddComponent<SpringJoint2D>();
                curJoint.connectedBody = children[closestBack].GetComponent<Rigidbody2D>();
                ((SpringJoint2D)curJoint).dampingRatio = bounceDampening;
                ((SpringJoint2D)curJoint).frequency = springStrength;
                curJoint.anchor = backAnchor;
                curJoint.connectedAnchor = backConnect;
                curJoint.enableCollision = true;

                curJoint = children[sortedSkinInds[i]].AddComponent<SpringJoint2D>();
                curJoint.connectedBody = children[closestFront].GetComponent<Rigidbody2D>();
                ((SpringJoint2D)curJoint).dampingRatio = bounceDampening;
                ((SpringJoint2D)curJoint).frequency = springStrength;
                curJoint.anchor = frontAnchor;
                curJoint.connectedAnchor = frontConnect;
                curJoint.enableCollision = true;

            
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
            scaler.localScale = new Vector3(xScale, yScale, 1);

            //move the scaler so the center of its colliders is at the same place as the center of the main picture
            // Bounds compCollidBounds = GetWholeBounds(mainObj, true);
            // scaler.position += rederBounds.center - compCollidBounds.center;
            scaler.localPosition = new Vector3(0,0);
        }
    }
    
            
    //find the closest vertebrae to attach to
    int closestVertebrae(int index, List<int> exclusions = null){
        int? closest = null;
        for(int a = 0; a < innerVerts.Count; a++)
            if(innerVerts[a] != index){
                if(exclusions == null || !exclusions.Contains(innerVerts[a])){
                    if(closest == null) closest = a;
                    else if(Vector2.Distance(vertices[index], vertices[innerVerts[a]]) < Vector2.Distance(vertices[index], vertices[innerVerts[(int)closest]]))
                        closest = a;
                }
            }
        if(closest == null) return -1;
        return innerVerts[(int)closest];
    }
    
    int closestVertebrae(Vector2 pt){
        Vector2 transPt = scaler.InverseTransformPoint(pt);
        int closest = 0;
        for(int a = 1; a < innerVerts.Count; a++)
            if(Vector2.Distance(transPt, vertices[innerVerts[a]]) < Vector2.Distance(transPt, vertices[innerVerts[(int)closest]]))
                closest = a;
        return innerVerts[(int)closest];
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


    private void Clear(){
        // Debug.ClearDeveloperConsole();
        if(sprite != null){
            mesh = null;
            mat = null;
        }

        Transform parts = transform.Find("parts");
        if(parts != null)
            DestroyImmediate(parts.gameObject);

        //reset serialized storage fields
        goodSkin = true;
        vertices = new Vector3[0];
        sortedSkinInds = new int[0];
        innerVerts.Clear();
        children = new GameObject[0];
    }
    void updateValues(){
        picture.GetComponent<Rigidbody2D>().WakeUp();

        //sync the values for the spine
        Rigidbody2D bod;
        SpringJoint2D[] springs;
        Collider2D collid;
        FrictionJoint2D frict;
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
            
            //friction values
            frict = children[innerVerts[a]].GetComponent<FrictionJoint2D>();
            if(frict != null){
                frict.maxForce = internalFriction;
                frict.maxTorque = internalFriction*2;
            }
            else if(frictionEnabled){
                frict = children[innerVerts[a]].AddComponent<FrictionJoint2D>();
                frict.connectedBody = picAnchor;
                frict.maxForce = internalFriction;
                frict.maxTorque = internalFriction*2;
                frict.autoConfigureConnectedAnchor = true;
            }
        }

        //sync the values for the skin
        for(int i = 0; i < sortedSkinInds.Length; i++){

            //rigidbody values: mass
            bod = children[sortedSkinInds[i]].GetComponent<Rigidbody2D>();
            if(bod != null){
                bod.mass = skinSegMass;
                bod.WakeUp();
            }

            //spring values: spring strength and dampening
            springs = children[sortedSkinInds[i]].GetComponents<SpringJoint2D>();
            for(int b = 0; b < springs.Length; b++){
                if(springs[b].frequency <= 13){ //it would be more than 13 if this spring were actually a psuedo-distance joint
                    springs[b].frequency = springStrength;
                    springs[b].dampingRatio = bounceDampening;
                }
            }

            //collider values: skin thickness, edge radius, and physics material
            collid = children[sortedSkinInds[i]].GetComponent<BoxCollider2D>();
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

                collid.sharedMaterial = physicsMaterial;
            }

            //friction values
            frict = children[sortedSkinInds[i]].GetComponent<FrictionJoint2D>();
            if(frict != null){
                frict.maxForce = internalFriction;
                frict.maxTorque = internalFriction*2;
            }
            else if(frictionEnabled){
                Vector2 transCenter = children[sortedSkinInds[i]].transform.InverseTransformPoint(collid.bounds.center);
                int closestBack = closestVertebrae(collid.bounds.center);

                frict = children[sortedSkinInds[i]].AddComponent<FrictionJoint2D>();
                frict.connectedBody = children[closestBack].GetComponent<Rigidbody2D>();
                frict.maxForce = internalFriction;
                frict.maxTorque = internalFriction*2;
                frict.anchor = transCenter;
                frict.enableCollision = true;
                frict.autoConfigureConnectedAnchor = true;
            }
        }
    }

    void Update(){
        if(scaler != null && children.Length > 0){
            //this small bit just keeps the mesh stretched to fit the colliders as they move
            for(int i = 0; i < children.Length; i++){
                vertices[i] = children[i].transform.localPosition;
                // Vector2 backThirdDrirect = new Vector2(Mathf.Cos(Mathf.Deg2Rad*(parts.GetChild(i).localEulerAngles.z+60)),Mathf.Sin(Mathf.Deg2Rad*(parts.GetChild(i).localEulerAngles.z+60)));
                // Debug.DrawRay(parts.GetChild(i).position, backThirdDrirect, Color.red);
            }
            mesh.vertices = vertices; 

            //this is the section that keeps the inspector changes synced up with the actual gameobjects after theyve been made
            if(updateLive){
                updateValues();
            }
        }
    }
    
    #if UNITY_EDITOR
    [SerializeField, HideInInspector] private Sprite lastSprite; //save of the last sprite that was set to caompare against to test for change
    void OnValidate(){
        if(sprite != null){
            if(sprite != lastSprite){
                UnityEditor.EditorApplication.delayCall+=()=>{
                    AutoSkin();
                    lastSprite = sprite;
                };
            }
            updateValues();
        }
        else if(children.Length > 0) UnityEditor.EditorApplication.delayCall+=Clear;
    }
    #endif
}
