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
    //setup udp client to send to localhost
    UdpClient client = new UdpClient();
    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 25666);

    public int _dataMultiplier = 100;

    // Start is called before the first frame update
    void Start()
    {
        client.Connect(remoteEP);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        LidarScan();
    }

    void LidarScan(){
        //array of distances of points from lidar
        List<int> distances = new List<int>();
        //ray cast from the center of the object 360 degrees around, for each degree say if hit
        for (int i = 0; i < 360; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Quaternion.Euler(0, i, 0) * transform.forward, out hit, 10))
            {
                if (Time.frameCount % 1 == 0)
                {
                    Debug.DrawRay(transform.position, Quaternion.Euler(0, i, 0) * transform.forward * hit.distance, Color.red);
                }
                //Debug.DrawRay(transform.position, Quaternion.Euler(0, i, 0) * transform.forward * hit.distance, Color.red);
                distances.Add((int)(hit.distance));
            }
            else
            {
                distances.Add(99999);
                //Debug.DrawRay(transform.position, Quaternion.Euler(0, i, 0) * transform.forward * 10, Color.white);
                //Debug.Log("Did not Hit");
            }
        }
        
        if (Time.frameCount % 4 == 0){
            // Create an array to store the values.
            int[] arr = new int[360];

            // Iterate over the array and append the current value multiplied by 100.
            for (int i = 0; i < 360; i++)
            {
                arr[i] = distances[i] * _dataMultiplier;
            }

            

            //convert array to bytes and add 0xf1 at the start
            byte[] bytes = arr.SelectMany(BitConverter.GetBytes).ToArray();
            byte[] bytes2 = new byte[bytes.Length + 1];
            bytes2[0] = 0xf1;
            Array.Copy(bytes, 0, bytes2, 1, bytes.Length);
            //send bytes to localhost
            client.Send(bytes2, bytes2.Length);
            
        }

    }
}
