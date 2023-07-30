using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class UDPCameraViewer : MonoBehaviour
{
    // Public camera
    public Camera _camera;
    //private orientation transform
    public Transform _orientationTransform;
    
    // UDP settings
    public string serverIP = "127.0.0.1"; // Change this to the IP address of the receiver
    public int serverPort = 12345;       // Change this to the port number of the receiver
    public RawImage rawImage;

    private int udpPort = 12345; // UDP port to listen on
    private UdpClient udpClient;
    private Texture2D receivedTexture;
    private bool isReceiving = false;

    private void Start()
    {
        udpClient = new UdpClient(udpPort);
        udpClient.Client.ReceiveTimeout = 2000; // Set the UDP socket timeout to 2 seconds

        receivedTexture = new Texture2D(1, 1);
        rawImage.texture = receivedTexture;

        _orientationTransform = _camera.transform;

        StartCoroutine(ReceiveFrames());
    }

    void FixedUpdate()
    {
        // Send the camera's X and Y angles via UDP
        SendCameraAnglesUDP(_camera.transform.eulerAngles.x, _camera.transform.eulerAngles.y);
    }

    // Send camera angles via UDP
    void SendCameraAnglesUDP(float xAngle, float yAngle)
    {
        string message = "X:" + xAngle.ToString() + "_Y:" + yAngle.ToString();
        byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
        udpClient.Send(data, data.Length, serverIP, serverPort);
        //log
        Debug.Log("Sent: " + message);
    }

    // Print on GUI the camera's angles
    void OnGUI()
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
    }

    private IEnumerator ReceiveFrames()
    {
        isReceiving = true;

        while (isReceiving)
        {
            try
            {
                IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, udpPort);
                byte[] data = udpClient.Receive(ref remoteIpEndPoint);

                // Create a new texture from the received data
                receivedTexture.LoadImage(data);
                //set rawimage resolution to received image's one
                //rawImage.rectTransform.sizeDelta = new Vector2(receivedTexture.width, receivedTexture.height);
                receivedTexture.Apply();

                // Adjust the raw image aspect ratio
                float aspectRatio = (float)receivedTexture.width / receivedTexture.height;
                rawImage.rectTransform.sizeDelta = new Vector2(rawImage.rectTransform.rect.height * aspectRatio, rawImage.rectTransform.rect.height);

                // Uncomment the next line if you want to flip the texture vertically (useful for some cameras)
                // receivedTexture.Apply(false, true);

            }
            catch (SocketException e)
            {
                Debug.LogWarning("Socket exception: " + e.Message);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error receiving frame: " + e.Message);
            }

            yield return null;
        }
    }

    private void OnApplicationQuit()
    {
        udpClient.Close();
    }
}
