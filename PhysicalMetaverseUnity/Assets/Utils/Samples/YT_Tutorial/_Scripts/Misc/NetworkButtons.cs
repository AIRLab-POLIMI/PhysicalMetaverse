using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
//using textmesh input
using TMPro;

public class NetworkButtons : MonoBehaviour {
    //ip prompt
    private string _ip;
    //tmp inputfield
    public TMP_InputField _clientInput;
    public TMP_InputField _hostInput;
    public bool _skipInput = false;
    public bool _isHost = false;

    private void Start(){
        //set ip to value stored by unity transport
        _ip = GetComponent<UnityTransport>().ConnectionData.Address;
    }
    private void OnGUI() {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
            if (GUILayout.Button("Host") || (_isHost && _skipInput)) {
                if (_skipInput){
                    NetworkManager.Singleton.StartHost();
                }
                else{
                    //popup ip prompt
                    _hostInput.gameObject.SetActive(true);
                    //set text to ip
                    _hostInput.text = _ip;
                    //NetworkManager.Singleton.StartHost();
                }
            }
            if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
            if (GUILayout.Button("Client") || (!_isHost && _skipInput)) {
                if (_skipInput){
                    NetworkManager.Singleton.StartClient();
                }
                else{
                    //popup ip prompt
                    _clientInput.gameObject.SetActive(true);
                    //set text to ip
                    _clientInput.text = _ip;
                    //NetworkManager.Singleton.StartClient();
                }
                //unlock cursor
                Cursor.lockState = CursorLockMode.None;
            }
        }

        GUILayout.EndArea();
    }

    public void ConnectClient(){
        GetComponent<UnityTransport>().ConnectionData.Address = _clientInput.text;
        //disable ip prompt
        _clientInput.gameObject.SetActive(false);
        NetworkManager.Singleton.StartClient();
    }

    public void StartHost(){
        GetComponent<UnityTransport>().ConnectionData.Address = _hostInput.text;
        //disable ip prompt
        _hostInput.gameObject.SetActive(false);
        NetworkManager.Singleton.StartHost();
    }

    // private void Awake() {
    //     GetComponent<UnityTransport>().SetDebugSimulatorParameters(
    //         packetDelay: 120,
    //         packetJitter: 5,
    //         dropRate: 3);
    // }
}