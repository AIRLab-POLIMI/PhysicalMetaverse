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

    private Thread _qrThread;
    private Process _qrProcess;
    private Thread _poseThread;
    private Process _poseProcess;
    // Path to your OpenCV Python script.
    [SerializeField] string _qrDetectionScriptPath = "C:/Users/Alessandro/Documents/Maurizio/PhysicalMetaverse/Python/MultipleQRDetect.py";
    [SerializeField] string _poseDetectionScriptPath = "C:/Users/Alessandro/Documents/Maurizio/PhysicalMetaverse/PhysicalMetaverseUnity/Assets/Code and Prefabs/Python Scripts/webcamPoseRecognition.py";

    [SerializeField] bool _ENABLE_QR = true;
    [SerializeField] bool _ENABLE_POSE = true;
    //VR enabled
    [SerializeField] bool _VR_ENABLED = true;
    // Start is called before the first frame update
    void Start()
    {
        if(!_VR_ENABLED)
            //disable TrackedPoseDriver
            _XRCamera.GetComponent<TrackedPoseDriver>().enabled = false;
        //run python script C:\Users\Alessandro\Documents\Maurizio\PhysicalMetaverse\Python\MultipleQRDetect.py

        if(_ENABLE_QR){
            // Start a new thread to run the Python script.
            _qrThread = new Thread(ExecuteQRScript)
            {
                //set daemon to true
                IsBackground = true
            };
            _qrThread.Start();
        }
        if(_ENABLE_POSE){
            // Start a new thread to run the Python script.
            _poseThread = new Thread(ExecutePoseScript)
            {
                //set daemon to true
                IsBackground = true
            };
            _poseThread.Start();
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!_VR_ENABLED)
            //set XRCamera local rotation to VirtualCamera rotation minus robotBase rotation euler angles
            _XRCamera.transform.localRotation = Quaternion.Euler(_virtualCamera.transform.rotation.eulerAngles - _robotBase.transform.rotation.eulerAngles);
        
    }

    void ExecuteQRScript()
    {
        // Path to the Python interpreter (change this to your Python executable path).
        string pythonPath = "python";

        // Create a new process to run the OpenCV Python script.
        _qrProcess = new Process();
        _qrProcess.StartInfo.FileName = pythonPath;
        _qrProcess.StartInfo.Arguments = _qrDetectionScriptPath;
        _qrProcess.StartInfo.UseShellExecute = false;
        _qrProcess.StartInfo.RedirectStandardOutput = true;
        _qrProcess.StartInfo.RedirectStandardError = true;
        _qrProcess.StartInfo.CreateNoWindow = true;

        // Start the process.
        _qrProcess.Start();

        // Read the output and error streams (optional).
        string output = _qrProcess.StandardOutput.ReadToEnd();
        string error = _qrProcess.StandardError.ReadToEnd();

        // Wait for the process to finish.
        _qrProcess.WaitForExit();

        // You can print the output and error to the Unity console (optional).
        UnityEngine.Debug.Log("OpenCV Script Output:\n" + output);
        UnityEngine.Debug.LogError("OpenCV Script Error:\n" + error);
    }

    void ExecutePoseScript(){
        // Path to the Python interpreter (change this to your Python executable path).
        string pythonPath = "python";

        // Create a new process to run the OpenCV Python script.
        _poseProcess = new Process();
        _poseProcess.StartInfo.FileName = pythonPath;
        _poseProcess.StartInfo.Arguments = _poseDetectionScriptPath;
        _poseProcess.StartInfo.UseShellExecute = false;
        _poseProcess.StartInfo.RedirectStandardOutput = true;
        _poseProcess.StartInfo.RedirectStandardError = true;
        _poseProcess.StartInfo.CreateNoWindow = true;

        // Start the process.
        _poseProcess.Start();

        // Read the output and error streams (optional).
        string output = _poseProcess.StandardOutput.ReadToEnd();
        string error = _poseProcess.StandardError.ReadToEnd();

        // Wait for the process to finish.
        _poseProcess.WaitForExit();

        // You can print the output and error to the Unity console (optional).
        UnityEngine.Debug.Log("OpenCV Script Output:\n" + output);
        UnityEngine.Debug.LogError("OpenCV Script Error:\n" + error);
    }

    void OnApplicationQuit()
    {
        // Terminate the Python process and thread when Unity is stopped.
        if (_qrProcess != null && !_qrProcess.HasExited)
        {
            _qrProcess.Kill();
            _qrProcess.WaitForExit();
            _qrProcess.Dispose();
        }
        if (_poseProcess != null && !_poseProcess.HasExited)
        {
            _poseProcess.Kill();
            _poseProcess.WaitForExit();
            _poseProcess.Dispose();
        }
        if (_qrThread != null && _qrThread.IsAlive)
        {
            _qrThread.Abort();
        }
        if (_poseThread != null && _poseThread.IsAlive)
        {
            _poseThread.Abort();
        }
    }
}
