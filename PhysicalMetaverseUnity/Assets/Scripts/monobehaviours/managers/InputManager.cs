
using System.Collections.Generic;
using Core;
using UnityEngine;
public class InputManager : Monosingleton<InputManager>
{
    [SerializeField] private Transform head;
    [SerializeField] private List<JoyController> controllers;
    
    // variable to keep track of last sent time
    public float deltaSendTime = 0.01f;
    private float _prevSendTime = 0;
    public bool ENABLE_LOG = false;
    private PoseManager _poseManager;
    
    // head angles variables
    [SerializeField] private string headXAngleKey = "ay";
    [SerializeField] private string headYAngleKey = "az";
    [Range(0, 1)][SerializeField] private float headAnglesTolerance = 0.2f;
    private Vector3 _headAngles;
    private float _prevXAngle;
    private float _prevYAngle;
    [SerializeField] public SetupSO setup;
    private EndPointSO _jetsonEndpoint;
    public string _jetsonIp;
    public Transform _poseRotationTransform;
    public bool _rotate180 = true;
    
    #region Event Functions
    
        void Update()
        {
            //disable Debug.Log for this object
            Debug.unityLogger.logEnabled = ENABLE_LOG;
            // Send the camera's X and Y angles via UDP every 0.05 seconds
            if (Time.time - _prevSendTime > deltaSendTime)
            {
                // NetworkManager.Instance.SendMsg(GetUdpMessage());
                //UDPManager.Instance.SendStringUpdToDefaultEndpoint(GetUdpMessage());
                string udpMessage = RoutineController.Instance.IsRunning 
                    ? RoutineController.Instance.GetMsg() 
                    : GetUdpMessage();
                /*if(udpMessage != "")
                    Debug.Log("udp " + udpMessage);*/
                //if not null
                if (udpMessage != "")
                    //send using NetworkManager
                    NetworkingManager.Instance.SendString(udpMessage, _jetsonIp);

                _prevSendTime = Time.time;
            }
            //rotate pose with speed
            if(!RoutineController.Instance.IsRunning)
                //rotate pose with speed
                RotatePoseWithSpeed();
            Debug.unityLogger.logEnabled = true;
        }
        
        private void FixedUpdate(){
            Odometry();
        }

        private void Odometry(){
            //use Ljx and Ljy to set odometry forward rotateright floats
            float LjxNorm = NormalizeJoy(Ljx);
            float LjyNorm = NormalizeJoy(Ljy);
            //set forward to Ljy, rotate to Ljy
            _odometryManager._forwardFloat = LjyNorm;
            _odometryManager._rotateRightFloat = LjxNorm;
        }

        private float NormalizeJoy(float val){
            val = (val - 127)/127;
            return val;
        }
    
    #endregion
        
        void Start(){
            _poseManager = PoseManager.Instance;
            //rotate this transform 180 if true
            if (_rotate180)
                transform.Rotate(0, 180, 0);
            //get ip string from _jetsonEndpoint = setup.JetsonEndpointUsage.Endpoint;
            _jetsonEndpoint = setup.JetsonEndpointUsage.Endpoint;
            _jetsonIp = _jetsonEndpoint.IP.ToString();
        }

    #region Compose UDP Mess

    public OdometryManager _odometryManager;

    //range 0 255 variable Ljx
    [Range(0, 255)] public int Ljx = 0;
    //range 0 255 variable Ljy
    [Range(0, 255)] public int Ljy = 0;

    //range 0 255 variable Lrx
    [Range(0, 255)] public int Lrx = 0;
    //range 0 255 variable Lry
    [Range(0, 255)] public int Lry = 0;
    

        public string GetUdpMessage()
        {
            var msg = GetHeadAnglesMsg();

            // AddMsg for every controller in the list
            foreach (var controller in controllers)
                msg = AddMsg(msg, controller.GetUdpMessage(_headAngles.y));
            
            //log if not empty
            //if (msg != "")
                //Debug.Log(msg);
            // if msg is not empty, SendMsg
            //parse from msg Ljx, Ljy, Lrx, Lry in format  Lrx:161_Lrz:161
            foreach (string keyVal in msg.Split(Constants.MsgDelimiter))
            {
                //split keyVal in key and val
                string[] keyValSplit = keyVal.Split(Constants.KeyValDelimiter);
                //if key is Ljx
                if (keyValSplit[0] == "Ljx")
                {
                    //int oldLjx = Ljx;
                    //parse val to int
                    int.TryParse(keyValSplit[1], out Ljx);
                    //if(Ljx == 127){
                    //    Ljx = oldLjx;
                    //}
                }
                //if key is Ljy
                if (keyValSplit[0] == "Ljy")
                {
                    //int oldLjy = Ljy;
                    //parse val to int
                    int.TryParse(keyValSplit[1], out Ljy);
                    //if(Ljy == 127){
                    //    Ljy = oldLjy;
                    //}
                }
                //if key is Lrx
                if (keyValSplit[0] == "Lrx")
                {
                    //parse val to int
                    int.TryParse(keyValSplit[1], out Lrx);
                }
                //if key is Lry
                if (keyValSplit[0] == "Lry")
                {
                    //parse val to int
                    int.TryParse(keyValSplit[1], out Lry);
                }
            }   
            return msg;
        }

        private string AddMsg(string prevMsg, string nextMsg)
        {
            // if prevMsg is empty, return nextMsg; if nextMsg is empty, return prevMsg; if neither are empty, return prevMsg_nextMsg
            if (prevMsg == "")
                return nextMsg;
            
            if (nextMsg == "")
                return prevMsg;
            
            return prevMsg + Constants.MsgDelimiter + nextMsg;
        }
        
        public Vector3 _headYTarget = new Vector3();
        public Vector3 _headYCurrent = new Vector3();
        private float _prevTimeYAngle = 0;
        private float _headYDeltaTime = 0.03f;
        [Range (0.01f, 180f)]
        public float _headYServoSpeed = 0.1f;

        private void RotatePoseWithSpeed(){ //TODO NEED TO OVERSHOOT, FRAME IS ALREADY ALIGNED
            // vector3 head.eulerAngles + 180 on y angle
            _headYTarget = head.eulerAngles;
            //log _headYTarget
            //Debug.Log("_headYTarget: " + _headYTarget);
            _headYTarget.y += 180;
            _headYTarget.y -= 360;
            //clamp -89 89
            _headYTarget.y = Mathf.Clamp(_headYTarget.y, -89, 89);
            //_poseRotationTransform.localRotation = Quaternion.Euler(0, _headYTarget.y, 0);
            if (Time.time - _prevTimeYAngle > _headYDeltaTime)
            {
                //if close set to same
                if (Mathf.Abs(_headYCurrent.y - _headYTarget.y) <= _headYServoSpeed)
                    _headYCurrent.y = _headYTarget.y;
                else{
                    if(_headYCurrent.y < _headYTarget.y)
                        _headYCurrent.y += _headYServoSpeed;
                    else if(_headYCurrent.y > _headYTarget.y)
                        _headYCurrent.y -= _headYServoSpeed;
                }
                //clamp between -89 and 89
                _headYCurrent.y = Mathf.Clamp(_headYCurrent.y, -89, 89);
                //rotate _poseRotationTransform by y
                _poseRotationTransform.localRotation = Quaternion.Euler(0, _headYTarget.y, 0);
                _prevTimeYAngle = Time.time;
            }
        }
        private string GetHeadAnglesMsg()
        {
            //rotate _poseRotationTransform by y
            //_poseRotationTransform.localRotation = Quaternion.Euler(0, _headYTarget.y, 0);
            _headAngles = RotationHelper.SubtractAllAngles(
                _headYTarget, 
                Constants.JoystickAngleNormalisation);
            
            _headAngles.x += HeadTiltOffset();
            // rescale from 0-180 to -3-3 and clamp
            var xAngle = Mathf.Clamp(
                MathHelper.MapRange(_headAngles.x, 0, 180, -6, 6),
                -6, 6);
            var yAngle = Mathf.Clamp(
                MathHelper.MapRange(_headAngles.y, 0, 180, -3, 3),
                -3, 3);

            yAngle /= 3;
            
            // if xAngle or yAngle is too close by 'tolerance' to previous value, don't send
            // if xAngle or yAngle are -3 or 3 and their prevAngle value is not exactly -3 or 3, send
            var msg = "";
            if (Mathf.Abs(xAngle - _prevXAngle) > headAnglesTolerance || Mathf.Abs(yAngle - _prevYAngle) > headAnglesTolerance ||
                (Mathf.Abs(xAngle - _prevXAngle) > 0.0001f && (xAngle >= 6 || xAngle <= -6)) ||
                (Mathf.Abs(yAngle - _prevYAngle) > 0.0001f && (yAngle >= 3 || yAngle <= -3)))
            {
                //update prevAngle
                _prevXAngle = xAngle;
                _prevYAngle = yAngle;
                //send udp message
                msg = headYAngleKey + Constants.KeyValDelimiter + yAngle.ToString() + 
                      Constants.MsgDelimiter +
                      headXAngleKey + Constants.KeyValDelimiter + xAngle.ToString();
                //replace commas with dots
                msg = msg.Replace(",", ".");
                //Debug.Log("msg: " + msg);
            }
            return msg;
        }

        private float HeadTiltOffset()
        {
            _headYOffset = 1/_poseManager.GetDistanceFromCamera()*_headAngleAtDistanceOne;
            return _headYOffset;
        }
        public float _headYOffset = 0f;
        public float _headAngleAtDistanceOne = 20f;
    #endregion 
}
