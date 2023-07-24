using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;

public class SetupManager : Monosingleton<SetupManager>
{
    [SerializeField] private GameObject _loadingText;
    [SerializeField] private GameObject _setupEnvironmentText;
    [SerializeField] private GameObject _setupNetworkText;
    public void Start()
    {
        _loadingText.GetComponent<LoadingText>().StartCycle();
    }

    public void DeleteEnvironmentSetupText()
    {
        _setupEnvironmentText.SetActive(false);
    }

    public void ActivateNetworkSetupText()
    {
        _setupNetworkText.SetActive(true);
    }

}
