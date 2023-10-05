
using System;
using UnityEngine;


public class ColliderController : MonoBehaviour
{
    // methods that allows another script to subscribe to its OnTriggerEnter and OnTriggerExit events
    private Action<Collider> _onEnter;
    private Action<Collider> _onExit;

    // methods to subscribe and unsubscribe to the events
    public void SubscribeOnEnter(Action<Collider> action) => _onEnter += action;
    public void UnsubscribeOnEnter(Action<Collider> action) => _onEnter -= action;
    public void SubscribeOnExit(Action<Collider> action) => _onExit += action;
    public void UnsubscribeOnExit(Action<Collider> action) => _onExit -= action;
    
    // when the collider is triggered by another collider
    private void OnTriggerEnter(Collider other)
    {
        // debug
        // Debug.Log("OnTriggerEnter");
        
        // invoke the _onEnter event
        _onEnter?.Invoke(other);
    }
    
    // when the collider is exited by another collider
    private void OnTriggerExit(Collider other)
    {
        // debug
        // Debug.Log("OnTriggerExit");
        
        // invoke the OnExit event
        _onExit?.Invoke(other);
    }
}
