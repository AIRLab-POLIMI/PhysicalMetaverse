//receive udp messages using UdpMessenger
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;


public class SimpleUdpReceiver : MonoBehaviour
{
    // Start is called before the first frame update
    private UdpClient _udpClient;
    private byte[] _data;
    [SerializeField] private GameObject _setupScreen;
    void Start()
    {
        //start udp client on port 24000
        _udpClient = new UdpClient(24000);
        //udp packets are sent as byte data
        _data = new byte[1024];
        //print address where it is receiving
        Debug.Log("Receiving on " + _udpClient.Client.LocalEndPoint.ToString());
        //begin listening for messages
        _udpClient.BeginReceive(new AsyncCallback(recv), null);
    }

    private void recv(IAsyncResult res)
    {
        //store the remote endpoint
        //IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 24000);
        //receive on 192.168.0.100
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.100"), 24000);
        //print address where it is receiving
        Debug.Log("Receiving on " + _udpClient.Client.LocalEndPoint.ToString());
        //change color of setup screen to random color
        _setupScreen.GetComponentInChildren<UnityEngine.UI.Image>().color = UnityEngine.Random.ColorHSV();
        //get the data
        _data = _udpClient.EndReceive(res, ref RemoteIpEndPoint);
        //get the message
        string message = Encoding.ASCII.GetString(_data);
        //log the message
        Debug.Log(message);
        //parse the message
        //Debug.Log(ParseData(message));
        //listen for new messages
        _udpClient.BeginReceive(new AsyncCallback(recv), null);
    }
}
