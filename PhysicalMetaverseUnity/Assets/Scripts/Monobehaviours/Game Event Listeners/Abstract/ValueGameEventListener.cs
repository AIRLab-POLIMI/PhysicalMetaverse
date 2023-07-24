
using UnityEngine;
using UnityEngine.Events;

namespace GameEvents
{
    public abstract class ValueGameEventListener<TInput> : BaseGameEventListener
    {
        [SerializeField] private UnityEvent<TInput> _unityEvent;
        [SerializeField] private ValueGameEventSO<TInput> _gameEvent;
        
        private void OnEnable() => _gameEvent.Subscribe(this);
        private void OnDisable() => _gameEvent.Unsubscribe(this);
        
        public void RaiseEvent(TInput value) => _unityEvent?.Invoke(value);
    }
}
