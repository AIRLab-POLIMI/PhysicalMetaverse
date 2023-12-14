using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualSun : MonoBehaviour
{
    public VirtualJetson _jetson;
    public GameObject _robotBase;
    public int _frameSkip = 4;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Time.frameCount % _frameSkip == 0){
            //sun angle string (int)(_robotBase.transform.rotation.eulerAngles.y)
            string sunAngle = (int)(_robotBase.transform.rotation.eulerAngles.y) + "";
            //convert string to byte array
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(sunAngle);
            //send rotation of robotbase to jetson
            _jetson.Send(bytes, 0xfa);
            //log
            //Debug.Log("SUN: " + sunAngle);
        }
    }
}
