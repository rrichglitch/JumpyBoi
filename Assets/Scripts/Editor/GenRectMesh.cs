using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class GenRectMesh : EditorWindow
{
    private Texture tex;
    private string saveName = "_";
    private int smallBoxes = 3; // the number of boxes across the short side of the texture
    private float textureScale = .1F;
    private string savePath {get{return "Assets/Generated/Meshes/"+saveName+".asset";}}
    public Mesh mesh;
    private bool ht;
    private List<(int, int, int)> sharedVerts; //a list of indexes of the vertebrae with the points they share on the perimeter
    private int vertebrae;
    Vector3[] vertices;
    Vector2[] uv;
    int[] triangles;

    // private static EditorWindow window;
    
    // void Awake(){
    //     if (Application.isEditor && !Application.isPlaying)
    //         GenerateMesh();
    // }

    [MenuItem("Window/GenRectMesh")]
    public static void ShowWindow(){
        GetWindow<GenRectMesh>("GenRectMesh");
    }

    void OnGUI(){
        tex = (Texture)EditorGUILayout.ObjectField("Texture", tex, typeof(Texture), true);
        saveName = EditorGUILayout.TextField("saveName", saveName);
        smallBoxes = EditorGUILayout.IntField("smallBoxes", smallBoxes);
        textureScale = EditorGUILayout.FloatField("textureScale", textureScale);
        if(GUILayout.Button("Generate!")){
            Debug.Log(tex);
            Debug.Log(saveName);
            Debug.Log(smallBoxes);
            Debug.Log(textureScale);
            GenerateMesh();
        }
    }

    [ContextMenu("Generate Mesh")]
    void GenerateMesh(){
        if(tex != null){
            //all field thats arent interactabnle should be initialize here
            //so that different runs dont interfere with eachother
            mesh = new Mesh();
            sharedVerts = new List<(int, int, int)>();
            int hBoxes;
            int wBoxes;
            float boxH;
            float boxW;
            int tallBoxes;
            float texSD;
            float texTD;

            //this is the ratio to transform the verticies tall dimension to the textures tall dimension
            float transRatio;

            if(tex.height > tex.width){
                texTD = tex.height;
                texSD = tex.width;
                ht = true;
            }
            else{
                texSD = tex.height;
                texTD = tex.width;
                ht = false;
            }
            Debug.Log("OG dimensions: "+ tex.width+", "+tex.height);

            //scale the size the mesh will be
            texTD *= textureScale;
            texSD *= textureScale;

            float boxS = texSD/smallBoxes;

            tallBoxes = Mathf.RoundToInt(texTD/boxS);

            //this is the ratio to transform the verticies tall dimension to the textures tall dimension
            transRatio = (texTD/texSD)/Mathf.RoundToInt(texTD/texSD);

            if(ht){
                boxH = texTD/tallBoxes;
                boxW = boxS;
                hBoxes = tallBoxes;
                wBoxes = smallBoxes;
            }
            else{
                boxW = texTD/tallBoxes;
                boxH = boxS;
                wBoxes = tallBoxes;
                hBoxes = smallBoxes;
            }

            vertebrae = Mathf.RoundToInt(texTD/texSD)-1;
            int len = (wBoxes+hBoxes)*2 + vertebrae;
            // Debug.Log(vertebrae+" total vertebrae");

            vertices = new Vector3[len];
            uv = new Vector2[len];
            triangles = new int[3*(vertices.Length-vertebrae+((vertebrae-1)*2))];

            int ind = 0;
            //make the "spine" or inner verticies
            float mid = texSD/2;
            float pad = texTD/(vertebrae+1);
            if(ht)
                for(; ind < vertebrae; ind++)
                    vertices[ind] = new Vector3(mid, (ind+1)*pad);
            else
                for(; ind < vertebrae; ind++)
                    vertices[ind] = new Vector3((ind+1)*pad, mid);

            // Debug.Log("spine: " + vertices[0]+", "+vertices[1]);

            //make the perimeter verticies and index them in a cloackwise manner
            if(ht){
                //top: L2R
                for(int w = 0; w<=wBoxes; w++){
                    vertices[ind] = new Vector3(w*boxW, texTD);
                    ind++;
                }
                //right: T2B
                for(int h = hBoxes-1; h>=0; h--){
                    vertices[ind] = new Vector3(texSD, h*boxH);
                    if(vertebrae > 1)
                        shareCheck(ind-1);
                    ind++;
                }
                //bottom: R2L
                for(int w = wBoxes-1; w>=0; w--){
                    vertices[ind] = new Vector3(w*boxW, 0);
                    ind++;
                }
                //left: B2T
                for(int h = 1; h<hBoxes; h++){
                    vertices[ind] = new Vector3(0, h*boxH);
                    if(vertebrae > 1)
                        shareCheck(ind-1);
                    ind++;
                }
                shareCheck(ind-1);
            }
            else{
                //top: L2R
                for(int w = 0; w<=wBoxes; w++){
                    vertices[ind] = new Vector3(w*boxW, texSD);
                    if(vertebrae > 1)
                        shareCheck(ind-1);
                    ind++;
                }
                //right: T2B
                for(int h = hBoxes-1; h>=0; h--){
                    vertices[ind] = new Vector3(texTD, h*boxH);
                    ind++;
                }
                //bottom: R2L
                for(int w = wBoxes-1; w>=0; w--){
                    vertices[ind] = new Vector3(w*boxW, 0);
                    if(vertebrae > 1)
                        shareCheck(ind-1);
                    ind++;
                }
                //left: B2T
                for(int h = 1; h<hBoxes; h++){
                    vertices[ind] = new Vector3(0, h*boxH);
                    ind++;
                }
                shareCheck(ind-1);
            }

            //clone the verticies on to the uv
            if(ht)
                for(int i = 0; i<vertices.Length; i++){
                    uv[i] = new Vector2(vertices[i].x/texSD, vertices[i].y/texTD);
                }
            else
                for(int i = 0; i<vertices.Length; i++){
                    uv[i] = new Vector2(vertices[i].x/texTD, vertices[i].y/texSD);
                }

            //set the triangle[]
            int triInd = 0;

            //set the triangles for the perimeter when there is only one vertebrae
            if(vertebrae == 1){
                Debug.Log("omae wa mou shinderu");
                for(int a = vertebrae; a< vertices.Length; a++){

                    triangles[triInd] = a;
                    Debug.Log(triangles[triInd]);
                    triInd++;

                    if(a <= vertices.Length-1) triangles[triInd] = vertebrae;
                    else triangles[triInd] = a+1;
                    Debug.Log(triangles[triInd]);
                    triInd++;
                    
                    triangles[triInd] = 0;
                    Debug.Log(triangles[triInd]);
                    triInd++;
                }
            }

            //set the inner triangles if theres a spine
            // Debug.Log("sharedVerts.Count == "+ sharedVerts.Count);
            for(int i = 0; i<sharedVerts.Count; i++){

                if(i+1 >= sharedVerts.Count) triangles[triInd] = sharedVerts[0].Item1;
                else triangles[triInd] = sharedVerts[i+1].Item1;
                // Debug.Log(triangles[triInd]);
                triInd++;
                
                triangles[triInd] = sharedVerts[i].Item1;
                // Debug.Log(triangles[triInd]);
                triInd++;

                triangles[triInd] = sharedVerts[i].Item2;
                // Debug.Log(triangles[triInd]);
                triInd++;
            }
            
            // Debug.Log("triangle Index after spine: "+ triInd);
            // for(int i =0; i<sharedVerts.Count;i++)
            //     Debug.Log("shared 1:"+sharedVerts[i]);

            //set the triangles for the perimeter when there is a spine
            //go through the strips in between shared vertexes and triangulate them
            //loop through shared vertices
            int startInd;
            for( int i = 0; i < sharedVerts.Count; i++){
                    // Debug.Log("vertebrae: "+i+": "+sharedVerts[i]);

                if(i-1 < 0) startInd = sharedVerts.Count-1;
                else startInd = i-1;
                //loop through vertexes till reaching the next shared vertex
                //special for loop that can go 360 degrees, a little riskier for an infinite loop
                for(int a = sharedVerts[startInd].Item2; a != sharedVerts[i].Item2; a++){
                    
                    // Debug.Log("tiangle index: "+triInd);
                    triangles[triInd] = a;
                    // Debug.Log(triangles[triInd]);
                    triInd++;

                    if(a >= vertices.Length-1) triangles[triInd] = vertebrae;
                    else triangles[triInd] = a+1;
                    // Debug.Log(triangles[triInd]);
                    triInd++;
                    
                    triangles[triInd] = sharedVerts[i].Item1;
                    // Debug.Log(triangles[triInd]);
                    triInd++;

                    //allows loop to continue when coming from the end around to the beginning
                    if(a+1 >= vertices.Length) a = vertebrae-1;
                }
            } 

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;

            AssetDatabase.CreateAsset(mesh, savePath);
            AssetDatabase.SaveAssets();
        }
        else Debug.Log("no texture assigned!");
    }

    //method used to vet and fill the sharedVerts list
    void shareCheck(int ind){
        // Debug.Log("checked "+ind);
        //find the closest vertebrae
        int next;
        if(ind >= vertices.Length-1) next = vertebrae;
        else next = ind+1;
        int closest=0;
        int sClosest=1;
        float bcDist = Vector2.Distance(vertices[closest], vertices[ind]);
        float bscDist = Vector2.Distance(vertices[sClosest], vertices[ind]);
        
        //closest should prefer leaning backwards to help with the triangle generation
        if(bscDist == bcDist){
            if(Vector2.Distance(vertices[sClosest], vertices[ind-1]) < Vector2.Distance(vertices[closest], vertices[ind-1])){
                // Debug.Log("leaned back!");
                int save = closest;
                closest = sClosest;
                sClosest = save;
            }
        }
        //sort which of the beginner 2 is correct
        else if(bscDist < bcDist){
            int save = closest;
            closest = sClosest;
            sClosest = save;
        }
        
        // if(closest > 1) Debug.Log(closest+"after initial closing");

        //find vertebrae closer than the beginner 2
        for(int i = 2; i<vertebrae; i++){
            // Debug.Log("more than 2 vertebrae");
            if(Vector2.Distance(vertices[i], vertices[ind]) < Vector2.Distance(vertices[sClosest], vertices[ind])){
                sClosest = i;
                //swap the closest and second closest if appropriate
                if( Vector2.Distance(vertices[sClosest], vertices[ind]) < Vector2.Distance(vertices[closest], vertices[ind])
                || Vector2.Distance(vertices[sClosest], vertices[ind-1]) < Vector2.Distance(vertices[closest], vertices[ind-1])){
                    int save = closest;
                    closest = sClosest;
                    sClosest = save;
                }
            }
        }
        // if(closest > 1) Debug.Log(closest+" after close changes");

        Vector2 mid = (vertices[closest] + vertices[sClosest])/2;
        float mDist = Vector2.Distance(mid, vertices[ind]);
        float lDist = Vector2.Distance(mid, vertices[ind-1]);
        float nDist = Vector2.Distance(mid, vertices[next]);
        // float cDist = Vector2.Distance(vertices[closest], vertices[ind]);
        // float scDist = Vector2.Distance(vertices[sClosest], vertices[ind]);
        // Debug.Log(ind+": "+vertices[ind]);
        // Debug.Log(mDist);

        //if the closest is not the same
        //if this vertex is closer to the middle than its neighboring verteces then add this as a point shared with the vertebrae
        (int,int,int) lShared;
        if(mDist <= lDist && mDist <= nDist){
            // Debug.Log("can share: "+ind);
            //check to see if the last neighboring vertex has covered sharing with the spine already
            if(sharedVerts.Count > 0){
                lShared = sharedVerts[sharedVerts.Count-1];
                if( lShared.Item2 == ind-1
                && (lShared.Item1 == closest || lShared.Item1 == sClosest)
                && (lShared.Item3 == closest || lShared.Item3 == sClosest)){
                    // Debug.Log(shared.Item2+" -> "+ind);
                    // sharedVerts[sharedVerts.Count-1] = (closest, ind, sClosest);
                    return;
                }
            }
            // if(lDist == nDist)
            bcDist = Vector2.Distance(vertices[closest], vertices[ind]);
            bscDist = Vector2.Distance(vertices[sClosest], vertices[ind]);

            // Debug.Log(vertices[ind]);
            // Debug.Log(lDist+" > "+mDist+" < "+nDist);
            // Debug.Log(bcDist+" < "+bscDist);
            sharedVerts.Add((closest, ind, sClosest));
        }
    }
}
