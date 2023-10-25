using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiidPoseController : MonoBehaviour
{
    //bool hide
    [SerializeField] private bool _HIDE = false;
    private bool _hideStatus = true;
    //serialize gameobject odileviz
    [SerializeField] private GameObject _odileViz;
    //serialize Evangelion
    [SerializeField] private GameObject _evangelion;
    private EvangelionPoseController _evangelionPoseController;
    //RobotPoseContoller
    private RobotPoseContoller _robotPoseController;
    [SerializeField] private FloatSO QtyOfMovement;
    //serialize lightball
    [SerializeField] private GameObject _lightBall;
    //serialize qtyofmovement
    [SerializeField] private float _qtyOfMovement;
    //serialize max emission intensity
    [SerializeField] private float _maxEmissionIntensity;
    private Color _lightBallEmissionColor;
    //serialize average distance
    [SerializeField] private float _averageDistance;
    //array of 4 DOFController
    [SerializeField] private DOFController[] _dofControllers = new DOFController[4];
    [SerializeField] private float _horizontalEye;
    [SerializeField] private float _verticalEye;
    //gameobject eye
    [SerializeField] private GameObject _eye;
    //serialize eye vertical range
    [SerializeField] private float _eyeVerticalRange = 30f;
    //serialize eye horizontal range
    [SerializeField] private float _eyeHorizontalRange = 120f;
    //serialize eye middle
    [SerializeField] private float _eyeVerticalMiddle = 340f;
    [SerializeField] private float _eyeHorizontalMiddle = 270f;
    //clamp eye
    [SerializeField] private float _eyeClamp = 0.335f;
    // Start is called before the first frame update
    void Start()
    {
        _evangelionPoseController = _evangelion.GetComponent<EvangelionPoseController>();
        _robotPoseController = _odileViz.GetComponent<RobotPoseContoller>();
        //create a copy of lightball emission color
        _lightBallEmissionColor = new Color(_lightBall.GetComponent<Renderer>().material.GetColor("_EmissionColor").r, _lightBall.GetComponent<Renderer>().material.GetColor("_EmissionColor").g, _lightBall.GetComponent<Renderer>().material.GetColor("_EmissionColor").b);
        //get components of trype DOFController in children of this
        _dofControllers = GetComponentsInChildren<DOFController>();

    }

    // Update is called once per frame
    void Update()
    {
        if(_HIDE){
            Hide(_hideStatus);
            _hideStatus = !_hideStatus;
            _HIDE = false;
        }
        //set position and rotation to odileviz
        transform.position = _odileViz.transform.position;
        //rotation odileviz - 135 on y
        transform.rotation = Quaternion.Euler(0, _odileViz.transform.rotation.eulerAngles.y - 135, 0);
        _qtyOfMovement = QtyOfMovement.runtimeValue;
        //multiply ball emission color
        _lightBall.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(_lightBallEmissionColor.r * _qtyOfMovement / _maxEmissionIntensity, _lightBallEmissionColor.g * _qtyOfMovement / _maxEmissionIntensity, _lightBallEmissionColor.b * _qtyOfMovement / _maxEmissionIntensity));

        Vector3 leftHandTracker = _robotPoseController.GetLeftHandTrackerLocalPosition();
        //right handtracker
        Vector3 rightHandTracker = _robotPoseController.GetRightHandTrackerLocalPosition();
        _averageDistance = Vector3.Distance(leftHandTracker, rightHandTracker);
        //resize 0 to 0.6 with 80 to -10
        float _averageAngle = _averageDistance/0.6f * 90;
        //for each DOFController
        foreach (DOFController dofController in _dofControllers)
        {
            //set angle
            dofController.SetAngle(-90f+_averageAngle);
        }
        _horizontalEye = (_robotPoseController.GetLookAngle() - _eyeHorizontalMiddle) / _eyeHorizontalRange;
        //315 to 350, 330 is middle
        _verticalEye = _robotPoseController.GetTiltAngle();
        _verticalEye = (_verticalEye - _eyeVerticalMiddle) / _eyeVerticalRange;
        //clamp between _eyeClamp
        _horizontalEye = Mathf.Clamp(_horizontalEye, -_eyeClamp, _eyeClamp);
        _verticalEye = Mathf.Clamp(_verticalEye, -_eyeClamp, _eyeClamp);
        _eye.transform.localPosition = new Vector3(-_horizontalEye, -_verticalEye, _eye.transform.localPosition.z);
    }

    public void Hide(bool hide){
        if(hide){
            //find all meshes in all children and disable
            foreach (MeshRenderer meshRenderer in GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.enabled = false;
            }
            //spriterenderer
            foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
            {
                spriteRenderer.enabled = false;
            }
        }else{
            foreach (MeshRenderer meshRenderer in GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.enabled = true;
            }
            //spriterenderer
            foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
            {
                spriteRenderer.enabled = true;
            }
        }
    }
}
