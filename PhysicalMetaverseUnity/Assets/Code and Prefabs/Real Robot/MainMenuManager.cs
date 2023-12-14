using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    [SerializeField] private IntSO EnvironmentMode;

    [SerializeField] private IntSO PoseMode;

    public void Start()
    {
    }

    public void SetEnvironmentMode(int mode)
    {
        EnvironmentMode.runtimeValue = mode;
    }
    
    public void SetPoseMode(int mode)
    {
        PoseMode.runtimeValue = mode;
    }

    public void LoadGame()
    {
        SceneManager.LoadScene(1);
    }
    
}
