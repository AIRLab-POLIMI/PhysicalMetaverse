
using UnityEngine;
using UnityEngine.InputSystem;


public class JoyController : MonoBehaviour
{
    
    #region Variables

        [SerializeField] private string key;
        
    [Space]
    [Header("Rotation Ranges")]
        
        [Range(-180, 180)] [SerializeField] float minAccX = 0;
        [Range(-180, 180)] [SerializeField] float maxAccX = 180;
        
    [Space]
        
        [Range(-180, 180)] [SerializeField] float minAccY = -90;
        [Range(-180, 180)] [SerializeField] float maxAccY = 90;
        
    [Space]

        [Range(-180, 180)] [SerializeField] float minAccZ = 0;
        [Range(-180, 180)] [SerializeField] float maxAccZ = 180;
            
    [Space]
    [Header("Buttons")]
    
        [SerializeField] public InputActionProperty moveInput;
        [SerializeField] public InputActionProperty indexInput;
        [SerializeField] public InputActionProperty middleInput;

                    
    [Space]
    [Header("Flags")]
        public bool sendJoyX = true;
        public bool sendJoyY = true;
        public bool sendAccX = true;
        public bool sendAccY = true;
        public bool sendAccZ = true;
        public bool sendIndex = true;
        public bool sendMiddle = true;
    
        
        // one SensorValue for each rotation axis  
        private SensorValue _rx;
        private SensorValue _ry;
        private SensorValue _rz;
        
        // ones sensor value for each of the two inputs of the joypad
        private SensorValue _jx;
        private SensorValue _jy;
        
        // one sensor value for each of the two buttons of the joypad
        private SensorValue _btrig;
        private SensorValue _bgrab;
        
    #endregion
    
    
    #region Methods

        void Start()
        {
            // initialize the sensor values
            _rx = new SensorValue("rx", minAccX, maxAccX);
            _ry = new SensorValue("ry", minAccY, maxAccY);
            _rz = new SensorValue("rz", minAccZ, maxAccZ);
            
            _jx = new SensorValue("jx", -1, 1);
            _jy = new SensorValue("jy", -1, 1);
            
            _btrig = new SensorValue("bt", 0, 1);
            _bgrab = new SensorValue("bg", 0, 1);
        }

        
        public string GetUdpMessage(float headYAngle)
        {
            // update all rotation 'sensorValues' based on current rotation, normalised on Y axis by head Y rot
            GetRotationAngles(headYAngle);
            
            // update all joypad 'sensorValues' based on current input from joystick
            GetJoystickInput();
            
            // generate empty message
            var msg = "";

            // try to get the message from each sensor value
            if (sendAccX) msg = AddMsg(_rx, msg);
            if (sendAccY) msg = AddMsg(_ry, msg);
            if (sendAccZ) msg = AddMsg(_rz, msg);
            if (sendJoyX) msg = AddMsg(_jx, msg);
            if (sendJoyY) msg = AddMsg(_jy, msg);
            if (sendIndex) msg = AddMsg(_btrig, msg);
            if (sendMiddle) msg = AddMsg(_bgrab, msg);
            
            // send the current message
            return msg;
        }
        
        private void GetRotationAngles(float headYAngle)
        {
            // get the global angles of the left hand
            var joystickAngles = RotationHelper.SubtractAllAngles(transform.eulerAngles, Constants.JoystickAngleNormalisation);
            // subtract the y angle of the head from the y angle of the left hand using unity method
            // needed when SIMULATING; disabling it on real oculus, otherwise Y rotation will be relative to head rot
            // joystickAngles.y = Mathf.DeltaAngle(joystickAngles.y, headYAngle);
            
            // set the current rotation values in the sensor values
            _rx.OnNewValueReceived(joystickAngles.x);
            _ry.OnNewValueReceived(joystickAngles.y);
            _rz.OnNewValueReceived(joystickAngles.z);
            
            // debug log the key and all the current rotations
            // Debug.Log(key + " - " + joystickAngles);
        }

        private void GetJoystickInput()
        {
            // joypad
            var moveValue = moveInput.action.ReadValue<Vector2>();
            Debug.Log(moveValue);
            _jx.OnNewValueReceived(moveValue.x);
            _jy.OnNewValueReceived(moveValue.y);
            
            // buttons
            var indexValue = indexInput.action.ReadValue<float>();
            _btrig.OnNewValueReceived(indexValue);
            var middleValue = middleInput.action.ReadValue<float>();
            _bgrab.OnNewValueReceived(middleValue);
        }
        
        private string AddMsg(SensorValue sensorValue, string currentMsg)
        {
            string sensorMsg = sensorValue.TryGetMsg();

            if (sensorMsg != "")
            {
                // add "key" to the message at the beginning
                sensorMsg = key + sensorMsg;

                if (currentMsg == "")
                    currentMsg = sensorMsg;
                else
                    currentMsg += (Constants.MsgDelimiter + sensorMsg);
            }
            
            return currentMsg;
        }

        
    #endregion

}
