using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Just a crappy character controller for the video
/// </summary>
public class PlayerController : MonoBehaviour {
    private CharacterController controller;

    [SerializeField]
    private Vector3 playerVelocity;

    [SerializeField]
    private float _speed = 2f;
    [SerializeField]
    private float _sensitivity = 150f;
    [SerializeField]
    private float _jumpHeight = 100.0f;

    [SerializeField]    
    private bool _grounded = true;
    [SerializeField]    
    private bool _canJump = true;
    [SerializeField]    
    private bool _gravity = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        //lock mouse in game window
        Cursor.lockState = CursorLockMode.Locked;
        //set playerVelocity to zero
        playerVelocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        // Rotate the controller using mouse
        controller.transform.Rotate(0, Input.GetAxis("Mouse X") * Time.deltaTime * _sensitivity, 0);
        // Move the player in direction he's looking
        controller.Move(controller.transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * _speed);
        controller.Move(controller.transform.right * Input.GetAxis("Horizontal") * Time.deltaTime * _speed);

        //if not grounded
        // Apply gravity
        if (!_grounded && _gravity)
            playerVelocity.y += Physics.gravity.y * Time.deltaTime;

        //if grounded set speed to zero
        if (_grounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        controller.Move(playerVelocity * Time.deltaTime);

        
    }

    void Update()
    {
        //space to jump
        if (Input.GetKeyDown(KeyCode.Space) && _canJump && _grounded)
        {
            //print jump
            Debug.Log("Jump");
            _grounded = false;
            playerVelocity.y += _jumpHeight;
        }
    }
    
    
    void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Collision");
        //log name of object
        Debug.Log(collision.gameObject.name);
        //if tag is Ground
        if (collision.gameObject.tag == "Ground")
        {
            //set grounded to true
            _grounded = true;
            //set canJump to true
            _canJump = true;
            playerVelocity.y = 0;
        }
    }
}