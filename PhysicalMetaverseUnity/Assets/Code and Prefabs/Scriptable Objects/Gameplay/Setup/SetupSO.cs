
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Setup SO", menuName = "Scriptable Objects/Gameplay/Setup/Setup")]

public class SetupSO : ScriptableObject
{
    [Header("Remote Devices")]
        [SerializeField] private EndpointUsage jetsonEndpointUsage;

    #region GETTERS/SETTERS

        public EndpointUsage JetsonEndpointUsage => jetsonEndpointUsage;

    #endregion
}

[System.Serializable]

public class EndpointUsage
{
    [SerializeField] private EndPointSO endpoint;
    public bool usage;

    public EndPointSO Endpoint => endpoint;
}
