using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;

public class UdpCameraStreamer : MonoBehaviour
{
    //streams a rawimage via udp to target ip and port
    public RenderTexture sendTexture;
    public string targetIP = "127.0.0.1";
    public int targetPort = 25667;
    public UdpClient udpClient;
    // Start is called before the first frame update
    void Start()
    {
        //set up udp socket to send
        udpClient = new UdpClient();
        udpClient.Connect(targetIP, targetPort);
        //socket size
        udpClient.Client.SendBufferSize = 65536;

    }

    //prevtime
    float _prevSendTime = 0;
    // Update is called once per frame
    void Update()
    {
        //send only if 60ms have passed
        if (Time.time - _prevSendTime > 0.06f)
        {
            _prevSendTime = Time.time;
            SendTexture();
        }
    }

    void SendTexture(){
        if (sendTexture != null)
        {
            //send to ip
            Texture2D tex = new Texture2D(sendTexture.width, sendTexture.height, TextureFormat.RGB24, false);
            RenderTexture.active = sendTexture;
            tex.ReadPixels(new Rect(0, 0, sendTexture.width, sendTexture.height), 0, 0);
            tex.Apply();
            //jpg compress
            byte[] jpg = tex.EncodeToJPG(50);
            udpClient.Send(jpg, jpg.Length);
            //avoid memory leak
            Destroy(tex);
            
        }
    }
}
