using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RobotController : MonoBehaviour
{
    //list of gameobjects robot joints arms
    //public List<GameObject> robotJointsArms = new List<GameObject>();
    
    //struct containing a string and a gameobject
    [System.Serializable]
    public class RobotJointsArmsDict
    {
        public String cwKey;
        public String ccKey;
        //list of joints
        public GameObject joint;
    }

    //list of RobotJointsArmsDict
    public List<RobotJointsArmsDict> _robotJointsArmsDict = new List<RobotJointsArmsDict>();
    //list of strings
    public List<KeyCode> _keys = new List<KeyCode>();
    //button to fire updatekeys
    public bool updateKeysButton = false;

    // Start is called before the first frame update
    void Start()
    {
        UpdateKeys();
        
        //insert R key and gameobject in dictionary
        //robotJointsArmsDict.Add("R", GameObject.Find("Cube (4)"));
    }

    void FixedUpdate(){
        //print input keycode

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
        if (updateKeysButton)
        {
            UpdateKeys();
            updateKeysButton = false;
        }
    }

    //todo updatekeys for different input systems
    void UpdateKeys(){
        //clear list
        _keys.Clear();
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
/*  
    void OldFixedUpdate()
    {
        //if q is pressed rotate second gameobject around first one on x axis, relative to first gameobject
        if (Input.GetKey(KeyCode.Q))
        {
            robotJointsArms[1].transform.RotateAround(robotJointsArms[0].transform.position, robotJointsArms[0].transform.right, 1f);
        }
        //if w is pressed rotate second gameobject around first one on x axis, relative to first gameobject
        if (Input.GetKey(KeyCode.W))
        {
            robotJointsArms[1].transform.RotateAround(robotJointsArms[0].transform.position, robotJointsArms[0].transform.right, -1f);
        }
        //if e is pressed rotate fourth gameobject around third one on x axis, relative to second gameobject
        if (Input.GetKey(KeyCode.E))
        {
            robotJointsArms[3].transform.RotateAround(robotJointsArms[2].transform.position, robotJointsArms[2].transform.right, 1f);
        }
        //if r is pressed rotate fourth gameobject around third one on x axis, relative to second gameobject
        if (Input.GetKey(KeyCode.R))
        {
            robotJointsArms[3].transform.RotateAround(robotJointsArms[2].transform.position, robotJointsArms[2].transform.right, -1f);
        }
        //if a is pressed rotate first gameobject around his own y axis
        if (Input.GetKey(KeyCode.A))
        {
            robotJointsArms[0].transform.RotateAround(robotJointsArms[0].transform.position, robotJointsArms[0].transform.up, 1f);
        }
        //if s is pressed rotate first gameobject around his own y axis
        if (Input.GetKey(KeyCode.S))
        {
            robotJointsArms[0].transform.RotateAround(robotJointsArms[0].transform.position, robotJointsArms[0].transform.up, -1f);
        }
    }
*/
}
