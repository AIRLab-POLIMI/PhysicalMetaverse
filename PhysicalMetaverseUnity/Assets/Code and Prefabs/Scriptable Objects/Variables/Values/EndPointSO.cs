using System.Net;
using UnityEngine;

[CreateAssetMenu(fileName = "EndPoint SO", menuName = "Scriptable Objects/Variables/Values/Endpoint")]

public class EndPointSO : ScriptableObject
{
    [SerializeField] private string stringIP;
    [SerializeField] private int port;
    [SerializeField] private string uiName;

    private IPEndPoint _endPoint;
    private IPAddress _ip;

    public string UINane => uiName;

    public IPEndPoint EndPoint
    {
        get
        {
            if (_endPoint == null)
                _endPoint = UdpMessenger.GetIPEndPoint(IP, Port);
            return _endPoint;
        }
    }

    public IPAddress IP
    {
        get
        {
            if (_ip == null)
                _ip = System.Net.IPAddress.Parse(stringIP);
            return _ip;
        }
    }

    public int Port => port;
}