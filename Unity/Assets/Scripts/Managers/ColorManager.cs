//this script should receive udp messages in unity and log them
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

//to test run the scene while jetson is running "python3 demo.py" in ~/Desktop/TesiMaurizioVetere/ProgettiPython/depthai_blazepose
//this manager receives x, y of the biggest blob of one chosen color in image frame and positions a sphere at such coordinates
public class ColorManager : MonoBehaviour
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

    public void OnMsgRcv(byte[] msg)
    {
        string message = Encoding.ASCII.GetString(msg);
        //log the message
        Debug.Log("Color\n" + message);
        parsedData = ParseData(message);
    }
    void Start()
    {
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
        if (parsedData != null)
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
        try{
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
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }

    //move spheres
    private void MoveSphere()
    {
        try{
            //get the sphere
            GameObject sphere = GameObject.Find("Sphere");
            //set the sphere's position
            sphere.transform.position = new Vector3(parsedData[1]/100, parsedData[0]/100, 0);
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }
}


