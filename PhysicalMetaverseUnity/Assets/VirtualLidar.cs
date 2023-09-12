using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Linq;

public class VirtualLidar : MonoBehaviour
{
    public VirtualJetson _jetson;
    public int _dataMultiplier = 100;
    public int _frameSkip = 4;
    public int _maxDistance = 50;
    public byte key = 0xf1;

    [Range(0.01f, 1.0f)]
    public float _noise = 0.1f;
    [Range(0.01f, 1.0f)]
    public float _missChance = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        LidarScan();
    }

    void LidarScan(){
        //array of distances of points from lidar
        List<float> distances = new List<float>();
        //ray cast from the center of the object 360 degrees around, for each degree say if hit
        for (int i = 0; i < 360; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Quaternion.Euler(0, i, 0) * transform.forward, out hit, _maxDistance))
            {
                if (Time.frameCount % _frameSkip == 0)
                {
                    Debug.DrawRay(transform.position, Quaternion.Euler(0, i, 0) * transform.forward * hit.distance, Color.red);
                }
                //random miss chance
                if (UnityEngine.Random.Range(0.0f, 1.0f) < _missChance)
                {
                    distances.Add(99999);
                    continue;
                }
                //Debug.DrawRay(transform.position, Quaternion.Euler(0, i, 0) * transform.forward * hit.distance, Color.red);
                //distances.Add((float)(hit.distance));
                //add gaussian noise
                distances.Add((float)(hit.distance + (UnityEngine.Random.Range(0.0f, 1.0f) * _noise)));
            }
            else
            {
                distances.Add(99999);
                //Debug.DrawRay(transform.position, Quaternion.Euler(0, i, 0) * transform.forward * 10, Color.white);
                //Debug.Log("Did not Hit");
            }
        }
        
        if (Time.frameCount % _frameSkip == 0){
            // Create an array to store the values.
            int[] arr = new int[360];

            // Iterate over the array and append the current value multiplied by 100.
            for (int i = 0; i < 360; i++)
            {
                arr[i] = (int)(distances[i] * _dataMultiplier);
            }

            

            //convert array to bytes and add 0xf1 at the start
            byte[] bytes = arr.SelectMany(BitConverter.GetBytes).ToArray();
            _jetson.Send(bytes, key);
            
        }

    }
}
