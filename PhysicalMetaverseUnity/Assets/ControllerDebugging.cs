using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerDebugging : MonoBehaviour
{
    Gamepad gamepad;
    //input settings scriptable object
    public InputSettings inputSettings;

    //set global input to input settings
    void Awake()
    {
        //set global input to input settings
        InputSystem.settings = inputSettings;
    }

    void Start()
    {
        //get gamepad
        gamepad = Gamepad.current;
    }

    void Update()
    {
        if (gamepad == null)
        {
            Debug.LogWarning("Logitech F710 not detected or not supported.");
            return;
        }

        //print all the buttons that were pressed
        foreach (InputControl control in gamepad.allControls)
        {
            if (control.IsPressed())
            {
                //log value
                Debug.Log(control.displayName + " " + control.ReadValueAsObject());
            }
        }
    }
}
