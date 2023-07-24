using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using UnityEngine.UI;

public class SimpleUdpReceiver : MonoBehaviour
{
    public int port = 1234;
    private UdpClient udpClient;
    public Image _image;

    void Start()
    {
        udpClient = new UdpClient(port);
        udpClient.BeginReceive(ReceiveCallback, null);
        //set timeout
        udpClient.Client.ReceiveTimeout = 50;
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
        byte[] receivedBytes = udpClient.EndReceive(ar, ref endPoint);
        string receivedMessage = Encoding.ASCII.GetString(receivedBytes);
        
        Debug.Log("Received: " + receivedMessage);
        //setupscreen getcomponent in children change to random color
        _image.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));

        // Process the received message or update game state accordingly

        // Start listening for the next message
        udpClient.BeginReceive(ReceiveCallback, null);
    }

    void OnApplicationQuit()
    {
        udpClient.Close();
    }
}