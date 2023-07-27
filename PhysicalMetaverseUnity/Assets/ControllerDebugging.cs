using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerDebugging : MonoBehaviour
{
    private Gamepad gamepad;

    private void Awake()
    {
        // Get the default gamepad (Logitech F710 should be recognized as a gamepad)
        gamepad = Gamepad.current;
    }

    private void Update()
    {
        if (gamepad == null)
        {
            Debug.LogWarning("Logitech F710 not detected or not supported.");
            return;
        }

        // Check individual buttons and log their state
        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            Debug.Log("Button South (A) pressed!");
        }

        if (gamepad.buttonNorth.wasPressedThisFrame)
        {
            Debug.Log("Button North (Y) pressed!");
        }

        // Add more buttons as needed (e.g., gamepad.buttonWest, gamepad.buttonEast, etc.)

        // Example for checking axis values (e.g., left stick)
        Vector2 leftStick = gamepad.leftStick.ReadValue();
        Debug.Log("Left Stick: " + leftStick);
    }
}
