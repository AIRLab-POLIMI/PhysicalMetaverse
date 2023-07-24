
using UnityEngine;

public abstract class DoubleValueSO<T> : CallbackScriptableObject
{
    [SerializeField] private T startValue1;

    [SerializeField] private T startValue2;

    public T runtimeValue1;

    public T runtimeValue2;

    public bool initOutside;

    public override void OnMonoBehaviourStart()
    {
        if (!initOutside)
        {
            runtimeValue1 = startValue1;
            runtimeValue2 = startValue2;
        }

    }
}
