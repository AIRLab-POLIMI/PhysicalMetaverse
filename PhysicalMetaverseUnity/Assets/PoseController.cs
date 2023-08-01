using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class PoseController : MonoBehaviour
{
    //list of joints gameobjects transforms
    public Transform[] _jointsTransforms;
    private UdpClient _udpClient;
    private int _udpPort = 44444;
    private bool _isReceiving = false;
    private byte[] _data;
    public float _scalePose = 1000f;
    // Start is called before the first frame update
    void Start()
    {
        _udpClient = new UdpClient(_udpPort);
        _udpClient.Client.ReceiveTimeout = 2000; // Set the UDP socket timeout to 2 seconds
        //start receive frames on separate thread using thrading
        Thread receiveThread = new Thread(new ThreadStart(ReceivePose));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        SetPose();
    }

    private void SetPose()
    {
        if (_data != null)
        {
            //dat contains [x_y_z,x_y_z,x_y_z] where x y z are the position to set to the nth joint
            string[] jointsPos = System.Text.Encoding.ASCII.GetString(_data).Split(',');
            for (int i = 0; i < jointsPos.Length; i++)
            {
                string[] jointPos = jointsPos[i].Split('_');
                //set position relative to father, coordinates / _scalePose
                _jointsTransforms[i].localPosition = new Vector3(float.Parse(jointPos[0]) / _scalePose, float.Parse(jointPos[1]) / _scalePose, float.Parse(jointPos[2]) / _scalePose);
                //log floats
                Debug.Log(float.Parse(jointPos[0]) + " " + float.Parse(jointPos[1]) + " " + float.Parse(jointPos[2]));
            }
        }
    }
    private void ReceivePose()
    {
        _isReceiving = true;
        while (_isReceiving)
        {
            try
            {
                IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, _udpPort);
                _data = _udpClient.Receive(ref remoteIpEndPoint);


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
}
