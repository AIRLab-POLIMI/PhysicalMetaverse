
using System.Collections.Generic;
using Core;
using UnityEngine;
public class InputManager : Monosingleton<InputManager>
{
    [SerializeField] private Transform head;
    [SerializeField] private List<JoyController> controllers;
    
    // variable to keep track of last sent time
    public float deltaSendTime = 0.05f;
    private float _prevSendTime = 0;
    
    // head angles variables
    [SerializeField] private string headXAngleKey = "ay";
    [SerializeField] private string headYAngleKey = "az";
    [Range(0, 1)][SerializeField] private float headAnglesTolerance = 0.2f;
    private Vector3 _headAngles;
    private float _prevXAngle;
    private float _prevYAngle;
    [SerializeField] public SetupSO setup;
    private EndPointSO _jetsonEndpoint;
    private string _jetsonIp;
    
    #region Event Functions
    
        void FixedUpdate()
        {
            // Send the camera's X and Y angles via UDP every 0.05 seconds
            if (Time.time - _prevSendTime > deltaSendTime)
            {
                // NetworkManager.Instance.SendMsg(GetUdpMessage());
                //UDPManager.Instance.SendStringUpdToDefaultEndpoint(GetUdpMessage());
                string udpMessage = GetUdpMessage();
                //if not null
                if (udpMessage != "")
                    //send using NetworkManager
                    NetworkingManager.Instance.SendString(udpMessage, _jetsonIp);

                _prevSendTime = Time.time;
            }
        }
    
    #endregion
        
        void Start(){
            //get ip string from _jetsonEndpoint = setup.JetsonEndpointUsage.Endpoint;
            _jetsonEndpoint = setup.JetsonEndpointUsage.Endpoint;
            _jetsonIp = _jetsonEndpoint.IP.ToString();
        }

    #region Compose UDP Mess

        public string GetUdpMessage()
        {
            var msg = GetHeadAnglesMsg();

            // AddMsg for every controller in the list
            foreach (var controller in controllers)
                msg = AddMsg(msg, controller.GetUdpMessage(_headAngles.y));
            
            //log if not empty
            if (msg != "")
                Debug.Log(msg);
            // if msg is not empty, SendMsg
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
        
        
        private string GetHeadAnglesMsg()
        {
            _headAngles = RotationHelper.SubtractAllAngles(
                head.eulerAngles, 
                Constants.JoystickAngleNormalisation);

            // rescale from 0-180 to -3-3 and clamp
            var xAngle = Mathf.Clamp(
                MathHelper.MapRange(_headAngles.x, 0, 180, -3, 3),
                -3, 3);
            var yAngle = Mathf.Clamp(
                MathHelper.MapRange(_headAngles.y, 0, 180, -3, 3),
                -3, 3);
            
            // if xAngle or yAngle is too close by 'tolerance' to previous value, don't send
            // if xAngle or yAngle are -3 or 3 and their prevAngle value is not exactly -3 or 3, send
            var msg = "";
            if (Mathf.Abs(xAngle - _prevXAngle) > headAnglesTolerance || Mathf.Abs(yAngle - _prevYAngle) > headAnglesTolerance ||
                (Mathf.Abs(xAngle - _prevXAngle) > 0.0001f && (xAngle >= 3 || xAngle <= -3)) ||
                (Mathf.Abs(yAngle - _prevYAngle) > 0.0001f && (yAngle >= 3 || yAngle <= -3)))
            {
                //update prevAngle
                _prevXAngle = xAngle;
                _prevYAngle = yAngle;
                //send udp message
                msg = headYAngleKey + Constants.KeyValDelimiter + yAngle.ToString() + 
                      Constants.MsgDelimiter +
                      headXAngleKey + Constants.KeyValDelimiter + xAngle.ToString();
            }
            return msg;
        }
        
    #endregion 
}
