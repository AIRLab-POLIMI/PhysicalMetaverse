using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RobotNetworkCleaner : NetworkBehaviour
{
    public override void OnNetworkSpawn() {
        if (!IsOwner){
            Destroy(transform.GetComponent<RobotController>());
            //get camera in children of this gameobject
            Destroy(GetComponentInChildren<Camera>());
        }
    }
}
