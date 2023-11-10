using System;
using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.XR;
using UnityEngine.InputSystem;

public class GameManager : Monosingleton<GameManager>
{

    

    private GameObject _setup;
    private GameObject _environment;
    
    [Space]
    
    [Header("TIME")]
    
    [Range(0, 1200)] [SerializeField] private int _gameDurationSeconds;
    [SerializeField] private int _remainingTime;
    [SerializeField] private float _normalisedElapsedTime;    
    private bool _gameOver;
    [Space]
    
    [Header("INTERFACE")]
    [SerializeField] private  GameObject _losePanel;
    [SerializeField] private  GameObject _winPanel;
    [SerializeField] private string _winMessage = "G:1";    
    [Space]
    
    [Header("SCORE")]
    [SerializeField] private int _score;
    [SerializeField] private int _scoreToWin;    
    [Space]
    
    [Header("RESET BUTTON")]
    [SerializeField] private InputActionReference _InputActionReferenceSceneRestart;
    [Space]
    
    [Header("ACTIONS")]
    [SerializeField] private  bool _resetTime = true;

    public void Start()
    {
        //enable vsync
        QualitySettings.vSyncCount = 1;
        
        _setup = GameObject.FindGameObjectWithTag("Setup");
        _environment = GameObject.FindGameObjectWithTag("Environment");
        
        //_InputActionReferenceSceneRestart.action.performed += _ => RestartScene();
        BumpManager.Instance.Setup();
        LidarManager.Instance.Setup();
        //PoseManager.Instance.Setup();
        
        _setup.GetComponent<SetupManager>().DeleteEnvironmentSetupText();
        _setup.GetComponent<SetupManager>().ActivateNetworkSetupText();
        
        NetworkingManager.Instance.Setup();

        _normalisedElapsedTime = 0;

        //set quality to high
        QualitySettings.SetQualityLevel(5);
        //resolution aspect ratio to 9:16
        Screen.SetResolution(1080, 1920, false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_gameOver)
            UpdateTime();
        CheckResetTime();
    }

    //serialize time scale
    [SerializeField] private float _timeScale = -20f;
    void UpdateTime()
    {
        _normalisedElapsedTime += Time.deltaTime/_gameDurationSeconds * _timeScale;
        //_remainingTime round
        _remainingTime = _gameDurationSeconds - (int) Math.Round(_normalisedElapsedTime * _gameDurationSeconds);
        AmbientManager.Instance.UpdateLight(_normalisedElapsedTime);
        if(_normalisedElapsedTime > 1){
            //enable _losePanel
            LoseTheGame();
        }
    }    

    public void SetTimeScale(float timeScale)
    {
        _timeScale = timeScale;
    }

    //get timescale
    public float GetTimeScale()
    {
        return _timeScale;
    }

    private void WinTheGame(){
        _gameOver = true;
        _winPanel.SetActive(true);
        NetworkingManager.Instance.SendString(_winMessage, NetworkingManager.Instance.GetPythonGamemanagerIp());
    }

    private void LoseTheGame(){
        _gameOver = true;
        _losePanel.SetActive(true);
    }

    private void CheckResetTime(){
        if(_resetTime){
            _resetTime = false;
            ResetTime();
        }
    }
    public void ResetTime(){
        //send reset time to python
        string data = "S:" + _gameDurationSeconds.ToString();
        NetworkingManager.Instance.SendString(data, NetworkingManager.Instance.GetPythonGamemanagerIp());
        //gamemanager teststationsmanager set _normalisedElapsedTime 0 gameDurationSeconds time string
        _normalisedElapsedTime = 0;

        //log resetted time
        Debug.Log("Resetted time to " + _gameDurationSeconds);
    }

    public void RestartScene()
    {
        NetworkingManager.Instance.CloseConnections();
        SceneManager.LoadScene(0);
    }

    public void UpdateScore(int score)
    {
        _score = score;
        if (_score >= _scoreToWin)
        {
            WinTheGame();
        }
    }

    public void SubtractTime(int time)
    {
        //normalise time and add it to _normalisedElapsedTime
        _normalisedElapsedTime += (float) time / _gameDurationSeconds;
    }

    //get normalized elapsed
    public float GetNormalizedElapsedTime()
    {
        return _normalisedElapsedTime;
    }

    //set normalized time
    public void SetNormalizedElapsedTime(float normalizedElapsedTime)
    {
        _normalisedElapsedTime = normalizedElapsedTime;
    }

    //get game duration
    public int GetGameDuration()
    {
        return _gameDurationSeconds;
    }

    //fullscreen variable
    [SerializeField] private bool _fullscreen = false;
    //set fullscreen
    public void ToggleFullscreen()
    {
        _fullscreen = !_fullscreen;
        Screen.fullScreen = _fullscreen;
        //enable log
        Debug.unityLogger.logEnabled = true;
        //log
        Debug.Log("Fullscreen: " + _fullscreen);
        //disable log
        Debug.unityLogger.logEnabled = false;
    }
}
