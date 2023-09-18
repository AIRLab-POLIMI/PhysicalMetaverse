using UnityEngine.Events;
using UnityEngine;

public class EventByteKeyMessageResponder : ByteKeyMessageResponder
{
    [SerializeField] private UnityEvent<byte[]> byteEventResponse;

    protected override void MessageResponse(byte[] val) => byteEventResponse?.Invoke(val);
}
