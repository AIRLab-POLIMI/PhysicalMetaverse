using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MirrorGameManager : MonoBehaviour
{
    private GameManager _gameManager;
    //serialize sprite renderer
    [SerializeField] private SpriteRenderer _blackPanel;
    //serialize _robotPoseController
    [SerializeField] private RobotPoseContoller _robotPoseController;
    //serialize normalized end time
    [SerializeField] private float _normalizedEndTime = 0.8f;
    //serialize _resetTimeSpeed
    [SerializeField] private float _resetTimeSpeed = -80f;
    private float _prevResetTimeSpeed = 0f;
    [SerializeField] private float _resetTimeDuration = 2f;
    //serialize _restoreTime
    [SerializeField] private float _restoreTime = 3f;
    //serialize _restoreTimeBeforeEnd
    [SerializeField] private float _restoreTimeBeforeEnd = 3f;
    //gameobject list vizlist
    [SerializeField] private GameObject[] _vizList;
    //[SerializeField] private UnityEvent<byte[]> byteEventResponse;
    //list of events UnityEvent<byte[]>
    [SerializeField] private List<UnityEvent<byte[]>> _byteEventList;
    //enum current viz, Odile, Siid, Evangelion
    public enum VizType{
        ODILE,
        SIID,
        EVANGELION
    }
    //arraylist of viztypes
    [SerializeField] private VizType[] _vizTypeList;
    [SerializeField] private VizType _currentVizType = VizType.ODILE;
    private float _exitTime = 0f;
    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameManager.Instance;
        _gameManager.SetTimeScale(1f);
        _blackPanel.gameObject.SetActive(true);
        //fill _vizTypeList with allvaluers of viztype
        _vizTypeList = (VizType[])System.Enum.GetValues(typeof(VizType));
        //set current viz type to 0
        _currentVizType = _vizTypeList[0];
    }
    [SerializeField] private bool _vizSetted = false;
    private bool _prevPoseDetected = true;
    private bool _fadeBlackPanel = false;
    // Update is called once per frame
    void Update()
    {
        //if not viz setted fire corresponding event with value true
        if(!_vizSetted){
            //fire event
            _byteEventList[(int)_currentVizType].Invoke(new byte[]{1});
            //fire next in list
            _byteEventList[((int)_currentVizType + 1) % _byteEventList.Count].Invoke(new byte[]{0});
            //change _currentVizType to next
            _currentVizType = _vizTypeList[((int)_currentVizType + 1) % _vizTypeList.Length];
            //set viz setted to true
            _vizSetted = true;
        }
        //if _robotPoseController getposedetected is false enable black panel and fade it in, else fade it out
        /*if(!_robotPoseController.GetPoseDetected()){
            _blackPanel.color = new Color(_blackPanel.color.r, _blackPanel.color.g, _blackPanel.color.b, Mathf.Lerp(_blackPanel.color.a, 1f, 0.1f));
        }else{
            _blackPanel.color = new Color(_blackPanel.color.r, _blackPanel.color.g, _blackPanel.color.b, Mathf.Lerp(_blackPanel.color.a, 0f, 0.1f));
        }*/
        
        //if elapsed time more than _normalizedEndTime set timescale to 0
        if(_gameManager.GetNormalizedElapsedTime() > _normalizedEndTime){
            _gameManager.SetTimeScale(0f);
            //restore when person leaves and comes back
            /*if(_robotPoseController.GetPoseDetected() && !_prevPoseDetected){
                //set timescale to -20
                _gameManager.SetTimeScale(_resetTimeSpeed);
                //set normalized time to end
                if(_gameManager.GetNormalizedElapsedTime() > _normalizedEndTime){
                    _gameManager.SetNormalizedElapsedTime(_normalizedEndTime);
                }
            }
            _prevPoseDetected = _robotPoseController.GetPoseDetected();*/
            RestoreTimeAfter(_restoreTime);
        }
        else{
            RestoreTimeAfter(_restoreTimeBeforeEnd);
        }
        //if normalised time is less than 0 set timescale to 1
        if(_gameManager.GetNormalizedElapsedTime() < 0){
            _gameManager.SetTimeScale(0f);
            if(_robotPoseController.GetPoseDetected())
                _gameManager.SetTimeScale(1f);
        }
        
        //if _fadeBlackPanel is true fade black panel in
        if(_gameManager.GetNormalizedElapsedTime() > _normalizedEndTime){
            _blackPanel.color = new Color(_blackPanel.color.r, _blackPanel.color.g, _blackPanel.color.b, Mathf.Lerp(_blackPanel.color.a, 1f, 0.1f));
        }else{
            _blackPanel.color = new Color(_blackPanel.color.r, _blackPanel.color.g, _blackPanel.color.b, Mathf.Lerp(_blackPanel.color.a, 0f, 0.1f));
        }

        //if _prevResetTimeSpeed >= 0 and _resetTimeSpeed < 0 set _vizSetted to false
        if(_prevResetTimeSpeed >= 0 && _gameManager.GetTimeScale() < 0){
            _vizSetted = false;
        }
        
        _prevResetTimeSpeed = _gameManager.GetTimeScale();

    }

    private void RestoreTimeAfter(float restoreTime){
        //restore 3 seconds after person left
        if(_robotPoseController.GetPoseDetected()){
            _exitTime = Time.time;
        }
        if(Time.time - _exitTime > restoreTime){
            //set timescale to -20
            _resetTimeSpeed = -GameManager.Instance.GetGameDuration() / _resetTimeDuration;
            _gameManager.SetTimeScale(_resetTimeSpeed);
            //set normalized time to end
            if(_gameManager.GetNormalizedElapsedTime() > _normalizedEndTime){
                _gameManager.SetNormalizedElapsedTime(_normalizedEndTime);
            }
        }
    }
}
