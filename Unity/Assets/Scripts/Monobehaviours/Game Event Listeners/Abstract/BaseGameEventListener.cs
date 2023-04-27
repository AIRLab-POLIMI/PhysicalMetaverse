using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEvents
{
    public class BaseGameEventListener : MonoBehaviour, IPrioritized
    {
        public PriorityLevel priorityLevel = PriorityLevel.Low;

        public PriorityLevel Priority => priorityLevel;
    }
}
