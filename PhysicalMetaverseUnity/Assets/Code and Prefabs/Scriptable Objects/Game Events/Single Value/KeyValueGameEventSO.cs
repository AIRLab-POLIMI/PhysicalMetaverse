using UnityEngine;

namespace GameEvents
{
    [CreateAssetMenu(fileName = "KeyValue Game Event",
        menuName = "Scriptable Objects/Game Events/One Input/KeyValue Game Event")]

    public class KeyValueGameEventSO : ValueGameEventSO<KeyValueMsg>
    {
    }
}