
using UnityEngine;

public abstract class ValueSO<T> : CallbackScriptableObject
{
    [SerializeField] private T startValue;

    public T runtimeValue;

    public bool initOutside;

    public override void OnMonoBehaviourStart()
    {
        if (!initOutside)
            runtimeValue = startValue;
    }
}
