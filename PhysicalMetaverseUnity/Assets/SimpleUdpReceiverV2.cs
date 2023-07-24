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

public class SimpleUdpReceiverV2 : MonoBehaviour {
   
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
    //textmesh object
    public TextMeshProUGUI _text;

    public IPAddress _myIpAddress;
    // start from shell
    private static void Main()
    {
       SimpleUdpReceiverV2 receiveObj=new SimpleUdpReceiverV2();
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
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(_myIpAddress, 0);
                byte[] data = _client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);
                lastReceivedUDPPacket=text;
                allReceivedUDPPackets=allReceivedUDPPackets+text;
                print("Server: " + text);

                if (text.Contains("MetaverseSetup")){
                    Debug.Log("Reset received, waiting for new robot ip");
                    _robotIp = "";
                    _state = "waiting";
                    // yellow
                    _color = new Color(1f, 1f, 0f);
                }

                if (_state == "waiting")
                {
                    //if text is "Metaverse "+ ip
                    if (text.Contains("MetaverseSetup"))
                    {
                        // red
                        _color = new Color(1f, 0f, 0f);
                        _robotIp = ParseIp(text);
                        //split _robotIp by . and replace last element with 255
                        string[] splitIp = _robotIp.Split('.');
                        splitIp[splitIp.Length - 1] = "255";
                        _robotIp = string.Join(".", splitIp);
                        Debug.Log("Broadcast ip is: " + _robotIp);

                        if(_robotIp != "ERROR"){
                            //print all my ips
                            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
                            foreach (IPAddress addr in localIPs)
                            {
                                //if addr contains first 8 characters of _robotIp
                                if (addr.ToString().Contains(_robotIp.Substring(0, 8)))
                                {
                                    Debug.Log("My ip is: " + addr.ToString());
                                    _myIp = addr.ToString();
                                    _myIpAddress = addr;
                                }
                            }
                            //send my ip to the robot
                            string sending = "Unity " + _myIp;
                            byte[] data2 = Encoding.UTF8.GetBytes(sending);
                            //send to _robotIp
                            _client.Send(data2, data2.Length, _robotIp, _sendPort);
                            lastSentUDPPacket = sending;
                            Debug.Log("I sent my ip to the robot");
                            _state = "found";
                            // blue
                            _color = new Color(0f, 0f, 1f);
                        }
                    }
                }
                else if (_state == "found")
                {
                    //if text is "Unity "+ ip
                    if (text.Contains("YouAre"))
                    {
                        //send my ip to the robot
                        string checkIp = ParseIp(text);
                        if(checkIp != "ERROR"){
                            if (checkIp == _myIp){
                                Debug.Log("Robot found me, let's begin");
                                _state = "ready";
                                // green
                                _color = new Color(0f, 1f, 0f);
                            }
                        }
                    }
                }
                else if (_state == "ready")
                {
                    //text = text + random number
                    text = text + " " + i.ToString();
                    i++;
                    //send received message back to sender
                    byte[] data2 = Encoding.UTF8.GetBytes(text);
                    if(_robotIp != "ERROR"){
                        //send to _robotIp
                        _client.Send(data2, data2.Length, _robotIp, _sendPort);
                            lastSentUDPPacket = text;
                        Debug.Log("I sent the robot's message back to the robot");
                        System.Random random = new System.Random();
        
                        float red = (float)random.NextDouble();
                        float green = (float)random.NextDouble();
                        float blue = (float)random.NextDouble();
        
                        _color = new Color(red, green, blue);
                    }
                    else{
                        Debug.Log("Robot ip not found");
                        _state = "waiting";
                        // red
                        _color = new Color(1f, 0f, 0f);
                    }
                }
                else
                {
                    Debug.Log("State not recognized");
                }
                //_image.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                //System.Random random = new System.Random();
//
                //float red = (float)random.NextDouble();
                //float green = (float)random.NextDouble();
                //float blue = (float)random.NextDouble();
//
                //_color = new Color(red, green, blue);

            }
            catch (Exception err)
            {
                print(err.ToString());
                errortext = err.ToString();
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
    private string errortext = "";
    private void FixedUpdate()
    {
        _image.color = _color;
        _text.SetText("Last Received Packet: \n"+ lastReceivedUDPPacket
                    + "\n\nLast Sent Packet: \n" + lastSentUDPPacket
                    + "\n\nRobot IP: "+ _robotIp
                    + "\n\nState: "+ _state);
        if (MEGAERROR)
        {
        _text.SetText("Last Received Packet: \n"+ lastReceivedUDPPacket
                    + "\n\nLast Sent Packet: \n" + lastSentUDPPacket
                    + "\n\nRobot IP: "+ _robotIp
                    + "\n\nState: "+ _state
                    + "\n\n"+ errortext);
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