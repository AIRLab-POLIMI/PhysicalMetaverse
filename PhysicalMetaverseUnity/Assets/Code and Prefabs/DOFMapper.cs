using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DOFMapper : MonoBehaviour
{
    [System.Serializable]
    public class DOFMap
    {
        //list of gameobjects
        public List<GameObject> _source = new List<GameObject>();
        public GameObject _target;
    }

    //list of RobotJointsArmsDict
    public List<DOFMap> _dofMap = new List<DOFMap>();
    public float _linePermanenceTime = 0.1f;

    [ContextMenu("Draw lines")]
    // Start is called before the first frame update
    void DrawLines()
    {
        //for each element of dofmap, for each gameobject in source draw a line from source to target
        foreach (DOFMap dofMap in _dofMap)
        {
            foreach (GameObject source in dofMap._source)
            {
                //color equal color of source
                Color color = source.GetComponent<Renderer>().sharedMaterial.color;
                Debug.DrawLine(source.transform.position, dofMap._target.transform.position, color, 1000f);
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //for each element of dofmap, for each gameobject in source draw a line from source to target
        foreach (DOFMap dofMap in _dofMap)
        {
            foreach (GameObject source in dofMap._source)
            {
                //color equal color of source
                Color color = source.GetComponent<Renderer>().sharedMaterial.color;
                Debug.DrawLine(source.transform.position, dofMap._target.transform.position, color, Time.deltaTime);
            }
        }
    }
}
