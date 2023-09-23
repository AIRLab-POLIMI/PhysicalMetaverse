using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandOrbManager : MonoBehaviour
{
    [SerializeField] private InputActionReference _InputActionReferencePressTrigger;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public float _sphereScale = 0.5f;
    void Update()
    {
        float currentTrigVal = _InputActionReferencePressTrigger.action.ReadValue<float>();
        // The trigger is being pressed
        Debug.Log("Orb trigger value " + currentTrigVal);
        //scale
        transform.localScale = new Vector3(_sphereScale-currentTrigVal*_sphereScale, _sphereScale-currentTrigVal*_sphereScale, _sphereScale-currentTrigVal*_sphereScale);
    }
    public void OrbResize(InputAction.CallbackContext context)
    {
        float currentTrigVal = context.action.ReadValue<float>();
        Debug.Log(currentTrigVal);
    }

    //on collision enter with a station call CompleteStation
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Hole"))
        {
            collision.gameObject.transform.parent.gameObject.GetComponent<SingleStationManager>().CompleteStation();
            //log
            Debug.Log("Station completed");
        }
    }
}
