using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System;
//using random
using Random = UnityEngine.Random;

public class PoseController : MonoBehaviour
{
    //list of joints gameobjects transforms
    public Transform[] _jointsTransforms;
    private UdpClient _udpClient;
    private int _udpPort = 44444;
    private bool _isReceiving = false;
    private byte[] _data;
    public float _scalePose = 1000f;
    public float _sphereScale = 0.05f;
    // Start is called before the first frame update
    void Start()
    {
        _udpClient = new UdpClient(_udpPort);
        _udpClient.Client.ReceiveTimeout = 2000; // Set the UDP socket timeout to 2 seconds
        //start receive frames on separate thread using thrading
        Thread receiveThread = new Thread(new ThreadStart(ReceivePose));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        
        //spawn 35 spheres children of this gameobject and populate transforms array with their transforms
        /*_jointsTransforms = new Transform[35];
        for (int i = 0; i < 35; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.parent = transform;
            sphere.transform.localScale = new Vector3(1f, 1f, 1f);
            sphere.transform.localPosition = new Vector3(0f, 0f, 0f);
            _jointsTransforms[i] = sphere.transform;
            //random color to each sphere
            sphere.GetComponent<Renderer>().material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            //add component client network transform
            sphere.AddComponent<ClientNetworkTransform>();

        }*/
        //find gameobjects containing "Sphere" in their name and fill transforms array with their transforms
        GameObject[] spheres = GameObject.FindGameObjectsWithTag("PoseTracker");
        _jointsTransforms = new Transform[spheres.Length];
        for (int i = 0; i < spheres.Length; i++)
        {
            _jointsTransforms[i] = spheres[i].transform;
            //give random color
            spheres[i].GetComponent<Renderer>().material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            //scale
            spheres[i].transform.localScale = new Vector3(_sphereScale, _sphereScale, _sphereScale);
        }

        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        SetPose();
    }

    /*OLD SetPose
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
    }*/

    private void SetPose()
    {
        if (_data != null)
        {
            //dat contains [x_y_z,x_y_z,x_y_z] where x y z are the position to set to the nth joint
            /*string[] jointsPos = System.Text.Encoding.ASCII.GetString(_data).Split(',');
            for (int i = 0; i < jointsPos.Length; i++)
            {
                string[] jointPos = jointsPos[i].Split('_');
                //set position relative to father, coordinates / _scalePose
                _jointsTransforms[i].localPosition = new Vector3(float.Parse(jointPos[0]) / _scalePose, float.Parse(jointPos[1]) / _scalePose, float.Parse(jointPos[2]) / _scalePose);
                //log floats
                Debug.Log(float.Parse(jointPos[0]) + " " + float.Parse(jointPos[1]) + " " + float.Parse(jointPos[2]));
            }*/
            //data comes in format
            /*[[ 401  282  -66]
 [ 368  245  -80]
 [ 367  226  -80]
 [ 365  203  -80]
 [ 372  266  -56]
 [ 372  267  -56]
 [ 374  269  -56]
 [ 378  141 -103]
 [ 386  216    7]
 [ 431  225  -72]
 [ 434  270  -40]
 [ 516   57 -130]
 [ 563  201   66]
 [ 608  159 -161]
 [ 749  280  102]
 [ 591  264 -133]
 [ 862  375   63]
 [ 583  262 -146]
 [ 892  405   57]
 [ 585  261 -144]
 [ 894  398   39]
 [ 592  261 -132]
 [ 887  382   52]
 [1060  165  -63]
 [1046  270   63]
 [1313  230  -80]
 [1297  341   73]
 [1640  293    8]
 [1625  370   98]
 [1700  270   15]
 [1692  354   98]
 [1699  411  -13]
 [1661  501   62]
 [1031  236    0]
 [ 165   80    0]]*/
            //set position relative to father, coordinates / _scalePose using for loop, up to the size of transforms array
            //split
            
            
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
