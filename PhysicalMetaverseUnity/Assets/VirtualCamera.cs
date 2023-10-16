using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using System.Threading;
using System.Diagnostics;

public class VirtualCamera : MonoBehaviour
{
    //camera _XRCamera
    public Camera _XRCamera;
    //camera _VirtualCamera
    public GameObject _virtualCamera;
    public GameObject _robotBase;

    private Thread pythonThread;
    private Process process;
    // Path to your OpenCV Python script.
    [SerializeField] string pythonScriptPath = "C:/Users/Alessandro/Documents/Maurizio/PhysicalMetaverse/Python/MultipleQRDetect.py";

    // Start is called before the first frame update
    void Start()
    {
        //disable TrackedPoseDriver
        _XRCamera.GetComponent<TrackedPoseDriver>().enabled = false;
        //run python script C:\Users\Alessandro\Documents\Maurizio\PhysicalMetaverse\Python\MultipleQRDetect.py

        // Start a new thread to run the Python script.
        pythonThread = new Thread(ExecutePythonScript)
        {
            //set daemon to true
            IsBackground = true
        };
        pythonThread.Start();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //set XRCamera local rotation to VirtualCamera rotation minus robotBase rotation euler angles
        _XRCamera.transform.localRotation = Quaternion.Euler(_virtualCamera.transform.rotation.eulerAngles - _robotBase.transform.rotation.eulerAngles);
        
    }

    void ExecutePythonScript()
    {
        // Path to the Python interpreter (change this to your Python executable path).
        string pythonPath = "python";

        // Create a new process to run the OpenCV Python script.
        process = new Process();
        process.StartInfo.FileName = pythonPath;
        process.StartInfo.Arguments = pythonScriptPath;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        // Start the process.
        process.Start();

        // Read the output and error streams (optional).
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        // Wait for the process to finish.
        process.WaitForExit();

        // You can print the output and error to the Unity console (optional).
        UnityEngine.Debug.Log("OpenCV Script Output:\n" + output);
        UnityEngine.Debug.LogError("OpenCV Script Error:\n" + error);
    }

    void OnApplicationQuit()
    {
        // Terminate the Python process and thread when Unity is stopped.
        if (process != null && !process.HasExited)
        {
            process.Kill();
            process.WaitForExit();
            process.Dispose();
        }
        if (pythonThread != null && pythonThread.IsAlive)
        {
            pythonThread.Abort();
        }
    }
}
