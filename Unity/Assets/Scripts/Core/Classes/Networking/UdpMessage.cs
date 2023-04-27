using System;
using System.Net;

public class UdpMessage
{
    public byte[] Msg { get; private set; }
    
    public IPEndPoint Sender { get; private set; }
    
    public DateTime ReceptionTime { get; private set; }

    public UdpMessage(byte[] msg, IPEndPoint sender)
    {
        Msg = msg;
        Sender = sender;
        ReceptionTime = DateTime.Now;
    }
}


