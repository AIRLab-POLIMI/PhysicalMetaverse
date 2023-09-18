using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class VirtualCamera : MonoBehaviour
{
    //camera _XRCamera
    public Camera _XRCamera;
    //camera _VirtualCamera
    public GameObject _virtualCamera;
    public GameObject _robotBase;

    // Start is called before the first frame update
    void Start()
    {
        //disable TrackedPoseDriver
        _XRCamera.GetComponent<TrackedPoseDriver>().enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //set XRCamera local rotation to VirtualCamera rotation minus robotBase rotation euler angles
        _XRCamera.transform.localRotation = Quaternion.Euler(_virtualCamera.transform.rotation.eulerAngles - _robotBase.transform.rotation.eulerAngles);
        
    }
}
