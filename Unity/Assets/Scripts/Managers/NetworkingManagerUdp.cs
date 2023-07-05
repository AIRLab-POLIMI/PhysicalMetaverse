//this script should receive udp messages in unity and log them
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using GameEvents;

//new version of NetworkingManager, just start listening and see what happens
//for now just receive, control of the robot is done via controller external to unity
public class NetworkingManagerUdp : MonoBehaviour
{
    //listen for udp messages on port 25888
    public int _udpPort = 25888;
    //udpclient object
    private UdpClient _client;
    //udp packet storage
    private byte[] _data;
    //udp packet size
    public int _packetSize = 1024;  //CHANGE IF NOT ENOUGH

    [SerializeField] private KeyValueGameEventSO _onKeyValueReceived;

    public void Setup()
    {
        //create udpclient object
        _client = new UdpClient(_udpPort);
        //udp packets are sent as byte data
        _data = new byte[_packetSize];
        //begin listening for messages
        _client.BeginReceive(new AsyncCallback(recv), null);
    }

    //recv
    private void recv(IAsyncResult res)
    {
        //store the remote endpoint
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, _udpPort);
        //get the data
        _data = _client.EndReceive(res, ref RemoteIpEndPoint);
        //get the message
        string message = Encoding.ASCII.GetString(_data);
        //log the message
        //Debug.Log(message);
        if (!CheckKeyValueMessage(_data))
        {
            Debug.Log("Message not recognized");
        Debug.Log(message);
        }
        //listen for new messages
        _client.BeginReceive(new AsyncCallback(recv), null);
    }

    private bool CheckKeyValueMessage(byte[] msg)
    {
        var keyValMsg = KeyValueMsg.ParseKeyValueMsg(msg);

        if (keyValMsg != null)
        {
            _onKeyValueReceived.Invoke(keyValMsg);
            return true;
        }

        return false;
    }

    private void FixedUpdate()
    {
        
    }

    private void OnApplicationQuit()
    {
        //stop listening
        _client.Close();
    }
}

