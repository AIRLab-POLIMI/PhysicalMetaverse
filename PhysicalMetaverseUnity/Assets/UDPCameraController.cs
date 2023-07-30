using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class UDPCameraController : MonoBehaviour
{
    // Public camera
    public Camera camera;

    // UDP settings
    public string serverIP = "127.0.0.1"; // Change this to the IP address of the receiver
    public int serverPort = 12345;       // Change this to the port number of the receiver

    private UdpClient udpClient;

    // Start is called before the first frame update
    void Start()
    {
        udpClient = new UdpClient();
    }

    // Update is called once per frame
    void Update()
    {
        // Send the camera's X and Y angles via UDP
        SendCameraAnglesUDP(camera.transform.eulerAngles.x, camera.transform.eulerAngles.y);
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
            "Camera Angles: \n" + "X: " + camera.transform.eulerAngles.x + "\nY: " + camera.transform.eulerAngles.y,
            customStyle);
    }

    private void OnApplicationQuit()
    {
        udpClient.Close();
    }
}