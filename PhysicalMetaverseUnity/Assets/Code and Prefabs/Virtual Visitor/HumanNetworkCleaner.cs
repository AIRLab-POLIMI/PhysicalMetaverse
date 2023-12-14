using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HumanNetworkCleaner : NetworkBehaviour
{
    public override void OnNetworkSpawn() {
        if (!IsOwner){
            Destroy(transform.GetComponent<PlayerController>());
            //get camera in children of this gameobject
            Destroy(GetComponentInChildren<Camera>());
            //destroy listener
            Destroy(GetComponentInChildren<AudioListener>());
            //destroy PlayerControllerVR
            Destroy(GetComponentInChildren<PlayerControllerVR>());
            //destroy PlayerControllerV2
            Destroy(GetComponentInChildren<PoseControllerV2>());
        }
    }
}
