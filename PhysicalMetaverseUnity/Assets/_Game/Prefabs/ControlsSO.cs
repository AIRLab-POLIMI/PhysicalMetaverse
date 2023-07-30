///scriptable object containing gameobject list
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "ControlsSO", menuName = "ScriptableObjects/ControlsSO")]
public class ControlsSO : ScriptableObject
{
    [System.Serializable]
    public class RobotJointsArmsDict
    {
        //list of joints
        public String joint;
        public String cwKey;
        public String ccKey;
        public String axis;
        public String direction;
        public bool invert;
    }

    //list of RobotJointsArmsDict
    public List<RobotJointsArmsDict> _robotJointsArmsDict = new List<RobotJointsArmsDict>();
}
