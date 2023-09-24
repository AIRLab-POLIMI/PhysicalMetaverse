//this script should receive udp messages in unity and log them
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

public class VirtualJetson : MonoBehaviour
{
    //setup udp client to send to localhost
    UdpClient client = new UdpClient();
    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 25666);
    IPEndPoint slamEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 25668);
    //public list devices
    public List<GameObject> _devices;
    public bool _STREAMINGLIDAR = false;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Send(byte[] data, byte key){
        byte[] bytes2 = new byte[data.Length + 1];
        bytes2[0] = key;
        Array.Copy(data, 0, bytes2, 1, data.Length);
        client.Send(bytes2, bytes2.Length, remoteEP);
        //if STREAMINGLIDAR, send slam data
        if (_STREAMINGLIDAR)
        {
            if (key == 0xf1)
            {
                //send slam data, remove key
                byte[] slamBytes = new byte[data.Length];
                Array.Copy(data, 0, slamBytes, 0, data.Length);
                //array of ints
                int[] slamData = new int[slamBytes.Length / 4];
                //convert bytes to ints
                for (int i = 0; i < slamData.Length; i++)
                {
                    slamData[i] = BitConverter.ToInt32(slamBytes, i * 4);
                }
                //shape the array like lidar_data = [(45, 5.2),(46,5.2),(47,5.2),(48,5.2),(49,5.2)] where first element is angle, second is distance
                //list of 2 element array, first element is place in array, second is distance
                string dataString = "[(";
                for (int i = 0; i < slamData.Length; i++)
                {
                    //if not 0
                    if (slamData[i] != 0)
                    {
                        //add to string
                        dataString += i.ToString() + "," + slamData[i].ToString() + "),(";
                    }
                }
                dataString = dataString.Substring(0, dataString.Length - 2);
                dataString += "]";
                //log data string
                Debug.Log(dataString);
                client.Send(Encoding.ASCII.GetBytes(dataString), Encoding.ASCII.GetBytes(dataString).Length, slamEP);
            }
        }
        
    }
}
