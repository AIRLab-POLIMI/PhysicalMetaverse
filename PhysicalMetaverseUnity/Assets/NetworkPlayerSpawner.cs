using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerSpawner : NetworkBehaviour {
    [SerializeField] private GameObject playerPrefabA; //add prefab in inspector
    [SerializeField] private GameObject playerPrefabB; //add prefab in inspector
 
    
    [ServerRpc(RequireOwnership=false)] //server owns this object but client can request a spawn
    public void SpawnPlayerServerRpc(ulong clientId,int prefabId) {
        GameObject newPlayer;
        if (prefabId==0)
            newPlayer=(GameObject)Instantiate(playerPrefabA);
        else
            newPlayer=(GameObject)Instantiate(playerPrefabB);
        NetworkObject netObj=newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId,true);
    }

    public override void OnNetworkSpawn() {
        if (!IsOwner) return; //only the owner of this object can spawn a player
        //if i am the host
        if (IsHost) {
            SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId,0);
        }
        else {
            SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId,1);
        }
    }

    
}