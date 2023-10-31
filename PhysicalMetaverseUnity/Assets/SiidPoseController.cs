using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiidPoseController : MonoBehaviour
{
    private PoseManager _poseManager;
    //bool hide
    [SerializeField] private bool _HIDE = false;
    private bool _hideStatus = true;
    //serialize gameobject odileviz
    [SerializeField] private GameObject _odileViz;
    //serialize Evangelion
    [SerializeField] private GameObject _evangelion;
    //y offset
    [SerializeField] private float _yOffset = -1.8f;
    private EvangelionPoseController _evangelionPoseController;
    //RobotPoseContoller
    private RobotPoseContoller _robotPoseController;
    [SerializeField] private FloatSO QtyOfMovement;
    //serialize lightball
    [SerializeField] private GameObject _lightBall;
    //serialize qtyofmovement
    [SerializeField] private float _qtyOfMovement;
    [SerializeField] private float _quantityOfMovementMultiplier = 0.34f;
    //serialize max emission intensity
    [SerializeField] private float _maxEmissionIntensity;
    private Color _lightBallEmissionColor;
    //serialize average distance
    [SerializeField] private float _averageDistance;
    //array of 4 DOFController
    [SerializeField] private DOFController[] _dofControllers = new DOFController[4];
    //dofcontroller eyeY and dofcontroller eyeX
    [SerializeField] private DOFController _dofControllerEyeY;
    [SerializeField] private DOFController _dofControllerEyeX;
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
        _poseManager = PoseManager.Instance;
        _evangelionPoseController = _evangelion.GetComponent<EvangelionPoseController>();
        _robotPoseController = _odileViz.GetComponent<RobotPoseContoller>();
        //create a copy of lightball emission color
        _lightBallEmissionColor = new Color(_lightBall.GetComponent<Renderer>().material.GetColor("_EmissionColor").r, _lightBall.GetComponent<Renderer>().material.GetColor("_EmissionColor").g, _lightBall.GetComponent<Renderer>().material.GetColor("_EmissionColor").b);
        //get components of trype DOFController in children of this
        _dofControllers = GetComponentsInChildren<DOFController>();
        _yOffset = transform.localPosition.y;
    }

    //serialize enum of HANDS DISTANCE, NOSE DISTANCE
    public enum PetalsControlType{
        HANDS,
        NOSE,
        CENTER
    }

    //serialize petals control type
    [SerializeField] private PetalsControlType _petalsControlType = PetalsControlType.NOSE;

    // Update is called once per frame
    void Update()
    {
        //_distance from filtered distance
        _distance = _robotPoseController.GetFilteredDistance();
        
        if(_HIDE){
            Hide(_hideStatus);
            _hideStatus = !_hideStatus;
            _HIDE = false;
        }
        //set position and rotation to odileviz
        transform.position = _odileViz.transform.position + _yOffset * Vector3.up;
        //rotation odileviz - 135 on y
        transform.rotation = Quaternion.Euler(0, _odileViz.transform.rotation.eulerAngles.y - 180f, 0);
        _qtyOfMovement = _poseManager.GetQuantityOfMovement() * _quantityOfMovementMultiplier;
        //multiply ball emission color
        _lightBall.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(_lightBallEmissionColor.r * _qtyOfMovement / _maxEmissionIntensity, _lightBallEmissionColor.g * _qtyOfMovement / _maxEmissionIntensity, _lightBallEmissionColor.b * _qtyOfMovement / _maxEmissionIntensity));

        //if _petalsControlType
        if(_petalsControlType == PetalsControlType.HANDS){
            PetalsAngleWithHandsDistance();
        }else if(_petalsControlType == PetalsControlType.NOSE){
            PetalsAngleWithNoseDistance();
        }
        else if(_petalsControlType == PetalsControlType.CENTER){
            PetalsAngleWithCenterDistance();
        }

        RotateEye();
    }

    void RotateEye(){
        _verticalEye = _poseManager.GetHeadAngleX();
        _horizontalEye = _poseManager.GetHeadAngleY() + 90f;
        _dofControllerEyeY.SetAngle(_horizontalEye);
        _dofControllerEyeX.SetAngle(_verticalEye);
    }
    
    void TraslateEye(){ //OLD Siid
        _horizontalEye = (_poseManager.GetHeadAngleY() - _eyeHorizontalMiddle) / _eyeHorizontalRange;
        //315 to 350, 330 is middle
        _verticalEye = _poseManager.GetHeadAngleX();
        _verticalEye = (_verticalEye - _eyeVerticalMiddle) / _eyeVerticalRange;
        //clamp between _eyeClamp
        _horizontalEye = Mathf.Clamp(_horizontalEye, -_eyeClamp, _eyeClamp);
        _verticalEye = Mathf.Clamp(_verticalEye, -_eyeClamp, _eyeClamp);
        _eye.transform.localPosition = new Vector3(-_horizontalEye, -_verticalEye, _eye.transform.localPosition.z);
    }
    
    //fire hide button
    public void FireHideButton(){
        _HIDE = true;
    }

    private void PetalsAngleWithHandsDistance(){
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
    }

    //distance
    [SerializeField] private float _distance = 0f;
    //Serialize lower nose distance limit
    [SerializeField] private float _lowerNoseDistanceLimit = 0.7f;
    //Serialize upper nose distance limit
    [SerializeField] private float _upperNoseDistanceLimit = 2.2f;
    private void PetalsAngleWithNoseDistance(){
        float correctedUpperNoseDistanceLimit = _upperNoseDistanceLimit / _distance;
        Vector3 noseTracker = _robotPoseController.GetNoseTrackerLocalPosition();
        //right handtracker
        Vector3 rightHandTracker = _robotPoseController.GetRightWristLocalPosition();
        Vector3 leftHandTracker = _robotPoseController.GetLeftWristLocalPosition();
        _averageDistance = Vector3.Distance(noseTracker, rightHandTracker) + Vector3.Distance(noseTracker, leftHandTracker);
        _averageDistance = _averageDistance / 2;
        //clamp between 0.7 and 2.5
        _averageDistance = Mathf.Clamp(_averageDistance, _lowerNoseDistanceLimit, correctedUpperNoseDistanceLimit);
        float _averageAngle = (_averageDistance-_lowerNoseDistanceLimit) * 90 / (correctedUpperNoseDistanceLimit - _lowerNoseDistanceLimit);
        //for each DOFController
        foreach (DOFController dofController in _dofControllers)
        {
            //set angle
            dofController.SetAngle(-90f+_averageAngle);
        }
    }
    //PetalsAngleWithCenterDistance
    //upper and lower limits
    [SerializeField] private float _lowerCenterDistanceLimit = 0.7f;
    [SerializeField] private float _upperCenterDistanceLimit = 2.5f;
    private void PetalsAngleWithCenterDistance(){
        float correctedUpperCenterDistanceLimit = _upperCenterDistanceLimit / _distance;
        //vertical axis passing by average of left and right shoulder
        Vector3 leftShoulderTracker = _robotPoseController.GetLeftShoulderLocalPosition();
        Vector3 rightShoulderTracker = _robotPoseController.GetRightShoulderLocalPosition();
        Vector3 centerShoulderTracker = (leftShoulderTracker + rightShoulderTracker) / 2;
        //left hand distance with x and z distance from center
        Vector3 leftHandTracker = _robotPoseController.GetLeftWristLocalPosition();
        Vector3 rightHandTracker = _robotPoseController.GetRightWristLocalPosition();
        Vector3 leftHandDistance = new Vector3(leftHandTracker.x - centerShoulderTracker.x, 0, leftHandTracker.z - centerShoulderTracker.z);
        Vector3 rightHandDistance = new Vector3(rightHandTracker.x - centerShoulderTracker.x, 0, rightHandTracker.z - centerShoulderTracker.z);
        //average distance
        _averageDistance = (leftHandDistance.magnitude + rightHandDistance.magnitude) / 2;
        //clamp between 0.7 and 2.5
        _averageDistance = Mathf.Clamp(_averageDistance, _lowerCenterDistanceLimit, correctedUpperCenterDistanceLimit);
        float _averageAngle = (_averageDistance-_lowerCenterDistanceLimit) * 90 / (correctedUpperCenterDistanceLimit - _lowerCenterDistanceLimit);
        //for each DOFController
        foreach (DOFController dofController in _dofControllers)
        {
            //set angle
            dofController.SetAngle(-90f+_averageAngle);
        }

        
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
