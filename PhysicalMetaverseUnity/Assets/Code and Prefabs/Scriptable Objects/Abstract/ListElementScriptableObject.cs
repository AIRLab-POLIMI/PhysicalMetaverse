
using UnityEngine;

public abstract class ListElementScriptableObject<T> : CallbackScriptableObject where T : ListElementScriptableObject<T>
{
    // these are Scriptable Objects that are part of a ListSO. 
    // they include:
    //  - the list reference
    //  - methods for being inserted in that List, and for being removed from it (on creation/destroy)

    [SerializeField] private BelongingListSO<T> belongingList;

    #region SETUP

    public override void OnMonoBehaviourAwake()
    {
        base.OnMonoBehaviourAwake();
        belongingList.Add(this);
    }
    public void OnDisable() => belongingList.Remove(this);
    public void OnDestroy() => belongingList.Remove(this);

    #endregion
}
