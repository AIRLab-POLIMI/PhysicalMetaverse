
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

public class UdpMessenger
{
    #region FIELDS

        private Thread _receiveThread;

        private int _maxMsgAge;

        private int _bufferSize;

        private UdpClient _client;

        private List<UdpMessage> _unreadUdpMsgs = new List<UdpMessage>();

    #endregion

    #region GETTERS/SETTERS
    
        // IP AND PORT

        public int MyPort { get; private set; }
        
        public IPEndPoint DefaultDestinationEndPoint { get; private set; }
        
        public IPAddress DefaultDestinationIP { get; private set; }
        
        public int DefaultDestinationPort { get; private set; }
        
        //UDP MSG
        
        public  UdpMessage LatestUdpMsg { get; private set; }

        public List<UdpMessage> UnreadUdpMessages
        {
            get
            {
                var temp = _unreadUdpMsgs;
                _unreadUdpMsgs = new List<UdpMessage>();
                return temp;
            }
        }

        public bool UnreadMsgsPresent => _unreadUdpMsgs.Count > 0;

    #endregion

    #region SETUP

    public void Init(
        IPEndPoint defaultDestination,
        int sourcePort = -1,
        int maxMsgAge = 5,
        int bufferSize = 20)
    {
        DefaultDestinationEndPoint = defaultDestination;
        DefaultDestinationIP = defaultDestination.Address;
        DefaultDestinationPort = defaultDestination.Port;

        this.MyPort = sourcePort;
        if (this.MyPort <= -1)
        {
            _client = new UdpClient();
            Debug.Log("Sending to " + DefaultDestinationIP + ": " + this.DefaultDestinationPort);
        }
        else
        {
            _client = new UdpClient(this.MyPort);
            Debug.Log("Sending to " + DefaultDestinationIP + ": " + this.DefaultDestinationPort + " from Source Port: " + this.MyPort);
        }

        _maxMsgAge = maxMsgAge;
        _bufferSize = bufferSize;

        _receiveThread = new Thread(
            new ThreadStart(ReceiveData));

        _receiveThread.IsBackground = true;
        _receiveThread.Start();
        
        Debug.Log("FINITO UDP INIT");
    }
    
    #endregion

    #region SEND

    public void SendUdp(byte[] data, IPEndPoint destination = null)
    {
        try
        {
            _client.Send(data, data.Length, destination == null ? DefaultDestinationEndPoint : destination);
        }
        catch (Exception err)
        {
            Debug.Log($"[SendUDP] - ERROR: '{err.ToString()}'");
        }
    }

    #endregion


    #region RECEIVE UDP

    private void ReceiveData()
    {
        while (true)
        {
            try
            {
                OnMessageReceived();
            }
            catch (Exception err)
            {
                Debug.Log("* RECEIVE ERROR: " + err.ToString());
            }
        }
    }

    private void OnMessageReceived()
    {
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
        byte[] data = _client.Receive(ref anyIP);
        //string text = Encoding.UTF8.GetString(data);

        LatestUdpMsg = new UdpMessage(data, anyIP);
        _unreadUdpMsgs.Add(LatestUdpMsg);
        
        if (_unreadUdpMsgs.Count > _bufferSize)
            _unreadUdpMsgs.RemoveAt(0);
    }

    #endregion

    #region CLEANUP

    public void TryRemoveOldMessages()
    {
        var now = DateTime.Now;

        for (int i = _unreadUdpMsgs.Count - 1; i >= 0; i--)
        {
            var msg = _unreadUdpMsgs[i];
            if (TimeHelper.TimeSubtractionInMinutes(msg.ReceptionTime, now) > _maxMsgAge)
                _unreadUdpMsgs.Remove(msg);
        }
    }

    public void ClosePorts()
    {
        Debug.Log("Closing receiving UDP on port:" + MyPort);
        
        if (_receiveThread != null)
            _receiveThread.Abort();
        _client.Close();
    }

    #endregion

    #region GET IP ENDPOINT

    public static IPEndPoint GetIPEndPoint(IPAddress ip, int port) =>
        new IPEndPoint(ip, port);

    #endregion
}
