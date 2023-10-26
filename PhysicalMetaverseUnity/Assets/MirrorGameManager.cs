using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    //serialize _restoreTime
    [SerializeField] private float _restoreTime = 3f;
    private float _exitTime = 0f;
    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameManager.Instance;
        _gameManager.SetTimeScale(1f);
        _blackPanel.gameObject.SetActive(true);
    }

    private bool _prevPoseDetected = true;
    private bool _fadeBlackPanel = false;
    // Update is called once per frame
    void Update()
    {
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

            //restore 3 seconds after person left
            if(_robotPoseController.GetPoseDetected()){
                _exitTime = Time.time;
            }
            if(Time.time - _exitTime > _restoreTime){
                //set timescale to -20
                _gameManager.SetTimeScale(_resetTimeSpeed);
                //set normalized time to end
                if(_gameManager.GetNormalizedElapsedTime() > _normalizedEndTime){
                    _gameManager.SetNormalizedElapsedTime(_normalizedEndTime);
                }
            }
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


    }
}
