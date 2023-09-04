
using UnityEngine;
using UnityEngine.Events;

namespace GameEvents
{
    public class VoidGameEventListener : BaseGameEventListener
    {
        [SerializeField] private UnityEvent _unityEvent;
        [SerializeField] private VoidGameEventSO _gameEvent;

        private void OnEnable() => _gameEvent.Subscribe(this);
        
        private void OnDisable() => _gameEvent.Unsubscribe(this);
        
        public void RaiseEvent() => _unityEvent?.Invoke();
    }
}
