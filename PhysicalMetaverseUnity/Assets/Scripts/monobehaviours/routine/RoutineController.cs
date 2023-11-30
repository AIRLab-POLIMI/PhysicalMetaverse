
using System.Collections;
using System.Collections.Generic;
using Core;
using Unity.VisualScripting;
using UnityEngine;


public class RoutineController : Monosingleton<RoutineController>
{

    [SerializeField] bool _playCoroutine = false;
    [SerializeField] bool _stopCoroutine = false;
    [SerializeField] float routineDuration;
    
    [SerializeField] List<RoutineComponent> routineComponents;

    
    public bool IsRunning => _coroutine != null;
    
    private Coroutine _coroutine;

    private string _currentMsg;
    
    
    private void Start()
    {
        _coroutine = null;
        
        foreach (var component in routineComponents)
            component.Start(routineDuration);
    }
    
    private void FixedUpdate(){
        if(_playCoroutine){
            _playCoroutine = false;
            Activate();
        }
        //if _stopCoroutine call Stop()
        if(_stopCoroutine){
            _stopCoroutine = false;
            Stop();
        }
    }
    
    #region Activate Routine

        public void Activate()
        {
            // debug log
            //Debug.Log("RoutineController.Activate()");
            
            if (_coroutine != null)
                StopCoroutine(_coroutine);
            
            _coroutinePlaying = true;
            _coroutine = StartCoroutine(ActivateRoutine());
        }

        public void Stop()
        {
            _coroutinePlaying = false;
        }

        public string GetMsg()
        {
            if (!IsRunning)
                return "";
            
            return _currentMsg;
        }
        
        private bool _coroutinePlaying = false;

        private IEnumerator ActivateRoutine()
        {
            // call GetMgs for every component in the list using the elapsed time from the start of the coroutine as input
            // until routineDuration is reached
            float elapsedTime = 0;
            while (elapsedTime < routineDuration && _coroutinePlaying)
            {
                GetMsg(elapsedTime);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            GetMsg(routineDuration);
            
            _coroutine = null;
        }

        public string GetInitialPositionMsg()
        {
            _currentMsg = "";
            foreach (var component in routineComponents)
                _currentMsg += component.GetDefaultMsg() + Constants.MsgDelimiter;
            
            // remove the last delimiter
            _currentMsg = _currentMsg.Remove(_currentMsg.Length - 1);
            return _currentMsg;
        }

        private void GetMsg(float elapsedTime)
        {
            _currentMsg = "";
            foreach (var component in routineComponents)
                _currentMsg += component.GetMsg(elapsedTime) + Constants.MsgDelimiter;
                
            // remove the last delimiter
            _currentMsg = _currentMsg.Remove(_currentMsg.Length - 1);
        }

    #endregion

}
