using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

[ExecuteInEditMode]
public class RandomReplace : MonoBehaviour
{
    public List<GameObject> possibles = new List<GameObject>();
    public float minScale = 1;
    public float maxScale = 0;
    public int SortLayer = 0;
    public bool addBody = false;
    private GameObject owned;
    [SerializeField] private bool run = false;
    private float lastTouch;

    void Update(){
        if(run && possibles.Count > 0){
            if(owned == null){
                owned = Instantiate(possibles[UnityEngine.Random.Range(0, possibles.Count-1)],transform.position,transform.rotation);
                
                if(maxScale != 0 && maxScale > minScale){
                    TranRandSeed trs = owned.GetComponent<TranRandSeed>();
                    if(trs == null) trs = owned.AddComponent<TranRandSeed>();
                    trs.minSize = minScale;
                    trs.maxSize = maxScale;
                    trs.run = true;
                }

                if(SortLayer != 0){
                    SpriteRenderer sr = owned.GetComponent<SpriteRenderer>();
                    if(sr != null){
                        sr.sortingOrder = SortLayer;
                    }

                    if(addBody){
                        if(owned.GetComponent<Rigidbody2D>() == null)
                            owned.AddComponent<Rigidbody2D>();
                    }
                }
            }
            else{
                owned.transform.position = transform.position;
            }

            lastTouch = Time.realtimeSinceStartup;
            selfDestruct();
        }
    }

    async void selfDestruct(){
        await Task.Delay(151);
        //check if the update hasnt run in atleast .15 seconds
        if(this != null && Time.realtimeSinceStartup-lastTouch >= .15F){
            #if UNITY_EDITOR
                // Register the creation in the undo system and select the new object
                UnityEditor.Undo.RegisterCreatedObjectUndo(owned, "Create " + owned.name);
                if(owned != null) UnityEditor.Selection.activeGameObject = owned;
                DestroyImmediate(gameObject);
                return;
            #endif
            Destroy(gameObject);
        }
    }
}
