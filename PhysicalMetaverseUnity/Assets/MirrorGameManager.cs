using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MirrorGameManager : MonoBehaviour
{
    private GameManager _gameManager;
    //serialize sprite renderer
    [SerializeField] private SpriteRenderer _blackPanel;
    private PoseManager _poseManager;
    //_poseReceiver
    private PoseReceiver _poseReceiver;
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

    [SerializeField] private bool _vizSetted;
    //_prevPoseDetected
    private bool _prevPoseDetected = false;
    [SerializeField] private GameState _currentState = GameState.PLAY;

    private AmbientManager _ambientManager;
    //VizType
    public enum VizType
    {
        ODILE,
        SIID,
        EVANGELION
    }
    //_vizTypeList
    private VizType[] _vizTypeList = { VizType.ODILE, VizType.SIID, VizType.EVANGELION };
    //_currentVizType
    private VizType _currentVizType = VizType.ODILE;

    //enum current viz, Odile, Siid, Evangelion
    public enum GameState
    {
        PLAY,
        END
    }

    private float _exitTime = 0f;

    void Start()
    {
        // Your initialization logic
        _poseReceiver = PoseReceiver.Instance;
        _poseManager = PoseManager.Instance;
        _ambientManager = AmbientManager.Instance;
        _gameManager = GameManager.Instance;
        _gameManager.SetTimeScale(1f);
        _blackPanel.gameObject.SetActive(true);
        _vizTypeList = (VizType[])System.Enum.GetValues(typeof(VizType));
        _currentVizType = _vizTypeList[0];

        EnterState(GameState.PLAY);
    }

    void EnterState(GameState state)
    {
        _currentState = state;

        switch (_currentState)
        {
            case GameState.PLAY:
                // Enter logic for play state (if any)
                break;
            case GameState.END:
                // Enter logic for end state
                _gameManager.SetTimeScale(0f);
                break;
        }
    }

    void Update()
    {
        switch (_currentState)
        {
            case GameState.PLAY:
                PlayStateUpdate();
                break;
            case GameState.END:
                EndStateUpdate();
                break;
        }
    }

    void PlayStateUpdate()
    {
        HandlePoseDetection();
        ChangeVisualization();
        HandleTime();
        HandleBlackPanelFade();

        if (_gameManager.GetNormalizedElapsedTime() > _normalizedEndTime)
        {
            EnterState(GameState.END);
        }
    }

    void EndStateUpdate()
    {
        HandlePoseDetection();
        HandleTime();
        HandleBlackPanelFade();
        HandleVisualizationReset();
    }

    void HandlePoseDetection()
    {
        bool poseDetected = _poseReceiver.GetPersonDetected();

        if (!_prevPoseDetected && poseDetected)
        {
            _byteEventList[(int)_currentVizType].Invoke(new byte[] { 0 });
        }
        if (_prevPoseDetected && !poseDetected)
        {
            _byteEventList[(int)_currentVizType].Invoke(new byte[] { 1 });
        }

        _prevPoseDetected = poseDetected;
    }

    void ChangeVisualization()
    {
        if (!_vizSetted)
        {
            _currentVizType = _vizTypeList[((int)_currentVizType + 1) % _vizTypeList.Length];
            _vizSetted = true;
            _gameManager.SetNormalizedElapsedTime(0.001f);
        }
    }

    void HandleTime()
    {
        if (_gameManager.GetNormalizedElapsedTime() > _normalizedEndTime)
        {
            _gameManager.SetTimeScale(0f);
            RestoreTimeAfter(_restoreTime);
        }
        else
        {
            RestoreTimeAfter(_restoreTimeBeforeEnd);
        }

        if (_gameManager.GetNormalizedElapsedTime() < 0)
        {
            _gameManager.SetTimeScale(0f);
            if (_poseReceiver.GetPersonDetected())
                _gameManager.SetTimeScale(1f);
        }
    }

    void HandleBlackPanelFade()
    {
        float targetAlpha = (_gameManager.GetNormalizedElapsedTime() > _normalizedEndTime) ? 1f : 0f;
        _blackPanel.color = new Color(_blackPanel.color.r, _blackPanel.color.g, _blackPanel.color.b, Mathf.Lerp(_blackPanel.color.a, targetAlpha, 0.1f));
    }

    void HandleVisualizationReset()
    {
        if (_prevResetTimeSpeed >= 0 && _gameManager.GetTimeScale() < 0)
        {
            _vizSetted = false;
        }
        _prevResetTimeSpeed = _gameManager.GetTimeScale();
    }

    void RestoreTimeAfter(float restoreTime)
    {
        if (_poseReceiver.GetPersonDetected())
        {
            _exitTime = Time.time;
        }

        if (Time.time - _exitTime > restoreTime)
        {
            _resetTimeSpeed = -GameManager.Instance.GetGameDuration() / _resetTimeDuration;
            _gameManager.SetTimeScale(_resetTimeSpeed);

            if (_gameManager.GetNormalizedElapsedTime() > _normalizedEndTime)
            {
                _gameManager.SetNormalizedElapsedTime(_normalizedEndTime);
            }
        }
    }
}
