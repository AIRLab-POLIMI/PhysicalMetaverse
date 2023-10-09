
using System;
using UnityEngine;


public class ColliderController : MonoBehaviour
{
    private int _updatesOut;
    private string _otherTag;
    
    public bool SphereInRange { get; private set; }

    public void Init(string otherTag)
    {
        _otherTag = otherTag;
        
        SphereInRange = false;
        _updatesOut = 0;
    }

    private bool CheckColliderTag(Collider other)
    {
        // check if the collider has the tag otherTag
        return other.CompareTag(_otherTag);
    }
    
    private void OnTriggerStay(Collider other)
    {
        // try to see if the other is the SphereController. If it is, set SphereInRange to true
        if (CheckColliderTag(other))
        {
            SphereInRange = true;
            _updatesOut = 0;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        // debug the name of the object that exited the trigger
        //Debug.Log(other.gameObject.name);
        
        // try to see if the other is the SphereController. If it is, set SphereInRange to true
        if (CheckColliderTag(other))
        {
            // debug
            //Debug.Log("Sphere out of range");
            
            SphereInRange = false;
        }
    }

    private void FixedUpdate()
    {
        _updatesOut++;
        
        if (_updatesOut >= 2)
        {
            SphereInRange = false;
        }
    }


    // methods that allows another script to subscribe to its OnTriggerEnter and OnTriggerExit events
    // private Action<Collider> _onEnter;
    // private Action<Collider> _onExit;
    //
    // // methods to subscribe and unsubscribe to the events
    // public void SubscribeOnEnter(Action<Collider> action) => _onEnter += action;
    // public void UnsubscribeOnEnter(Action<Collider> action) => _onEnter -= action;
    // public void SubscribeOnExit(Action<Collider> action) => _onExit += action;
    // public void UnsubscribeOnExit(Action<Collider> action) => _onExit -= action;
    //
    // // when the collider is triggered by another collider
    // private void OnTriggerEnter(Collider other)
    // {
    //     // debug
    //     // Debug.Log("OnTriggerEnter");
    //     
    //     // invoke the _onEnter event
    //     _onEnter?.Invoke(other);
    // }
    // 
    // // when the collider is exited by another collider
    // private void OnTriggerExit(Collider other)
    // {
    //     // debug
    //     // Debug.Log("OnTriggerExit");
    //     
    //     // invoke the OnExit event
    //     _onExit?.Invoke(other);
    // }
}
