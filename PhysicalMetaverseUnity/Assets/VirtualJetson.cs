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
    //public list devices
    public List<GameObject> _devices;
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
    }
}
