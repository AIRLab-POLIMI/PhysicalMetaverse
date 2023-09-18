
public abstract class ByteKeyMessageResponder : KeyMessageResponder<byte[]>
{
    public void OnKeyValueMsgReceived(KeyValueMsg keyValueMsg)
    {
        if (dofKey.runtimeValue == keyValueMsg.key)
            MessageResponse(keyValueMsg.value);
    }
}