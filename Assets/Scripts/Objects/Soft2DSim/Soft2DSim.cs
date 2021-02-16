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
    [Range(2,6)] public int consideredIn = 4; //this is the threshhold of number of triangles a vertex must be in the be considered an inner vertex
    public bool solidSpine = true; //are the spine segments in a fixed place
    [MinAttribute(0)] public float vertebraeMass = 1;
    [MinAttribute(0)] public float spineStrength = 5;
    [Range(0,1)] public float spineDampening = .9F;
    public bool internalFriction = false; //should energy be held in the soft object?(should deformations be more permanent?)
    [MinAttribute(0)] public float frictionStrength = 1;
    [MinAttribute(0)] public float skinThickness = .3F;
    private float squarEdgeRad{get{return skinThickness/4;}}
    public PhysicsMaterial2D physMat; //set this should the skin use a speciific physics material
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
                if(i==0){
                    curJoint = curChild.AddComponent<FixedJoint2D>();
                    curJoint.connectedBody = contBody;
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
        #endregion

        #region create skin
            //I dont feel like rewriting all the calls for the newer method implimentation so Ill just use this wrapper
            int neighborSkin(int ind, int skinDist){
                return GetValidIndex(vertices.Length, ind, skinDist, innerVerts);
            }

            int start = neighborSkin(vertices.Length-1,1);

            //set up variables that are used often henceforth
            int lastInd = start;
            int nextInd;
            Vector2 toward;

            //find the closest vertebrae to attach to
            int closestVertebrae(int index){
                int closest = 0;
                for(int a = 1; a < innerVerts.Count; a++)
                    if(Vector2.Distance(vertices[index], vertices[innerVerts[a]]) < Vector2.Distance(vertices[index], vertices[innerVerts[closest]]))
                        closest = a;
                return innerVerts[closest];
            }

            //calculate whether the vertices are indexed clockwise or counter based off the first vertices
            nextInd = neighborSkin(start, 1);
            toward = (vertices[nextInd] - vertices[start]).normalized;
            Vector2 toVertebrae = (vertices[closestVertebrae(start)] - vertices[start]).normalized;
            Vector2 difFromPerp = Vector2.Perpendicular(toVertebrae) - toward;
            bool clockwise = (difFromPerp.magnitude < 1);
            // Debug.Log("this mesh is clockwise?..."+clockwise);

            //create and join the skin colliders
            //this assumes the vertices have been indexed in a clockwise order
            float skinGap;
            Vector2 mid;
            Vector2 frontAnchor;
            Vector2 backAnchor;
            Vector2 frontConnect;
            Vector2 backConnect;
            for(int i = start; i < vertices.Length && lastInd <= i; i = nextInd){
                
                //create the actual gameobject and rigidbody for this skin segment
                curChild = new GameObject("part "+i, typeof(Rigidbody2D));
                curChild.transform.parent = scaler.transform;
                curChild.transform.localScale = new Vector3(1,1,1);
                children[i] = curChild;
                curChild.transform.localPosition = vertices[i];
                curChild.GetComponent<Rigidbody2D>().mass = partMass;

                //set the variables for the current segment
                lastInd = neighborSkin(i,-1);
                nextInd = neighborSkin(i, 1);
                skinGap = Vector2.Distance(vertices[i], vertices[nextInd]);
                mid = curChild.transform.parent.TransformPoint((vertices[i]+vertices[nextInd])/2);
                toward = scaler.transform.TransformDirection(vertices[nextInd] - vertices[i]);

                //make the collider for this segment
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
                if(physMat != null) curCollid.sharedMaterial = physMat;
                

                //find the closest vertebrae to attach to
                int closest = closestVertebrae(i);
                
                //create the friction for this segment if its enabled
                if(internalFriction){
                    curJoint = curChild.AddComponent<FrictionJoint2D>();
                    curJoint.connectedBody = children[closest].GetComponent<Rigidbody2D>();
                    ((FrictionJoint2D)curJoint).maxForce = frictionStrength;
                    ((FrictionJoint2D)curJoint).maxTorque = frictionStrength*2;
                    curJoint.anchor = curCollid.bounds.center;
                }

            #region join each side of segment to spine
                //do some vector math so calculate how this segment should join to the spine
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
            #endregion

                lastInd = i;
            }
            
            //loop through the vertices again to finish configuring skin joints to eachother now that the colliders have been made
            Bounds oBounds;
            int twoBehind;
            float hypot = Mathf.Sqrt(Mathf.Pow(mesh.bounds.size.x, 2) + Mathf.Pow(mesh.bounds.size.y, 2));
            // Debug.Log("hypot is: "+hypot);
            lastInd = start;
            for(int i = start; i < vertices.Length && lastInd <= i; i = nextInd){

                //set the variables for the current segment
                lastInd = neighborSkin(i,-1);
                nextInd = neighborSkin(i, 1);
                curCollid = children[i].transform.GetComponent<Collider2D>();
                oBounds = children[nextInd].transform.GetComponent<Collider2D>().bounds;

                //set the front and back anchor
                backAnchor = new Vector2(curCollid.bounds.center.x-(curCollid.bounds.extents.x*.9F), curCollid.bounds.center.y);
                frontAnchor = new Vector2(curCollid.bounds.center.x+(curCollid.bounds.extents.x*.9F), curCollid.bounds.center.y);

                //make distance joints so colliders dont fall out the skin 
                curJoint = children[i].AddComponent<DistanceJoint2D>();
                curJoint.connectedBody = children[nextInd].GetComponent<Rigidbody2D>();
                curJoint.anchor = new Vector2(curCollid.bounds.max.x, curCollid.bounds.center.y);

                //connect curChild to the skin collider that is 2 behind
                twoBehind = neighborSkin(i, -2);
                curJoint = children[i].AddComponent<SpringJoint2D>();
                ((SpringJoint2D)curJoint).dampingRatio = bounceDampening;
                ((SpringJoint2D)curJoint).frequency = springStrength;
                curJoint.connectedBody = children[twoBehind].GetComponent<Rigidbody2D>();
                curJoint.anchor = backAnchor;
                curJoint.connectedAnchor = children[twoBehind].GetComponent<Collider2D>().bounds.center;

                //set up the Rayjoin class on the segment so it can run on its own
                List<int> rayExclude = innerVerts.GetRange(0,innerVerts.Count);
                rayExclude.Add(lastInd);
                rayExclude.Add(nextInd);
                children[i].AddComponent<RayJoin>().Setup(clockwise, rayExclude, hypot,spineStrength, spineDampening);
            
                lastInd = i;
            }
        #endregion
        #endregion

            //re order the children in the scaler accoring to vertex index
            for(int i = 0; i < children.Length; i++)
                children[i].transform.SetSiblingIndex(i);

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
        for(int skinCount = 0; skinCount != endDist && Mathf.Abs(skinCount) < totalLength-1; skinCount += dir){
            ret = (ret + dir)%totalLength;
            if(ret + dir < 0)
                ret = totalLength+dir;
            while(exlusions.Contains(ret) && Mathf.Abs(ret) < totalLength){
                ret = (ret + dir)%totalLength;
                if(ret + dir < 0)
                    ret = totalLength+dir;
            }
        }
        return ret;
    }


    //get the total bounds of an object and its children in world space
    //super useful for scaling purposes
    //pass a second parameter of true to base the bounds on colliders instead of renderers
    Bounds GetMaxBounds(GameObject g, bool colliderBased = false){
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
                        if(dimSave.x > squarEdgeRad*2 && skinThickness > squarEdgeRad*2){
                            ((BoxCollider2D)collid).size = new Vector2(dimSave.x, skinThickness-(squarEdgeRad*2));
                            ((BoxCollider2D)collid).edgeRadius = squarEdgeRad;
                        }
                        else ((BoxCollider2D)collid).size = new Vector2(dimSave.x, skinThickness);

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
