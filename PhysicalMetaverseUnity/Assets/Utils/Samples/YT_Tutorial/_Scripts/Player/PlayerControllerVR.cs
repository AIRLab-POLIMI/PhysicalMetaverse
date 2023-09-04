using Unity.Netcode;
using UnityEngine;

/// <summary>
/// VR version of the demo player controller, best effort to have some working controls, almost made Fede throw up after 5 minutes of testing
/// </summary>
public class PlayerControllerVR : MonoBehaviour {
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
    public GameObject _camera;
    public GameObject _myCamera;

    //rrawimage
    public GameObject _rawImage;
    void Start()
    {
        controller = GetComponent<CharacterController>();
        //lock mouse in game window
        Cursor.lockState = CursorLockMode.Locked;
        //set playerVelocity to zero
        playerVelocity = Vector3.zero;
        //camera = find TrackCamera
        _camera = GameObject.Find("TrackCamera");
        //find gameobject named XR Origin and
    }

    void FixedUpdate()
    {
        // rotate controller to direction of camera plus rotationx
        //_myCamera.transform.rotation = Quaternion.Euler(_camera.transform.eulerAngles.x, _myCamera.transform.rotation.y, 0);
        // controller.transform.rotation = Quaternion.Euler(controller.transform.eulerAngles.x, _camera.transform.rotation.y, controller.transform.eulerAngles.z);
        //make _camera face controller direction
        //_camera.transform.rotation = Quaternion.Euler(_camera.transform.eulerAngles.x, controller.transform.rotation.y, 0);
        //set camera y
        //_myCamera.transform.rotation = Quaternion.Euler(_camera.transform.eulerAngles.x, _myCamera.transform.eulerAngles.y + _rotationX, _myCamera.transform.eulerAngles.z);

        // Move the player in direction he's looking
        //controller.Move(controller.transform.right * Input.GetAxis("Horizontal") * Time.deltaTime * _speed);
        //get the other analog axis to change rotation
        //change angle y of controller transform
        //if input more than 0.01 abs
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f)
            controller.transform.rotation = Quaternion.Euler(controller.transform.eulerAngles.x, controller.transform.eulerAngles.y + Input.GetAxis("Horizontal") * Time.deltaTime * _sensitivity, controller.transform.eulerAngles.z);

        //print input
        //Debug.Log(Input.GetAxis("Horizontal"));
        controller.Move(controller.transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * _speed);
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
        //rotate camera as trackcamera locally
        _myCamera.transform.localRotation = _camera.transform.localRotation;
        //space to jump
        if (Input.GetKeyDown(KeyCode.Space) && _canJump && _grounded)
        {
            //print jump
            Debug.Log("Jump");
            _grounded = false;
            playerVelocity.y += _jumpHeight;
        }
        //VR A button to jump
        if (Input.GetKeyDown(KeyCode.JoystickButton0) && _canJump && _grounded)
        {
            //print jump
            Debug.Log("Jump");
            _grounded = false;
            playerVelocity.y += _jumpHeight;
        }
        //vr left trigger to jump
        if (Input.GetKeyDown(KeyCode.JoystickButton14) && _canJump && _grounded)
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