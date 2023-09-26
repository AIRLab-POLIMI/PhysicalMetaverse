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
    public bool _tracked = false;
    public OdometryManager _odometryManager;
    //transform orientationtransform
    private Transform _orientationTransform;
    public Transform _untrackedParent;
    public Transform _stationManager;
    //station color
    public Color _stationColor = Color.green;
    // Start is called before the first frame update
    void Start()
    {
        _thisColor = gameObject.GetComponent<Renderer>().material.color;
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
        Odometry();
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

    public int _untrackedAngle = 0;
    public bool _prevTracked = false;
    private float _resetAngle = 0;
    [Range(0.001f, 0.02f)]
    public float _fadeSpeed = 0.1f;
    //gameObject.GetComponent<Renderer>().material.color
    private Color _thisColor;
    public bool _lerp = false;
    private void Odometry()
    {
        if (_tracked)
        {
            transform.parent = _stationManager;
            //only rotate the station on its own axis, movement is done by StationManager looking at the camera
            //locally rotate according to orientation transform y angle
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x , _orientationTransform.rotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
            float positiveOrientation = _orientationTransform.transform.eulerAngles.y;
            if (positiveOrientation < 0)
                positiveOrientation += 360;
            _untrackedAngle = (int)positiveOrientation;
            _prevTracked = true;
            _untrackedParent.transform.localRotation = Quaternion.Euler(0, 0, 0);
            if(!_prevTracked)
                //reset position to 0
                transform.position = Vector3.zero;
                //set alpha to 1
                gameObject.GetComponent<Renderer>().material.color = new Color(gameObject.GetComponent<Renderer>().material.color.r, gameObject.GetComponent<Renderer>().material.color.g, gameObject.GetComponent<Renderer>().material.color.b, 1);

        }
        else
        {
            transform.parent = _untrackedParent;
            //set _untrackedStations rotation to _untrackedAngles[i] + _sun.transform.eulerAngles.y lerp
            //_untrackedStations[i].transform.localRotation = Quaternion.Lerp(_untrackedStations[i].transform.localRotation, Quaternion.Euler(0, -(_untrackedAngles[i] - _sun.transform.eulerAngles.y), 0), _speed);
            //set _untrackedStations rotation to _untrackedAngles[i] + _sun.transform.eulerAngles.y
            //_untrackedStations[i].transform.localRotation = Quaternion.Euler(0, -(_untrackedAngles[i] - _sun.transform.eulerAngles.y), 0);
            //diff angle -(_untrackedAngles[i] - _sun.transform.eulerAngles.y)
            //get positive value of _orientationTransform
            float positiveOrientation = _orientationTransform.transform.eulerAngles.y;
            if (positiveOrientation < 0)
                positiveOrientation += 360;
            float diffAngle = -(_untrackedAngle - positiveOrientation);
            diffAngle = diffAngle % 360;
            //avoid lerp skip
            if (diffAngle > 180)
                diffAngle -= 360;
            else if (diffAngle < -180)
                diffAngle += 360;
            //if tracked changed dont lerp
            if (!_prevTracked)
            {
                if(_lerp)
                    _untrackedParent.transform.localRotation = Quaternion.Lerp(_untrackedParent.transform.localRotation, Quaternion.Euler(0, diffAngle, 0), 0.1f);
                else
                    _untrackedParent.transform.localRotation = Quaternion.Euler(0, diffAngle, 0);
            }
                //no lerp
                //_untrackedParent.transform.localRotation = Quaternion.Euler(0, diffAngle, 0);
            
            _prevTracked = false;

            //check if something is true in odometry manager
            if (_odometryManager._forward)
            {
                transform.position -= Vector3.forward * _odometryManager._speed * Time.deltaTime;
                //fade material alpha a bit, no lerp
                FadeOut();
            }
            if (_odometryManager._backward)
            {
                transform.position += Vector3.forward * _odometryManager._speed * Time.deltaTime;
                FadeOut();
            }
            if (_odometryManager._left)
            {
                transform.position += Vector3.right * _odometryManager._speed * Time.deltaTime;
                FadeOut();
            }
            if (_odometryManager._right)
            {
                transform.position -= Vector3.right * _odometryManager._speed * Time.deltaTime;
                FadeOut();
            }
        }
    }

    public void FadeOut(){
        if (gameObject.GetComponent<Renderer>().material.color.a > 0)
            gameObject.GetComponent<Renderer>().material.color = new Color(gameObject.GetComponent<Renderer>().material.color.r, gameObject.GetComponent<Renderer>().material.color.g, gameObject.GetComponent<Renderer>().material.color.b, gameObject.GetComponent<Renderer>().material.color.a - _fadeSpeed);
        else
        {
            gameObject.GetComponent<Renderer>().material.color = new Color(gameObject.GetComponent<Renderer>().material.color.r, gameObject.GetComponent<Renderer>().material.color.g, gameObject.GetComponent<Renderer>().material.color.b, 0);
            gameObject.SetActive(false);
        }
    }
    public void SetTracked(bool tracked)
    {
        _tracked = tracked;
    }
    public void SetUntrackedParent(Transform untrackedParent)
    {
        _untrackedParent = untrackedParent;
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
