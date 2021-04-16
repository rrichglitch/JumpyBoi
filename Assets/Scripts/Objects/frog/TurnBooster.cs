using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TurnBooster : MonoBehaviour
{
    [Range(0,1)] public float turnSpeeed = .1F;
    private float startRot;
    private float? held = null;
    private Glider glider;

    // Start is called before the first frame update
    void Start(){
        startRot = transform.localEulerAngles.z;
        glider = GetComponent<Glider>();
    }

    public void OnTurnBoooster(InputAction.CallbackContext ctx){
        if(ctx.canceled){
            held = null;
            glider.enabled = false;
        }
        else{
            held = Vector2.SignedAngle(new Vector2(0,1), ctx.ReadValue<Vector2>());
            if(held < 0) held = 360 + held;
            glider.enabled = true;
        }
    }

    void FixedUpdate(){
        Vector3 saveAng;
        if(held != null){
            saveAng = transform.eulerAngles;
            saveAng.z = Mathf.LerpAngle(saveAng.z, (float)held, turnSpeeed);
            transform.eulerAngles = saveAng;
        }
        else{
            saveAng = transform.localEulerAngles;
            saveAng.z = Mathf.LerpAngle(saveAng.z, startRot, turnSpeeed);
            transform.localEulerAngles = saveAng;
        }
    }
}
