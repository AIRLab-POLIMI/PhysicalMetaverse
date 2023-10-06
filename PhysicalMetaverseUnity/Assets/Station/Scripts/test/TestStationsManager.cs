using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestStationsManager : MonoBehaviour
{
    
    [Space]
    
    [Header("TIME")]
    
    [SerializeField, Range(0, 600)] private float gameDurationSeconds;

    [Range(0, 1)]
    [SerializeField]
    private float _normalisedElapsedTime;

    private void Start()
    {
        _normalisedElapsedTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTime();
    }

    public GameObject _losePanel;
    void UpdateTime()
    {
        _normalisedElapsedTime += Time.deltaTime/gameDurationSeconds;
        if (_normalisedElapsedTime >= gameDurationSeconds)
        {
            // end game
        }   
        AmbientManager.Instance.UpdateLight(_normalisedElapsedTime);
        if(_normalisedElapsedTime > 1){
            //enable _losePanel
            _losePanel.SetActive(true);
        }
    }
}
