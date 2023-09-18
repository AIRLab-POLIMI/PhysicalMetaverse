
using System.Collections.Generic;
using UnityEngine;

public abstract class ListSO<T> : CallbackScriptableObject
{
    [SerializeField] private bool startEmpty = false;

    public List<T> list = new List<T>();

    public int Count => list.Count;

    public override void OnMonoBehaviourAwake()
    {
        if (startEmpty)
            list = new List<T>();
    }

    public virtual void Add(T value)
    {
        if (!list.Contains(value))
            list.Add(value);
    }

    public virtual void Remove(T value)
    {
        if (list.Contains(value))
            list.Remove(value);
    }
}