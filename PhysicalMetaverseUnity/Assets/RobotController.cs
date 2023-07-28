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
        public bool invert;
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

    public enum JoystickMode
    {
        Hold,
        Move
    }

    //public InputType
    public InputType _inputType = InputType.Joystick;
    public JoystickMode _joystickMode = JoystickMode.Hold;
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
            if(_joystickMode == JoystickMode.Hold)
                JoystickUpdateHold();
            else if (_joystickMode == JoystickMode.Move)
                JoystickUpdateMove();
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
    //TODO FIX JOINTS THAT ARE REPEATED ON MORE AXES
    Dictionary<string, float> _prevJointValue = new Dictionary<string, float>();
    Dictionary<string, string> _prevAxisValue = new Dictionary<string, string>();
    public float  _moveUpdate = 100f;
    public float  _angleUpdate = 10f;
    void JoystickUpdateHold(){
        //print all the buttons that were pressed
        foreach (InputControl control in _gamepad.allControls)
        {
            //print control.displayName if value > 0
            /*if (control.ReadValueAsObject().ToString() != "0")
            {
                Debug.Log(control.displayName + " " + control.ReadValueAsObject().ToString());
            }*/
            /*
            Right Bumper
            D-Pad Y
            X
            Y
            ...
            */
            //find all occurrencies
            List<RobotJointsArmsDict> occurrencies = _robotJointsArmsDict.FindAll(x => x.axis == control.displayName);
            /*
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
                    //if invert is true
                    int invert = 1;
                    if (_robotJointsArmsDict[index].invert)
                    {
                        //invert value
                        invert = -1;
                    }
                    //rotate back by prevvalue
                    child.RotateAround(father.position, father.right, -invert * float.Parse(_prevAxisValue[control.displayName]) * 90);
                    //map value to 0 180 and rotate joint to that precise angle relative to father's right
                    child.RotateAround(father.position, father.right, invert * valueFloat * 90);
                    //get vector pointing as child rotation
                    //Vector3 vector = child.rotation * Vector3.up;
                    //move joint to distance amount from father
                    //child.position += vector * distance;
                    //set prevaxisvalue to value
                    _prevAxisValue[control.displayName] = value;
                }
            }*/
            if(occurrencies.Count > 0){
                //log occurrencies if count > 1
                /*if (occurrencies.Count > 1)
                {
                    Debug.Log("More than one occurrency of " + control.displayName);
                    foreach (RobotJointsArmsDict occurrency in occurrencies)
                    {
                        Debug.Log(occurrency.joint.name);
                    }
                }*/
                //do the same but for all occurrencies
                foreach (RobotJointsArmsDict occurrency in occurrencies)
                {
                    //if occurrency name is Odile controls move it
                    if(occurrency.joint.name == "Odile"){
                        //if dpad y
                        if (control.displayName == "D-Pad Y")
                        {
                            //move odile up and forward to transform forward
                            occurrency.joint.transform.position += float.Parse(control.ReadValueAsObject().ToString()) * occurrency.joint.transform.forward / _moveUpdate;
                        }
                        if (control.displayName == "D-Pad X")
                        {
                            //move odile up and forward to transform forward
                            occurrency.joint.transform.position += float.Parse(control.ReadValueAsObject().ToString()) * occurrency.joint.transform.right / _moveUpdate;
                        }
                        if (control.displayName == "Right Bumper")
                        {
                            //rotate odile right, sum angle to euler rotation
                            occurrency.joint.transform.eulerAngles += new Vector3(0, float.Parse(control.ReadValueAsObject().ToString())/_angleUpdate, 0);
                        }
                        if (control.displayName == "Left Bumper")
                        {
                            //rotate odile right, sum angle to euler rotation
                            occurrency.joint.transform.eulerAngles += new Vector3(0, -float.Parse(control.ReadValueAsObject().ToString())/_angleUpdate, 0);
                        }
                    }
                    else{
                        //update prevaxisvalue dictionary
                        if (!_prevJointValue.ContainsKey(occurrency.joint.name))
                        {
                            _prevJointValue.Add(occurrency.joint.name, 0);
                        }
                        string value = control.ReadValueAsObject().ToString();
                        //if control is left trigger print value
                        if (control.displayName == "Left Trigger")
                        {
                            Debug.Log("Left Trigger " + value);
                        }

                        //value to float
                        float valueFloat = float.Parse(value);
                        if (control.displayName == "Left Trigger")
                        {
                            Debug.Log("Left Trigger float " + valueFloat);
                            Debug.Log("Prev Left Trigger float " + _prevJointValue[occurrency.joint.name]);
                        }
                        //Debug.Log("Axis " + control.displayName + " " + valueFloat);
                        //store parent
                        Transform father = occurrency.joint.transform.parent;
                        Transform child = occurrency.joint.transform;
                        //save distance amount between joint and father
                        //float distance = Vector3.Distance(father.position, child.position);
                        //move joint to father
                        //child.position = father.position;
                        //if invert is true
                        int invert = 1;
                        if (occurrency.invert)
                        {
                            //invert value
                            invert = -1;
                        }
                        //rotate back by prevvalue
                        ////child.RotateAround(father.position, father.right, -invert * float.Parse(_prevAxisValue[occurrency.joint.name]) * 90);
                        //map value to 0 180 and rotate joint to that precise angle relative to father's right
                        ////child.RotateAround(father.position, father.right, invert * valueFloat * 90);
                        child.RotateAround(father.position, father.right, invert * (valueFloat - _prevJointValue[occurrency.joint.name]) * 90);
                        //get vector pointing as child rotation
                        //Vector3 vector = child.rotation * Vector3.up;
                        //move joint to distance amount from father
                        //child.position += vector * distance;
                        //set prevaxisvalue to value
                        _prevJointValue[occurrency.joint.name] = valueFloat;
                    }
                }
            }
        }
        if (_updateKeysButton)
        {
            UpdateKeysJoystick();
            _updateKeysButton = false;
        }
    }

    void JoystickUpdateMove(){
        //print all the buttons that were pressed
        foreach (InputControl control in _gamepad.allControls)
        {
            List<RobotJointsArmsDict> occurrencies = _robotJointsArmsDict.FindAll(x => x.axis == control.displayName);

            if(occurrencies.Count > 0){
                //do the same but for all occurrencies
                foreach (RobotJointsArmsDict occurrency in occurrencies)
                {
                    //if occurrency name is Odile controls move it
                    if(occurrency.joint.name == "Odile"){
                        //if dpad y
                        if (control.displayName == "D-Pad Y")
                        {
                            //move odile up and forward to transform forward
                            occurrency.joint.transform.position += float.Parse(control.ReadValueAsObject().ToString()) * occurrency.joint.transform.forward / _moveUpdate;
                        }
                        if (control.displayName == "D-Pad X")
                        {
                            //move odile up and forward to transform forward
                            occurrency.joint.transform.position += float.Parse(control.ReadValueAsObject().ToString()) * occurrency.joint.transform.right / _moveUpdate;
                        }
                        if (control.displayName == "Right Bumper")
                        {
                            //rotate odile right, sum angle to euler rotation
                            occurrency.joint.transform.eulerAngles += new Vector3(0, float.Parse(control.ReadValueAsObject().ToString())/_angleUpdate, 0);
                        }
                        if (control.displayName == "Left Bumper")
                        {
                            //rotate odile right, sum angle to euler rotation
                            occurrency.joint.transform.eulerAngles += new Vector3(0, -float.Parse(control.ReadValueAsObject().ToString())/_angleUpdate, 0);
                        }
                    }
                    else{
                        //update prevaxisvalue dictionary
                        if (!_prevJointValue.ContainsKey(occurrency.joint.name))
                        {
                            _prevJointValue.Add(occurrency.joint.name, 0);
                        }
                        string value = control.ReadValueAsObject().ToString();

                        //value to float
                        float valueFloat = float.Parse(value);
                        //Debug.Log("Axis " + control.displayName + " " + valueFloat);
                        //store parent
                        Transform father = occurrency.joint.transform.parent;
                        Transform child = occurrency.joint.transform;
                        //save distance amount between joint and father
                        //float distance = Vector3.Distance(father.position, child.position);
                        //move joint to father
                        //child.position = father.position;
                        //if invert is true
                        int invert = 1;
                        if (occurrency.invert)
                        {
                            //invert value
                            invert = -1;
                        }
                        //rotate back by prevvalue
                        //child.RotateAround(father.position, father.right, -invert * float.Parse(_prevAxisValue[occurrency.joint.name]) * 90);
                        //map value to 0 180 and rotate joint to that precise angle relative to father's right
                        child.RotateAround(father.position, father.right, invert * valueFloat);
                        //get vector pointing as child rotation
                        //Vector3 vector = child.rotation * Vector3.up;
                        //move joint to distance amount from father
                        //child.position += vector * distance;
                        //set prevaxisvalue to value
                        _prevJointValue[occurrency.joint.name] = valueFloat;
                    }
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
            //invert
            _robotJointsArmsDict[_robotJointsArmsDict.Count - 1].invert = robotJointsArm.invert;
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
    }
    private string axisInput = ""; // Variable to store the user input
    private string jointInput = ""; // Variable to store the user input
    private int numButtons = 20; // Number of buttons
    private float buttonHeight = 30f; // Height of each button
    private float spacing = 10f; // Spacing between buttons
    private Vector2 scrollPosition = Vector2.zero; // Scroll position
    public List<int> selectedOption = new List<int>(); // List of selected options
    public List<int> prevSelectedOption = new List<int>(); // List of selected options
    public List<bool> showOptions = new List<bool>(); // List of booleans to show/hide the options
    private bool listSetup = false; // Boolean to check if the list has been setup
    public string[] options = { "Right Stick X", "Right Stick Y", "Left Stick X", "Left Stick Y", "Right Trigger", "Left Trigger", "D-Pad X", "D-Pad Y", "Right Bumper", "Left Bumper", "A", "B", "X", "Y", "Start", "Select", "Left Stick Button", "Right Stick Button", "Left Stick Button", "Right Stick Button", "None" };
    private void OnGUI()
    {
        GUIStyle customStyle = new GUIStyle(GUI.skin.box);
        // If the list has not been setup, set it up
        if (!listSetup)
        {
            //add one bool for each element in _robotJointsArmsDict
            for (int i = 0; i < _robotJointsArmsDict.Count; i++)
            {
                showOptions.Add(false);
                selectedOption.Add(0);
                prevSelectedOption.Add(0);
                //for each selected option set index to name of corresponding robot joint, if not present set to last
                selectedOption[i] = Array.IndexOf(options, _robotJointsArmsDict[i].axis);
                if (selectedOption[i] == -1)
                {
                    selectedOption[i] = options.Length - 1;
                }
                prevSelectedOption[i] = selectedOption[i];
            }
            listSetup = true;
        }
        float scrollViewHeight = (numButtons * buttonHeight) + ((numButtons - 1) * spacing);
        float scrollViewWidth = 500f;

        // Begin the scroll view
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, customStyle, GUILayout.Width(scrollViewWidth), GUILayout.Height(600));
        //set background
        GUI.backgroundColor = Color.grey;
        
        GUILayout.Label("Robot controls editor");
        GUILayout.Label("Active controls: " + _activeControls.name);
        if (GUILayout.Button("Mode " + _joystickMode, customStyle)){
            if (_joystickMode == JoystickMode.Hold)
                _joystickMode = JoystickMode.Move;
            else if (_joystickMode == JoystickMode.Move)
                _joystickMode = JoystickMode.Hold;
        }
        GUILayout.Label("Input type: " + _inputType);

        // Text input field to allow the user to change the value
        GUILayout.Label("Enter new axis value:");
        axisInput = GUILayout.TextField(axisInput, 25); // '25' is the maximum character limit (optional)
        //GUILayout.Label("Enter new Joint value:");
        //jointInput = GUILayout.TextField(jointInput, 25); // '25' is the maximum character limit (optional)
        // Only show the options if the showOptions variable is true
        
        GUILayout.Label("Keys: ");

        // Loop through the dictionary elements
        try{
            foreach (RobotJointsArmsDict robotJointsArm in _robotJointsArmsDict)
            {
                int robotJointsArmsIndex = _robotJointsArmsDict.IndexOf(robotJointsArm);
                GUILayout.BeginHorizontal();

                // Display the joint name and axis
                GUILayout.Label(robotJointsArm.joint.name + " " + robotJointsArm.axis);

                // Add a button to modify the value for this dictionary entry
                /*if (GUILayout.Button("Change"))
                {
                    // Example: If you want to update the axis value of this entry
                    robotJointsArm.axis = axisInput;
                    //robotJointsArm.joint = GameObject.Find(jointInput);
                }*/
                if (GUILayout.Button(options[selectedOption[robotJointsArmsIndex]]))
                {
                    // Toggle the visibility of the options when the main button is clicked
                    showOptions[robotJointsArmsIndex] = !showOptions[robotJointsArmsIndex];
                    //set all other actives to false
                    for (int i = 0; i < showOptions.Count; i++)
                    {
                        if (i != robotJointsArmsIndex)
                        {
                            showOptions[i] = false;
                        }
                    }
                }
                
                if (showOptions[robotJointsArmsIndex] == true)
                {
                    // Display the options as a toggle group
                    selectedOption[robotJointsArmsIndex] = GUILayout.SelectionGrid(selectedOption[robotJointsArmsIndex], options, 1);
                    robotJointsArm.axis = options[selectedOption[robotJointsArmsIndex]];
                    //if an option is clicked close the options
                    if (selectedOption[robotJointsArmsIndex] != prevSelectedOption[robotJointsArmsIndex])
                    {
                        showOptions[robotJointsArmsIndex] = false;
                        prevSelectedOption[robotJointsArmsIndex] = selectedOption[robotJointsArmsIndex];
                    }
                }
                // button to toggle invert
                if (GUILayout.Button("Invert " + (robotJointsArm.invert? "True" : "False")))
                {
                    // Example: If you want to update the axis value of this entry
                    robotJointsArm.invert = !robotJointsArm.invert;
                }
                if (GUILayout.Button("Copy"))
                {
                    // Example: Add a new entry to the dictionary with default values
                    RobotJointsArmsDict newEntry = new RobotJointsArmsDict();
                    newEntry.joint = robotJointsArm.joint;
                    newEntry.axis = axisInput;
                    /*
                    _robotJointsArmsDict.Add(newEntry);
                    showOptions.Add(false);
                    selectedOption.Add(0);
                    prevSelectedOption.Add(0);
                    */
                    //add everything at this position
                    _robotJointsArmsDict.Insert(robotJointsArmsIndex, newEntry);
                    showOptions.Insert(robotJointsArmsIndex, false);
                    selectedOption.Insert(robotJointsArmsIndex, 0);
                    prevSelectedOption.Insert(robotJointsArmsIndex, 0);
                    //update all other indexes
                    for (int i = robotJointsArmsIndex + 1; i < showOptions.Count; i++)
                    {
                        selectedOption[i] = Array.IndexOf(options, _robotJointsArmsDict[i].axis);
                        if (selectedOption[i] == -1)
                        {
                            selectedOption[i] = options.Length - 1;
                        }
                        prevSelectedOption[i] = selectedOption[i];
                    }
                }
                if (GUILayout.Button("Remove"))
                {
                    //if there are no other entries with this name do not remove it
                    if (_robotJointsArmsDict.FindAll(x => x.joint == robotJointsArm.joint).Count > 1)
                    {
                        // Example: Add a new entry to the dictionary with default values
                        _robotJointsArmsDict.Remove(robotJointsArm);
                        showOptions.RemoveAt(robotJointsArmsIndex);
                        selectedOption.RemoveAt(robotJointsArmsIndex);
                        prevSelectedOption.RemoveAt(robotJointsArmsIndex);
                        //update all other indexes
                        for (int i = robotJointsArmsIndex; i < showOptions.Count; i++)
                        {
                            selectedOption[i] = Array.IndexOf(options, _robotJointsArmsDict[i].axis);
                            if (selectedOption[i] == -1)
                            {
                                selectedOption[i] = options.Length - 1;
                            }
                            prevSelectedOption[i] = selectedOption[i];
                        }
                    }
                }

                GUILayout.EndHorizontal();
            }
        }
        //catch foreach exception
        catch(InvalidOperationException e){
            Debug.Log(e);
        }
        /*
        // Add button to add new dictionary entry
        if (GUILayout.Button("Add New Entry"))
        {
            // Example: Add a new entry to the dictionary with default values
            RobotJointsArmsDict newEntry = new RobotJointsArmsDict();
            newEntry.axis = userInput;
            _robotJointsArmsDict.Add(newEntry);
        }*/
        GUILayout.EndScrollView();
    }
}
