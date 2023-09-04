
using GameEvents;
using UnityEngine;

public abstract class KeyMessageResponder<T> : KeyValueGameEventListener
{
    [Space]
    
    [SerializeField] protected ByteSO dofKey;

    protected abstract void MessageResponse(T val);
}