using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DOFController : MonoBehaviour
{
    //variable string controlled angle
    [SerializeField] private string _controlledAngle = "x";
    //variable float angle
    [SerializeField] private float _angle = 0f;
    [SerializeField] private float _sensitivity = 1f;
    //bool active
    [SerializeField] private bool _active = true;
    [SerializeField] private bool _lerp = true;
    [SerializeField] private bool _upsideDownFix = false;
    private float _angleOffset = 0f;
    // Start is called before the first frame update
    void Start()
    {
        //get controlled angle value
        if(_controlledAngle == "x"){
            _angleOffset = transform.localEulerAngles.x;
        }
        if(_controlledAngle == "y"){
            _angleOffset = transform.localEulerAngles.y;
        }
        if(_controlledAngle == "z"){
            _angleOffset = transform.localEulerAngles.z;
        }
    }

    public void SetAngle(float angle){
        //set angle
        _angle = angle;
    }

    //getangle
    public float GetAngle(){
        if(_controlledAngle == "x"){
            return transform.localEulerAngles.x;
        }
        if(_controlledAngle == "y"){
            return transform.localEulerAngles.y;
        }
        if(_controlledAngle == "z"){
            return transform.localEulerAngles.z;
        }
        return 0f;
    }

    // Update is called once per frame
    void Update()
    {
        //if active set transform angle depending on controlledangle
        if(_active){
            _angle *= _sensitivity;
            if(_controlledAngle == "x"){
                if(_upsideDownFix){
                    if(transform.localEulerAngles.x <= -150f){
                        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, transform.localEulerAngles.z);
                    }
                }
                if(_lerp)
                    transform.localEulerAngles = new Vector3(Mathf.LerpAngle(transform.localEulerAngles.x, _angle + _angleOffset, 0.1f), transform.localEulerAngles.y, transform.localEulerAngles.z);
                else
                    transform.localEulerAngles = new Vector3(_angle + _angleOffset, transform.localEulerAngles.y, transform.localEulerAngles.z);
            }
            if(_controlledAngle == "y"){
                if(_lerp)
                    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, Mathf.LerpAngle(transform.localEulerAngles.y, _angle + _angleOffset, 0.1f), transform.localEulerAngles.z);
                else
                    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, _angle + _angleOffset, transform.localEulerAngles.z);
            }
            if(_controlledAngle == "z"){
                if(_lerp)
                    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, Mathf.LerpAngle(transform.localEulerAngles.z, _angle + _angleOffset, 0.1f));
                else
                    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, _angle + _angleOffset);
            }
        }
    }
}
