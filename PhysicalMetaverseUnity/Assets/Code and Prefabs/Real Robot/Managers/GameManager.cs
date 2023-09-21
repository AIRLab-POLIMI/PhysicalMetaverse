using System;
using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.XR;
using UnityEngine.InputSystem;

public class GameManager : Monosingleton<GameManager>
{

    [SerializeField] private InputActionReference _InputActionReferenceSceneRestart;

    private GameObject _setup;
    private GameObject _environment;
    public void Start()
    {
        //enable vsync
        QualitySettings.vSyncCount = 1;
        
        _setup = GameObject.FindGameObjectWithTag("Setup");
        _environment = GameObject.FindGameObjectWithTag("Environment");
        
        _InputActionReferenceSceneRestart.action.performed += _ => RestartScene();
        BumpManager.Instance.Setup();
        LidarManager.Instance.Setup();
        //PoseManager.Instance.Setup();
        
        _setup.GetComponent<SetupManager>().DeleteEnvironmentSetupText();
        _setup.GetComponent<SetupManager>().ActivateNetworkSetupText();
        
        NetworkingManager.Instance.Setup();
    }

    public void RestartScene()
    {
        NetworkingManager.Instance.CloseConnections();
        SceneManager.LoadScene(0);
    }
}
