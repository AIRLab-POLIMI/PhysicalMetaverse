using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class RobotController : MonoBehaviour
{
    //list of gameobjects robot joints arms
    //public List<GameObject> robotJointsArms = new List<GameObject>();
    
    //struct containing a string and a gameobject
    [System.Serializable]
    public class RobotJointsArmsDict
    {
        //list of joints
        public GameObject joint;
        public String cwKey;
        public String ccKey;
        public String axis;
    }

    //list of RobotJointsArmsDict
    public List<RobotJointsArmsDict> _robotJointsArmsDict = new List<RobotJointsArmsDict>();
    //list of strings
    public List<KeyCode> _keys = new List<KeyCode>();
    //button to fire updatekeys
    public bool _updateKeysButton = false;
    Gamepad _gamepad;
    
    //public enum containing keyboard and joystick selectable inputs
    public enum InputType
    {
        Keyboard,
        Joystick
    }

    //public InputType
    public InputType _inputType = InputType.Joystick;
    //controlsSO list
    public List<ControlsSO> _controlsList = new List<ControlsSO>();
    private ControlsSO _activeControls = null;
    public InputSettings _inputSettings;
    //set global input to input settings
    void Awake()
    {
        //set global input to input settings
        InputSystem.settings = _inputSettings;
    }
    // Start is called before the first frame update
    void Start()
    {
        _gamepad = Gamepad.current;
        if (_gamepad == null)
        {
            Debug.LogWarning("Logitech F710 not detected or not supported.");
            return;
        }
        if (_inputType == InputType.Keyboard)
        {
            //get active controls
            _activeControls = _controlsList[0];
            UpdateKeysKeyboard();
        }
        else if (_inputType == InputType.Joystick)
        {
            //get active controls
            _activeControls = _controlsList[1];
            UpdateKeysJoystick();
        }


        //insert R key and gameobject in dictionary
        //robotJointsArmsDict.Add("R", GameObject.Find("Cube (4)"));
    }

    void FixedUpdate(){
        if (_inputType == InputType.Keyboard)
        {
            KeyboardUpdate();
        }
        else if (_inputType == InputType.Joystick)
        {
            JoystickUpdate();
        }
        
    }

    void KeyboardUpdate(){
        if (Input.anyKey)
        {
            // Loop through all the possible key codes and check if the key is pressed
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(keyCode))
                {
                    //print(keyCode);
                    //if key pressed is in list of keys, check each occurrence
                    if(_keys.Contains(keyCode))
                    {
                        //get list of indexes of key pressed
                        List<int> indexes = new List<int>();
                        for (int i = 0; i < _keys.Count; i++)
                        {
                            if (_keys[i] == keyCode)
                            {
                                indexes.Add(i);
                            }
                        }
                        //for each index do
                        foreach (int index in indexes)
                        {
                            //if index is even
                            if (index % 2 == 0)
                            {
                                //store parent
                                Transform father = _robotJointsArmsDict[index / 2].joint.transform.parent;
                                //rotate joint clockwise around father
                                _robotJointsArmsDict[index / 2].joint.transform.RotateAround(father.position, father.right, 1f);
                            }
                            //if index is odd
                            else
                            {
                                //rotate joint counterclockwise
                                Transform father = _robotJointsArmsDict[(index - 1) / 2].joint.transform.parent;
                                _robotJointsArmsDict[(index - 1) / 2].joint.transform.RotateAround(father.position, father.right, -1f);
                            }
                        }
                    }
                }
            }
        }
        if (_updateKeysButton)
        {
            UpdateKeysKeyboard();
            _updateKeysButton = false;
        }
    }

    //prevaxisvalue dictionary
    Dictionary<string, string> _prevAxisValue = new Dictionary<string, string>();
    void JoystickUpdate(){
        //print all the buttons that were pressed
        foreach (InputControl control in _gamepad.allControls)
        {
            if (control.IsPressed())
            {
                //log value
                Debug.Log(control.displayName + " " + control.ReadValueAsObject());
            }
            //if control display name is equal to the axis of the robot joint arm
            if (_robotJointsArmsDict.Exists(x => x.axis == control.displayName))
            {
                //get value of x axis from unity global input
                string value = control.ReadValueAsObject().ToString();
                //update prevaxisvalue dictionary
                if (!_prevAxisValue.ContainsKey(control.displayName))
                {
                    _prevAxisValue.Add(control.displayName, "0");
                }
                //if value changed
                if (value != _prevAxisValue[control.displayName])
                {
                    //get index of robot joint arm
                    int index = _robotJointsArmsDict.FindIndex(x => x.axis == control.displayName);
                    //value to float
                    float valueFloat = float.Parse(value);
                    Debug.Log("Axis " + control.displayName + " " + valueFloat);
                    //store parent
                    Transform father = _robotJointsArmsDict[index].joint.transform.parent;
                    Transform child = _robotJointsArmsDict[index].joint.transform;
                    //save distance amount between joint and father
                    //float distance = Vector3.Distance(father.position, child.position);
                    //move joint to father
                    //child.position = father.position;
                    //rotate back by prevvalue
                    child.RotateAround(father.position, father.right, -float.Parse(_prevAxisValue[control.displayName]) * 180);
                    //map value to 0 180 and rotate joint to that precise angle relative to father's right
                    child.RotateAround(father.position, father.right, valueFloat * 180);
                    //get vector pointing as child rotation
                    //Vector3 vector = child.rotation * Vector3.up;
                    //move joint to distance amount from father
                    //child.position += vector * distance;
                    //set prevaxisvalue to value
                    _prevAxisValue[control.displayName] = value;
                }

            }
        }
        if (_updateKeysButton)
        {
            UpdateKeysJoystick();
            _updateKeysButton = false;
        }
    }
    void UpdateKeysKeyboard(){
        //clear list
        _keys.Clear();
        //clear _robotJointsArmsDict
        _robotJointsArmsDict.Clear();
        //for each element in active controls fill _robotJointsArmsDict using gameobject find
        foreach (ControlsSO.RobotJointsArmsDict robotJointsArm in _activeControls._robotJointsArmsDict)
        {
            //add key to list parsing string, case insensitive
            _robotJointsArmsDict.Add(new RobotJointsArmsDict{joint = GameObject.Find(robotJointsArm.joint), cwKey = robotJointsArm.cwKey, ccKey = robotJointsArm.ccKey});
        }
        //populate list of keys using RobotJointsArmsDict
        foreach (RobotJointsArmsDict robotJointsArm in _robotJointsArmsDict)
        {
            //add key to list parsing string, case insensitive
            _keys.Add((KeyCode)System.Enum.Parse(typeof(KeyCode), robotJointsArm.cwKey, true));
            _keys.Add((KeyCode)System.Enum.Parse(typeof(KeyCode), robotJointsArm.ccKey, true));
        }
        //log keys updated
        Debug.Log("Keys updated");
    }

    void UpdateKeysJoystick(){
        //clear list
        _keys.Clear();
        //clear _robotJointsArmsDict
        _robotJointsArmsDict.Clear();
        //for each element in active controls fill _robotJointsArmsDict using gameobject find
        foreach (ControlsSO.RobotJointsArmsDict robotJointsArm in _activeControls._robotJointsArmsDict)
        {
            //add key to list parsing string, case insensitive
            _robotJointsArmsDict.Add(new RobotJointsArmsDict{joint = GameObject.Find(robotJointsArm.joint), axis = robotJointsArm.axis});
        }
        //populate list of keys using RobotJointsArmsDict
        foreach (RobotJointsArmsDict robotJointsArm in _robotJointsArmsDict)
        {
            //add key to list parsing string, case insensitive, if not null
            if (robotJointsArm.cwKey != null)
            {
                _keys.Add((KeyCode)System.Enum.Parse(typeof(KeyCode), robotJointsArm.cwKey, true));
            }
            if (robotJointsArm.ccKey != null)
            {
                _keys.Add((KeyCode)System.Enum.Parse(typeof(KeyCode), robotJointsArm.ccKey, true));
            }
        }
        //log keys updated
        Debug.Log("Keys updated");
    }
}
