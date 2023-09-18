//this script should receive udp messages in unity and log them
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

//to test run the scene while jetson is running "python3 demo.py" in ~/Desktop/TesiMaurizioVetere/ProgettiPython/depthai_blazepose
public class UDPReceiver : MonoBehaviour
{
    //make a slider to adjust the rotation angle
    //public float rotationAngle = -20f;
    [Range(-90f, 90f)]
    public float rotationAngle = -20f;

    //listen for udp messages on port 5005
    static int port = 5005;
    //udpclient object
    private UdpClient client;
    //udp packet storage
    private byte[] data;
    private int[][] parsedData;

    //spawned
    private bool spawned = false;

    private GameObject[] spheres;
    void Start()
    {
        //create udpclient object
        client = new UdpClient(port);
        //udp packets are sent as byte data
        data = new byte[1024];
        //begin listening for messages
        client.BeginReceive(new AsyncCallback(recv), null);
    }

    //recv
    private void recv(IAsyncResult res)
    {
        //store the remote endpoint
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, port);
        //get the data
        data = client.EndReceive(res, ref RemoteIpEndPoint);
        //get the message
        string message = Encoding.ASCII.GetString(data);
        //log the message
        Debug.Log(message);
        //parse the message
        parsedData = ParseData(message);
        //Debug.Log(ParseData(message));
        //listen for new messages
        client.BeginReceive(new AsyncCallback(recv), null);
    }

    //a receive looks like
/*
[[ 226   63  -22]
[ 239   46  -17]
[ 244   46  -17]
[ 249   47  -17]
[ 224   46  -20]
[ 219   45  -20]
[ 215   45  -20]
[ 257   62    7]
[ 212   59    0]
[ 236   89  -13]
[ 217   88  -15]
[ 259  214   27]
[ 164  147  -20]
[ 198  360   10]
[  87  181  -65]
[ 125  447  -18]
[  46  129 -116]
[ 109  478  -22]
[  30  108 -133]
[ 102  467  -33]
[  34  109 -132]
[ 105  456  -23]
[  40  121 -118]
[  61  453   19]
[   4  417  -20]
[ 128  529    0]
[  74  514  -47]
[  59  614   69]
[  20  583   35]
[  48  625   75]
[  13  586   44]
[  50  650   75]
[   5  628   52]
[  29  437    0]
[ 331   10    0]]
 */
    //function to parse it into an array of arrays of 3 integers
    private int[][] ParseData(string data)
    {
        //split the data into lines
        string[] lines = data.Split('\n');
        //create an array of arrays of 3 integers
        int[][] parsedData = new int[lines.Length][];
        //for each line
        for (int i = 0; i < lines.Length; i++)
        {
            //log
            //Debug.Log(lines[i]);
            //split the line into integers, spit on variable number of spaces
            string[] integers = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int j = 0; j < integers.Length; j++)
            {
                //remove any space, [, or ]
                integers[j] = integers[j].Replace(" ", "").Replace("[", "").Replace("]", "");
                //if the integer is empty discard it
                if (integers[j] == "")
                {
                    //remove the integer
                    integers[j] = null;
                }
            }

            //store non empy elements into a new integers array
            int[] newIntegers = new int[3];
            //index for newIntegers
            int index = 0;
            //for each integer
            for (int j = 0; j < integers.Length; j++)
            {
                //if the integer is not null
                if (integers[j] != null)
                {
                    //parse the integer
                    int integer = int.Parse(integers[j]);
                    //store the integer in newIntegers
                    newIntegers[index] = integer;
                    //increment index
                    index++;
                }
            }
            //Debug.Log(newIntegers);
            parsedData[i] = newIntegers;
        }
        /*string parsed = "";
        //log all elements of parsedData
        for (int i = 0; i < parsedData.Length; i++)
        {
            for (int j = 0; j < parsedData[i].Length; j++)
            {   
                parsed += parsedData[i][j] + " ";
            }
            parsed += "\n";
        }
        Debug.Log(parsed);
        */
        //return the array of arrays
        return parsedData;
    }

    //at the first receive spawn one sphere for each element fo the array, then at each receive move the spheres to the new position
    //data is an array of numbers not a string
    private void FixedUpdate()
    {
        //if first receive
        if (data.Length > 0)
        {
            if(!spawned)
                //call function to spawn spheres
                SpawnSpheres();
            
            //move spheres to position
            MoveSpheres();
        }

    }

    //spawn spheres
    private void SpawnSpheres()
    {
        spheres = new GameObject[parsedData.Length];
        //spawn a sphere for each element in the array
        for (int i = 0; i < parsedData.Length; i++)
        {
            //spawn a sphere
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //name the sphere
            sphere.name = "Sphere (" + i + ")";
            //scale 10
            sphere.transform.localScale = new Vector3(80f/scale, 80f/scale, 80f/scale);
            //from 0 to 10 make spheres red, from 11 to 21 make spheres yellow only on odd numbers, from 10 to 20 make spheres orange only on even numbers,
            //from 24 to 32 make spheres green only on even numbers, from 23 to 31 make spheres blue only on odd numbers
            if (i >= 0 && i <= 10)
            {
                sphere.GetComponent<Renderer>().material.color = Color.cyan;
            }
            if (i >= 11 && i <= 21)
            {
                if (i % 2 == 1)
                {
                    sphere.GetComponent<Renderer>().material.color = Color.yellow;
                }
            }
            if (i >= 10 && i <= 20)
            {
                if (i % 2 == 0)
                {
                    sphere.GetComponent<Renderer>().material.color = Color.red;
                }
            }
            if (i >= 24 && i <= 32)
            {
                if (i % 2 == 0)
                {
                    sphere.GetComponent<Renderer>().material.color = Color.green;
                }
            }
            if (i >= 23 && i <= 31)
            {
                if (i % 2 == 1)
                {
                    sphere.GetComponent<Renderer>().material.color = Color.blue;
                }
            }
            //set the position of the sphere
            sphere.transform.position = new Vector3(i, 0, 0);
            //add the sphere to the spheres array
            spheres[i] = sphere;
        }
        //set spawned to true
        spawned = true;
        //set sphere34 if sphere name is sphere 34
        sphere34 = spheres[33];
    }
    //gameobject sphere 34
    private GameObject sphere34;
    [Range(1f, 5000f)]
    public float scale = 169f;

    [Range(-100f, 100f)]
    public float zOffset = 5.5f;

    [Range(0f, 100f)]
    public float zMultiplier = 1f;

    //move spheres
    private void MoveSpheres()
    {
        try{
            //for each sphere
            for (int i = 0; i < parsedData.Length; i++)
            {
                //get the sphere
                GameObject sphere = spheres[i];
                //set positions
                //sphere.transform.position = new Vector3(parsedData[i][1], parsedData[i][0], parsedData[i][2]);
                //should rotate z by 45 degrees. if a point has y = 0 z is unchanged, if a point has y = 100, z is brought closer
                sphere.transform.position = new Vector3(parsedData[i][1]/scale, parsedData[i][0]/scale, parsedData[i][2]/scale + zOffset + 1/sphere34.transform.position.y * zMultiplier);
                sphere.transform.localScale = new Vector3(80f/scale, 80f/scale, 80f/scale);
                //rotate spheres position by 45 degrees with fulcrum at 
                Vector3 rotationAxis = Vector3.right; // You can adjust the axis according to your requirements

                // Specify the rotation angle in degrees
                //float rotationAngle = -20f; // You can adjust the angle as desired
                //rotation center in 0 0
                Vector3 rotationCenter = new Vector3(224.1144f/scale, -26f/scale, -359.3866f/scale); // You can adjust the center of rotation as desired
                // Rotate the sphere around the center of rotation
                sphere.transform.RotateAround(rotationCenter, rotationAxis, rotationAngle);
                //move gradually
                //sphere.transform.position = Vector3.Lerp(sphere.transform.position, new Vector3(parsedData[i][0], parsedData[i][1], parsedData[i][2]), 0.05f);

            }
        }
        catch(Exception e)
        {
            Debug.Log(":-)");
            //log size of data
            Debug.Log(parsedData.Length);
            //log size of first element
            Debug.Log(parsedData[0].Length);
            //log spheres size
            Debug.Log(spheres.Length);
        }
    }
    private void OnApplicationQuit()
    {
        //stop listening
        client.Close();
    }
}

