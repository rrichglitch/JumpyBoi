using UnityEngine;

[ExecuteInEditMode]
public class TranRandSeed : MonoBehaviour
{
    public float minSize = 1F;
    public float maxSize = 0F;
    public int minRotate = 0;
    public int maxRotate = 0;
    public bool run = false;

    void Start(){
        if(run){
            if(maxSize > minSize){
                float newSize = minSize+(Mathf.Lerp(minSize, maxSize, Random.value));
                transform.localScale = new Vector3(newSize,newSize,newSize);
                transform.localScale = new Vector3(newSize,newSize,newSize);
            }
            if(maxRotate > minRotate){
                Vector3 toRotate = transform.eulerAngles;
                toRotate.z = Random.Range(minRotate,maxRotate);
                transform.eulerAngles = toRotate;
            }
            
            if(Application.isEditor) DestroyImmediate(this);
            else Destroy(this);
        }
    }
}
