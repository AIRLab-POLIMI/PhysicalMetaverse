using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class UDPCameraViewer : MonoBehaviour
{
    // Public camera
    public GameObject _camera;
    //private orientation transform
    public Transform _orientationTransform;
    
    // UDP settings
    public string serverIP = "192.168.0.103"; // Change this to the IP address of the receiver
    public int serverPort = 40616;       // Change this to the port number of the receiver
    public RawImage rawImage;

    private int udpPort = 12345; // UDP port to listen on
    private UdpClient udpClient;
    private Texture2D receivedTexture;
    private Texture2D lastValidReceivedTexture;
    private bool isReceiving = false;
    private byte[] data;
    private Thread receiveThread;

    private void Start()
    {
        udpClient = new UdpClient(udpPort);
        udpClient.Client.ReceiveTimeout = 2000; // Set the UDP socket timeout to 2 seconds

        receivedTexture = new Texture2D(1, 1);
        lastValidReceivedTexture = new Texture2D(1, 1);
        rawImage.texture = lastValidReceivedTexture;

        _orientationTransform = _camera.transform;

        //start receive frames on separate thread using thrading
        Thread receiveThread = new Thread(new ThreadStart(ReceiveFrames));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        //udp send ay:-3_ax:-1_az:1_gx:-2_gy:0_gz:1\n
        //udpClient.Send(System.Text.Encoding.ASCII.GetBytes("ay:-3_ax:-1_az:1_gx:-2_gy:0_gz:1\n"), 28, serverIP, serverPort);
    }

    //prev send time
    private float _prevSendTime = 0;
    public float _deltaSendTime = 0.05f;
    public float _ySensitivity = 1/20f;
    public float _xSensitivity = 1/30f;

    void FixedUpdate()
    {
        // Send the camera's X and Y angles via UDP every 0.05 seconds
        if (Time.time - _prevSendTime > _deltaSendTime)
        {
            _prevSendTime = Time.time;
            SendCameraAnglesUDP(_camera.transform.eulerAngles.x, _camera.transform.eulerAngles.y);
        }
    }
    // Send camera angles via UDP
    void SendCameraAnglesUDP(float xAngle, float yAngle)
    {
        //if xangle > 180 then xangle = xangle - 360
        if (xAngle > 180)
        {
            xAngle = xAngle - 360;
            //map xangle from 0 360 to -3 3
            xAngle = xAngle / _ySensitivity;
        }
        else
        {
            //map xangle from 0 360 to -3 3
            xAngle = (xAngle) / _ySensitivity;
        }
        xAngle = -xAngle;
        //if yangle > 180 then yangle = yangle - 360
        if (yAngle > 180)
        {
            yAngle = yAngle - 360;
            //map yangle from 0 360 to -3 3
            yAngle = yAngle / _xSensitivity;
        }
        else
        {
            //map yangle from 0 360 to -3 3
            yAngle = (yAngle) / _xSensitivity;
        }
        yAngle = -yAngle;
        //clamp xAngle and yAngle between 3 and -3
        xAngle = Mathf.Clamp(xAngle, -3, 3);
        yAngle = Mathf.Clamp(yAngle, -3, 3);
        string message = "az:" + yAngle.ToString() + "_ay:" + xAngle.ToString() + "\n";
        //replace all "," with "."
        message = message.Replace(",", ".");
        byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
        udpClient.Send(data, data.Length, serverIP, serverPort);
        //log
        Debug.Log("Sent: " + message);
    }

    void Update(){
        // Create a new texture from the received data
        receivedTexture.LoadImage(data);
        //if imgage is square continue
        if (receivedTexture.width == receivedTexture.height)
        {
            Debug.LogWarning("Received image is square. Image will not be displayed.");
        }
        else
        {
            lastValidReceivedTexture.LoadImage(data);
        }
        //if lastvalid is not null apply
        if (lastValidReceivedTexture != null)
        {
            //set rawimage resolution to received image's one
            //rawImage.rectTransform.sizeDelta = new Vector2(receivedTexture.width, receivedTexture.height);
            receivedTexture.Apply();

            // Adjust the raw image aspect ratio
            float aspectRatio = (float)receivedTexture.width / receivedTexture.height;
            rawImage.rectTransform.sizeDelta = new Vector2(rawImage.rectTransform.rect.height * aspectRatio, rawImage.rectTransform.rect.height);

            // Uncomment the next line if you want to flip the texture vertically (useful for some cameras)
            // receivedTexture.Apply(false, true);
        }
    }
    // Print on GUI the camera's angles
    /*void OnGUI()
    {
        // GUI background style
        GUIStyle customStyle = new GUIStyle(GUI.skin.box);
        customStyle.fontSize = 20;
        customStyle.normal.textColor = Color.white;
        GUI.backgroundColor = Color.grey;

        // Show angles in the center of the screen
        GUI.Box(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 200, 100),
            "Camera Angles: \n" + "X: " + _camera.transform.eulerAngles.x + "\nY: " + _camera.transform.eulerAngles.y,
            customStyle);
    }*/

    private void ReceiveFrames()
    {
        isReceiving = true;
        while (isReceiving)
        {
            try
            {
                IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, udpPort);
                data = udpClient.Receive(ref remoteIpEndPoint);


            }
            catch (SocketException e)
            {
                Debug.LogWarning("Socket exception: " + e.Message);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error receiving frame: " + e.Message);
            }

        }
    }

    private void OnApplicationQuit()
    {
        //end thread
        isReceiving = false;
        //close thread
        receiveThread.Abort();

        udpClient.Close();
    }
}
