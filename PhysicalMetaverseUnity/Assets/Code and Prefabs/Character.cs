using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    //public gameobject camera
    public GameObject _other;

    public GameObject _robotCollider;

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

    //when keyboard q is pressed
    /*void OnGUI()
    {
        if (Event.current.Equals(Event.KeyboardEvent("q")))
        {
            //disable this Gameobject's camera
            this.GetComponentInChildren<Camera>().enabled = false;
            //enable other camera
            _other.GetComponentInChildren<Camera>().enabled = true;
            //enable other character controller
            _other.GetComponent<CharacterController>().enabled = true;
            //enable other character script
            _other.GetComponent<Character>().enabled = true;
            //disable character controller
            this.GetComponent<CharacterController>().enabled = false;
            //disable this script
            this.GetComponent<Character>().enabled = false;
            //toggle robot collider if not null
            if (_robotCollider != null)
            {
                _robotCollider.SetActive(!_robotCollider.activeSelf);
            }
        }
    }*/
}
