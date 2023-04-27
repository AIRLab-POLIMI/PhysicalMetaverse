
using System.Collections.Generic;
using UnityEngine;

namespace GameEvents
{
    [CreateAssetMenu(fileName = "Void Game Event", menuName = "Scriptable Objects/Game Events/Void Game Event")]

    public class VoidGameEventSO : BaseGameEventSO
    {
        private List<VoidGameEventListener> _listeners = new List<VoidGameEventListener>();

        public void Subscribe(VoidGameEventListener listener)
        {
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
                
                PriorityLevelHelper.SortByPriorityLevelDescending(_listeners);
            }
        }

        public void Unsubscribe(VoidGameEventListener listener)
        {
            if (_listeners.Contains(listener))
                _listeners.Remove(listener);
        }

        public void Invoke()
        {
            foreach (var listener in _listeners)
            {
                listener.RaiseEvent();
            }
        }
    }
}
