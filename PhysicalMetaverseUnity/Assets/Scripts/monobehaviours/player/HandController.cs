
using UnityEngine;
using UnityEngine.InputSystem;


public class HandController : MonoBehaviour
{
    public Animator handAnimator;
    
    public InputActionProperty pinchAnimationAction;
    public InputActionProperty grabAnimationAction;
    
    // Update is called once per frame
    void Update()
    {
        // get float value from pinch animation action
        float pinchValue = pinchAnimationAction.action.ReadValue<float>();
        // assign pinch value to animator parameter "Trigger"
        handAnimator.SetFloat("Trigger", pinchValue);
        
        // get float value from grab animation action
        float grabValue = grabAnimationAction.action.ReadValue<float>();
        // assign grab value to animator parameter "Grip"
        handAnimator.SetFloat("Grip", grabValue);
    }
}
