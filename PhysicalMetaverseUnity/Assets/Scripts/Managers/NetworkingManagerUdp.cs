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
    //robot ip public variable init null
    public string _robotIp = null;

    private bool _confirmed = false;

    [SerializeField] private KeyValueGameEventSO _onKeyValueReceived;

    public void Setup()
    {
        //log start
        Debug.Log("Starting udp listener on port " + _udpPort);
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
        Debug.Log(message);
        if (_robotIp == null)
        {
            //the robot is sending in udp broadcast message " "Metaverse is on " + socket.gethostbyname(socket.gethostname()) ", find it and set the robot ip"
            if (message.Contains("Metaverse is on"))
            {
                //split the message
                string[] splitMessage = message.Split(' ');
                //get the ip
                _robotIp = splitMessage[splitMessage.Length - 1];
                Debug.Log("Robot ip is " + _robotIp);
                //reply with "Client on " + socket.gethostbyname(socket.gethostname())
                _client.Send(Encoding.ASCII.GetBytes("Client on " + IPAddress.Any.ToString()), Encoding.ASCII.GetBytes("Client on " + IPAddress.Any.ToString()).Length, _robotIp, _udpPort);
                Debug.Log("Sending my ip as: \"Client on " + IPAddress.Any.ToString() + "\"");
            }
            else
            {
                Debug.Log("Waiting for robot ip on broadcast");
            }
        }
        else if(!_confirmed)
        {
            //the robot is now sending in udp my ip, check if it is correct, message is "Client on " + socket.gethostbyname(socket.gethostname())
            if (message.Contains("Client on"))
            {
                //split the message
                string[] splitMessage = message.Split(' ');
                //get the ip
                string clientIp = splitMessage[splitMessage.Length - 1];
                //check if it is correct
                if (clientIp == IPAddress.Any.ToString())
                {
                    Debug.Log("Robot found me on " + clientIp + ", connection confirmed");
                    _confirmed = true;
                }
                else
                {
                    Debug.Log("Client ip is not correct");
                }
            }
        } 
        else
        {
            //log the message
            //Debug.Log(message);
            if (!CheckKeyValueMessage(_data))
            {
                Debug.Log("Message not recognized");
            }
            //listen for new messages
        }
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

