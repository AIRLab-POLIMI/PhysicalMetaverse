using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseRenderer : MonoBehaviour
{
    //list of lists of transforms
    [System.Serializable]
    public class PoseComponents
    {
        //list of gameobjects
        public List<Transform> _part = new List<Transform>();
    }

    public List<PoseComponents> _poseComponents = new List<PoseComponents>();
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //enable debug lines
        Debug.unityLogger.logEnabled = true;
        //for each list in pose, for each element in list draw a line from element to next element with white color
        foreach (PoseComponents poseComponents in _poseComponents)
        {
            for (int i = 0; i < poseComponents._part.Count - 1; i++)
            {
                Color color = Color.white;
                Debug.DrawLine(poseComponents._part[i].position, poseComponents._part[i + 1].position, color, Time.deltaTime);
            }
        }
        Debug.unityLogger.logEnabled = false;
    }
}
