using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEvents
{
    public abstract class BaseGameEventSO : ScriptableObject
    {
        [SerializeField] [TextArea] protected string info;
    }
}
