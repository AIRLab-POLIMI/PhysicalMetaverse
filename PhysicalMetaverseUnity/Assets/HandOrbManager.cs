using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandOrbManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
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
