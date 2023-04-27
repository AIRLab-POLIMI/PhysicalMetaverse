
using UnityEngine;

public abstract class CallbackScriptableObject : ScriptableObject, IPrioritized
{
    [SerializeField] private PriorityLevel priorityLevel = PriorityLevel.Low;

    public PriorityLevel Priority => priorityLevel;
    
    public virtual void OnMonoBehaviourAwake(){}
    
    public virtual void OnMonoBehaviourEnable(){}
    
    public virtual void OnMonoBehaviourStart(){}
    
    public virtual void OnMonoBehaviourDisable(){}
    
}
