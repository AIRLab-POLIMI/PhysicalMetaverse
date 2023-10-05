
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;


public class RoutineController : Monosingleton<RoutineController>
{

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
    
    
    #region Activate Routine

        public void Activate()
        {
            // debug log
            Debug.Log("RoutineController.Activate()");
            
            if (_coroutine != null)
                StopCoroutine(_coroutine);
                
            _coroutine = StartCoroutine(ActivateRoutine());
        }

        public string GetMsg()
        {
            if (!IsRunning)
                return "";
            
            return _currentMsg;
        }
        
        private IEnumerator ActivateRoutine()
        {
            // call GetMgs for every component in the list using the elapsed time from the start of the coroutine as input
            // until routineDuration is reached
            float elapsedTime = 0;
            while (elapsedTime < routineDuration)
            {
                GetMsg(elapsedTime);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            GetMsg(routineDuration);
            
            _coroutine = null;
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
