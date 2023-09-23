using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OdometryManager : MonoBehaviour
{
    //bool forward
    public bool _forward = false;
    //bool backward
    public bool _backward = false;
    //bool left
    public bool _left = false;
    //bool right
    public bool _right = false;
    //bool rotate left
    public bool _rotateLeft = false;
    //bool rotate right
    public bool _rotateRight = false;
    //slider
    [Range(0.1f, 10f)]
    public float _speed = 1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
