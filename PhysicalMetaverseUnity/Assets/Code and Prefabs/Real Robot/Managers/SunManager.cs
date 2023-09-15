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
    private float xAngle = 180-65f;
    private bool firstUpdate = true;
    //range
    [Range(0.01f, 1.0f)]
    public float _lerpParam = 0.1f;

    public void OnMsgRcv(byte[] msg) //Sun receive key 250
    {
        //log received integer from sun
        //Debug.Log("Sun received: " + msg.ToString());
        //for each element of msg decode ascii and build a string
        string received = Encoding.ASCII.GetString(msg);
        //log string
        //Debug.Log("Sun received: " + received);
        //string to int
        int receivedInt = Int32.Parse(received);
        receivedSunAngle = receivedInt;
        if (firstUpdate)
        {
            directionalLight.transform.eulerAngles = new Vector3(xAngle, receivedSunAngle, 0);
            firstUpdate = false;
        }
        
    }

    //gradually rotate y angle of sun to received angle keeping x and z angle the same
    void FixedUpdate()
    {
        //set sun angle to received angle
        directionalLight.transform.eulerAngles = new Vector3(xAngle, -receivedSunAngle, 0);
        //gradually
        //directionalLight.transform.eulerAngles = new Vector3(xAngle, Mathf.Lerp(prevSunAngle, receivedSunAngle, _lerpParam), 0);
    }
}


