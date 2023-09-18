using UnityEngine;
using System.Collections;
 
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.UI;
//using math
using System.Collections.Generic;
//usingg textmeshpro
using TMPro;

public class SimpleUdpReceiverV3 : MonoBehaviour {
   
    // receiving Thread
    Thread receiveThread;
 
    // udpclient object
    private UdpClient _client;
    public Image _image;
    // public
    // public string IP = "127.0.0.1"; default local
    public int port = 5020; // define > init
    public int _sendPort = 5021;
 
    // infos
    public string lastReceivedUDPPacket="";
    public string lastSentUDPPacket="";
    public string allReceivedUDPPackets=""; // clean up this from time to time!
    
    public string _state = "waiting";
    private Color _color;
    public string _robotIp;
    public string _myIp = "broadcast";
    public int i = 0;
    private IPEndPoint anyIP;
    //textmesh object
    public TextMeshProUGUI _text;

    public IPAddress _myIpAddress;
    private string recIp;
    // start from shell
    private static void Main()
    {
       SimpleUdpReceiverV3 receiveObj=new SimpleUdpReceiverV3();
       receiveObj.init();
 
        string text="";
        do
        {
             text = Console.ReadLine();
        }
        while(!text.Equals("exit"));
    }
    // start from unity3d
    public void Start()
    {
        _myIpAddress = IPAddress.Any;
        init();
    }
   
       
    // init
    private void init()
    {
        print("UDPSend.init()");

        print("Sending to 127.0.0.1 : "+port);
        print("Test-Sending to this Port: nc -u 127.0.0.1  "+port+"");
 
        receiveThread = new Thread(
            new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
 
    }
 
    // receive thread
    private  void ReceiveData()
    {
        _client = new UdpClient(port);
        //discard old packets
        _client.Client.ReceiveTimeout = 50;
        while (true)
        {
            try
            {
                anyIP = new IPEndPoint(_myIpAddress, 0);
                byte[] data = _client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);
                lastReceivedUDPPacket=text;
                allReceivedUDPPackets=allReceivedUDPPackets+text;
                //if receive ip is same as mine continue
                Debug.Log("received on " + anyIP.Address.ToString());
                Debug.Log("I am " + _myIpAddress.ToString());
                
                System.Random random = new System.Random();

                float red = (float)random.NextDouble();
                float green = (float)random.NextDouble();
                float blue = (float)random.NextDouble();

                _color = new Color(red, green, blue);
                text = text + i.ToString();
                i++;
                data = Encoding.UTF8.GetBytes(text);
                _client.Send(data, data.Length, anyIP.Address.ToString(), _sendPort);

                
            }
            catch (Exception err)
            {
                print(err.ToString());
                errors = err.ToString();
                _client.Close();
                MEGAERROR = true;
                receiveThread.Abort();
                receiveThread = new Thread(
                    new ThreadStart(ReceiveData));
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
        }
    }
    
    private bool MEGAERROR = false;
    private string errors = "";
    private void FixedUpdate()
    {
        _image.color = _color;
        _text.SetText("Last Received Packet: \n"+ lastReceivedUDPPacket
                    + "\n\nLast Sent Packet: \n" + lastSentUDPPacket
                    + "\n\nRobot IP: "+ _robotIp
                    + "\n\nState: "+ _state
                    + "\n\nFrom IP: "+ recIp
                    + "\n\nI am: "+ _myIpAddress.ToString()
                    );
        if (MEGAERROR)
        {
        _text.SetText(errors);
        }
    }
    public string getLatestUDPPacket()
    {
        allReceivedUDPPackets="";
        return lastReceivedUDPPacket;
    }

    private string ParseIp(string text)
    {
        try{
            //split text by space
            string[] splitText = text.Split(' ');
            //if size of splitText is not 2 return "ERROR"
            if (splitText.Length != 2)
            {
                Debug.Log("Error parsing ip: splitText.Length != 2");
                return "ERROR";
            }
            //return last element
            return splitText[splitText.Length - 1];
        }
        catch (Exception err)
        {
            Debug.Log("Error parsing ip: " + err.ToString());
            return "ERROR";
        }
    }

    void OnApplicationQuit()
    {
        _client.Close();
    }
}