using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleStationManager : MonoBehaviour
{
    //enum IDLE, INTERACTABLE, ANIMATED, ACTIVATED
    public enum StationState
    {
        IDLE,
        INTERACTABLE,
        INTERACTED,
        ACTIVATED
    }

    public string _stationIp;
    public StationState _stationState;

    public bool _correctStation = true;
    //transform orientationtransform
    private Transform _orientationTransform;
    //station color
    public Color _stationColor = Color.green;
    // Start is called before the first frame update
    void Start()
    {
        ResetStation();
        if (_correctStation)
        {
            _stationColor = Color.green;
            gameObject.GetComponent<Renderer>().material.color = _stationColor;
        }
        else
        {
            _stationColor = Color.red;
            gameObject.GetComponent<Renderer>().material.color = _stationColor;
        }
        //add station color to list of colors
        _colors.Add(_stationColor);

        //reset udp station

    }

    // Update is called once per frame
    void Update()
    {
        //case
        switch (_stationState)
        {
            case StationState.IDLE:
                //do nothing
                break;
            case StationState.INTERACTABLE:
                CheckCompletion();
                break;
            case StationState.INTERACTED:
                BlinkNonBlocking();
                break;
            case StationState.ACTIVATED:
                //do nothing
                break;
        }
        
    }

    void FixedUpdate()
    {
        //locally rotate according to orientation transform y angle
        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x , _orientationTransform.rotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
    }

    public void EnterStation()
    {   
        Debug.Log("Entered " + gameObject.name);
        //if IDLE
        if (_stationState == StationState.IDLE)
        {
            //set INTERACTABLE
            _stationState = StationState.INTERACTABLE;
            //set color to PINK
            gameObject.GetComponent<Renderer>().material.color = Color.yellow;
        }
    }

    public void ExitStation()
    {
        Debug.Log("Exited " + gameObject.name);
        //if INTERACTABLE
        if (_stationState == StationState.INTERACTABLE)
        {
            //set IDLE
            _stationState = StationState.IDLE;
            //set color to station color
            gameObject.GetComponent<Renderer>().material.color = _stationColor;
        }
    }

    public void SetIp(string ip)
    {
        _stationIp = ip;
    }

    public void SetOrientationTransform(Transform orientationTransform)
    {
        _orientationTransform = orientationTransform;
    }
    private void CheckCompletion()
    {
        //if space is pressed complete station
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CompleteStation();
        }
    }
    
    public void ResetStation()
    {
        string data = "RESET";
        NetworkingManager.Instance.SendString(data, _stationIp);
        _stationState = StationState.IDLE;
    }
    public void CompleteStation()
    {
        //if not already interacted
        if (_stationState != StationState.INTERACTED && _stationState != StationState.ACTIVATED)
        {
            //data is "BLINK" in bytes
            string data = "BLINK";

            //key is (char)195
            NetworkingManager.Instance.SendString(data, _stationIp);
            _stationState = StationState.INTERACTED;
        }
    }

    //private prev time
    private float _prevTime = 0f;
    //private colors gray
    private List<Color> _colors = new List<Color> {Color.gray};
    //public total blinks = 4
    public int _totalBlinks = 4;
    public float _blinkTime = 0.3f;
    //blink amount
    private int _blinkAmount = 0;
    private void BlinkNonBlocking()
    {
        //if time passed is greater than 1 second
        if (Time.time - _prevTime > _blinkTime)
        {
            //set prev time to current time
            _prevTime = Time.time;
            //increment blink amount
            _blinkAmount++;
            //set color to the other color
            gameObject.GetComponent<Renderer>().material.color = _colors[_blinkAmount % 2];
        }
        if (_blinkAmount/2 == _totalBlinks)
        {
            //set ACTIVATED
            _stationState = StationState.ACTIVATED;
            //set color to GREEN
            gameObject.GetComponent<Renderer>().material.color = Color.blue;
        }
    }
}
