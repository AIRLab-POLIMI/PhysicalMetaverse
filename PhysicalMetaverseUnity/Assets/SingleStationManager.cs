using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleStationManager : MonoBehaviour
{
    public bool _tracked = false;
    public OdometryManager _odometryManager;
    //transform orientationtransform
    private Transform _orientationTransform;
    public Transform _untrackedParent;
    public Transform _stationManager;
    //private lidar manager
    private LidarManager _lidarManager;
    // Start is called before the first frame update
    void Start()
    {
        //get lidar manager mono instance
        _lidarManager = LidarManager.Instance;

        //disable mesh
        gameObject.GetComponent<MeshRenderer>().enabled = false;

        _interactionGameObject.transform.position = new Vector3(0,-100,0);
        _interactionGameObject.GetComponent<StationController>().Init();

    }

    public int _stationId = -1;
    // Update is called once per frame
    void Update()
    {
        Odometry();
        
    }

    public float _petalsAlpha = 1f;
    void FixedUpdate(){
        if(_stationId < 0){
            //parse station id from last char pf name
            _stationId = int.Parse(gameObject.name.Substring(gameObject.name.Length - 1));
            //if station id even right true, if odd right false
            _interactionGameObject.GetComponent<StationController>().isRight = _stationId % 2 == 0 ? true : false;
        }
        //if tracked set tag to Station if not set to InvalidStation
        if (_tracked)
        {
            gameObject.tag = "Station";
        }
        else
        {
            gameObject.tag = "InvalidStation";
        }

        //if station invalidated fade _interactionGameObject mesh, if not invalidated restore full alpha
        if (_stationInvalidated)
        {
            if (_petalsAlpha > 0)
                _petalsAlpha -= _fadeSpeed;
            else
            {
                ////_interactionGameObject.GetComponent<Renderer>().material.color = new Color(_interactionGameObject.GetComponent<Renderer>().material.color.r, _interactionGameObject.GetComponent<Renderer>().material.color.g, _interactionGameObject.GetComponent<Renderer>().material.color.b, 0);
                //set this station to false
                ////gameObject.SetActive(false);
                _petalsAlpha = 0;
                Hide();
            }
        }
        else
        {
            //enable mesh
            ////interactionGameObject.GetComponent<MeshRenderer>().enabled = true;
            //set alpha to 1
            _petalsAlpha = 1f;
            Show();
        }

        if (_petalsAlpha == 0)
        {
            Hide(); //TODO FIX THIS IS NOT WORKING
        }
    }

    public int _untrackedAngle = 0;
    public bool _prevTracked = false;
    private float _resetAngle = 0;
    [Range(0.001f, 0.02f)]
    public float _fadeSpeed = 0.1f;
    public bool _lerp = false;
    public bool _lidarTracking = false;
    public GameObject _interactionGameObject;
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
            if(!_lidarTracking){
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
            else{
                _lidarManager.LidarTrack(this.gameObject);
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
        if(_tracked)
            _stationInvalidated = false;
    }
    public void SetUntrackedParent(Transform untrackedParent)
    {
        _untrackedParent = untrackedParent;
    }


    public void SetOrientationTransform(Transform orientationTransform)
    {
        _orientationTransform = orientationTransform;
    }

    public void CompleteStation(){
        //_interactionGameObject.GetComponent<StationInteractionManager>().CompleteStation();
    }

    /*public void ExitStation(){
        //_interactionGameObject.GetComponent<StationInteractionManager>().ExitStation();
    }*/

    public void SetIp(string ip){
        //_interactionGameObject.GetComponent<StationInteractionManager>().SetIp(ip);
        _interactionGameObject.GetComponent<StationController>().SetIp(ip);
    }

    /*public void EnterStation(){
        //_interactionGameObject.GetComponent<StationInteractionManager>().EnterStation();

    }*/

    public void Show(){
        _interactionGameObject.GetComponent<StationController>().Show();
    }

    public void Hide(){
        _interactionGameObject.GetComponent<StationController>().Hide();
    }

    public bool _stationInvalidated = false;
    public void InvalidateStation(){
        _stationInvalidated = true;
    }
}
