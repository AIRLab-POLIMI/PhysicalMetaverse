
using System.Collections.Generic;
using UnityEngine;

namespace GameEvents
{
    public abstract class ValueGameEventSO<TInput> : BaseGameEventSO
    {
        private List<ValueGameEventListener<TInput>> _listeners = new List<ValueGameEventListener<TInput>>();

        public void Subscribe(ValueGameEventListener<TInput> listener)
        {
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
                
                PriorityLevelHelper.SortByPriorityLevelDescending(_listeners);
            }
        }
        
        public void Unsubscribe(ValueGameEventListener<TInput> listener)
        {
            if (_listeners.Contains(listener))
                _listeners.Remove(listener);
        }
        
        public void Invoke(TInput value)
        {
            // Debug.Log($"[ValueGameEventSO][Invoke] - VALUE: {typeof(TInput)} - CALLED with '{_listeners.Count}' listeners");
            foreach (var listener in _listeners) 
                listener.RaiseEvent(value);
        }
    }
}
