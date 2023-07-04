using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        //lock mouse in game window
        Cursor.lockState = CursorLockMode.Locked;

    }

    void Update()
    {
        // Rotate the controller using mouse
        controller.transform.Rotate(0, Input.GetAxis("Mouse X") * Time.deltaTime * 100f, 0);
        // Move the player in direction he's looking
        controller.Move(controller.transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * 5f);
        controller.Move(controller.transform.right * Input.GetAxis("Horizontal") * Time.deltaTime * 5f);

        // Apply gravity
        playerVelocity.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}
