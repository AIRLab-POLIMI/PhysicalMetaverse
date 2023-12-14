//this script should receive udp messages in unity and log them
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

//to test run the scene while jetson is running "python3 demo.py" in ~/Desktop/TesiMaurizioVetere/ProgettiPython/depthai_blazepose
public class UDPReceiverColor : MonoBehaviour
{
    //listen for udp messages on port 5004
    static int port = 5004;
    //udpclient object
    private UdpClient client;
    //udp packet storage
    private byte[] data;
    private int[] parsedData;

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
[460, 590]
*/
    //function to parse it into an array of arrays of 3 integers
    private int[] ParseData(string data)
    {
        //mind the comma and the space
        string[] splitData = data.Split(new string[] { ", " }, StringSplitOptions.None);
        int[] parsedData = new int[splitData.Length];
        //remove [ and ] from elements of the array
        for (int i = 0; i < splitData.Length; i++)
        {
            splitData[i] = splitData[i].Replace("[", "");
            splitData[i] = splitData[i].Replace("]", "");
            parsedData[i] = int.Parse(splitData[i]);
        }
        //print elements of parsedData
        for (int i = 0; i < parsedData.Length; i++)
        {
            Debug.Log(parsedData[i]);
        }
        return parsedData;
    }

    //at the first receive spawn one sphere for each element fo the array, then at each receive move the spheres to the new position
    //data is an array of numbers not a string
    private void FixedUpdate()
    {
        //if first receive
        if (parsedData.Length > 0)
        {
            if(!spawned)
                //call function to spawn spheres
                SpawnSphere();
            
            //move spheres to position
            MoveSphere();
        }

    }

    //spawn spheres
    private void SpawnSphere()
    {
        //create a sphere
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        spawned = true;
        //name the sphere
        sphere.name = "Sphere";
        //set the sphere's position
        sphere.transform.position = new Vector3(parsedData[1]/100, parsedData[0]/100, 0);
        //set the sphere's scale
        sphere.transform.localScale = new Vector3(10f, 10f, 10f);
        //set the sphere's color
        sphere.GetComponent<Renderer>().material.color = Color.blue;
    }

    //move spheres
    private void MoveSphere()
    {
        //get the sphere
        GameObject sphere = GameObject.Find("Sphere");
        //set the sphere's position
        sphere.transform.position = new Vector3(parsedData[1]/100, parsedData[0]/100, 0);
    }
    private void OnApplicationQuit()
    {
        //stop listening
        client.Close();
    }
}

