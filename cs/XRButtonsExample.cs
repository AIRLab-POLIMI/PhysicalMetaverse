using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
public class XRButtonsExample : MonoBehaviour
{
    public ActionBasedController _actionBasedController;
    public bool _ENABLELOG = true;

    // Start is called before the first frame update
    void Start()
    {
        //store gameObject.GetComponent<ActionBasedController>()
        _actionBasedController = gameObject.GetComponent<ActionBasedController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(_ENABLELOG)
        {
            //for each button in the controller log its value
            Debug.Log("Grip value " + gameObject.GetComponent<ActionBasedController>().selectActionValue.action.ReadValue<float>());
            Debug.Log("Trigger value " + gameObject.GetComponent<ActionBasedController>().activateActionValue.action.ReadValue<float>());
        }
    }
}
