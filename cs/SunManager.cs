using System;
using System.Linq;
using Core;
using UnityEngine;
using System.Text;
using System.Collections;

public class SunManager : Monosingleton<SunManager>
{
    //[SerializeField] private FlashImage _flashImage = null;

    //[SerializeField] private IntSO numOfBump;
    
    //public void Setup()
    //{
    //    Debug.Log("[Bump Manager setup]");
    //    numOfBump.runtimeValue = 0;
    //}
    
    //directional light gameobject serialize private
    [SerializeField] private GameObject directionalLight;
    private float prevSunAngle = 0f;
    private float receivedSunAngle = 0f;
    private bool firstUpdate = true;
    public bool _received = false;
    //range
    [Range(0.01f, 1.0f)]
    public float _lerpParam = 0.1f;
    public float _sunSpeed = 1f;

    public void OnMsgRcv(byte[] msg) //Sun receive key 250
    {
        //log received integer from sun
        //Debug.Log("Sun received: " + msg.ToString());
        //for each element of msg decode ascii and build a string
        string received = Encoding.ASCII.GetString(msg);
        _received = true;
        //log string
        //Debug.Log("Sun received: " + received);
        //string to int
        int receivedInt = Int32.Parse(received);
        receivedSunAngle = receivedInt;
        if (firstUpdate)
        {
            directionalLight.transform.eulerAngles = new Vector3(directionalLight.transform.eulerAngles.x, -receivedSunAngle, directionalLight.transform.eulerAngles.z);
            firstUpdate = false;
        }
        
    }

    //gradually rotate y angle of sun to received angle keeping x and z angle the same
    void Update()
    {
        if(_received){
            //set sun angle to received angle
            directionalLight.transform.eulerAngles = new Vector3(directionalLight.transform.eulerAngles.x, -receivedSunAngle, directionalLight.transform.eulerAngles.z);
            _received = false;
        }
        //else rotate with odometry
        else{
            //get odometry rotation
            bool odometryRotationRight = OdometryManager.Instance._rotateRight;
            bool odometryRotationLeft = OdometryManager.Instance._rotateLeft;
            //if odometry rotation is not 0
            if(odometryRotationRight){
                //rotate sun
                directionalLight.transform.RotateAround(this.transform.position, Vector3.up, -_sunSpeed * Time.deltaTime);
            }
            if(odometryRotationLeft){
                //rotate sun
                directionalLight.transform.RotateAround(this.transform.position, Vector3.up, _sunSpeed * Time.deltaTime);
            }
        }
        //gradually
        //directionalLight.transform.eulerAngles = new Vector3(xAngle, Mathf.Lerp(prevSunAngle, receivedSunAngle, _lerpParam), 0);
    }
}


