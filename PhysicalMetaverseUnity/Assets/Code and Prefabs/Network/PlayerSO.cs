///scriptable object containing gameobject list
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSO", menuName = "ScriptableObjects/PlayerSO")]
public class PlayerSO : ScriptableObject
{
    public List<GameObject> _playerList;
}
