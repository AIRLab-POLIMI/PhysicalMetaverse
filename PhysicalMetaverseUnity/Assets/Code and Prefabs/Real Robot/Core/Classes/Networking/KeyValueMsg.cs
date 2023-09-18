
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.OpenXR;

public class KeyValueMsg
{
    public byte key;
    public byte[] value;
    public float floatValue;
    public bool isFloat;

    public KeyValueMsg(byte key, byte[] value)
    {
        this.key = key;
        this.value = value;
        //floatValue = StringMsgHelper.StringToFloat(value, out this.isFloat);
    }

    public static KeyValueMsg ParseKeyValueMsg(byte[] msg)
    {
        var keyVal = msg[0];

        var msgLen = msg.Length;

        byte[] value = new byte[msgLen - 1];
        Array.Copy(msg, sourceIndex: 1, value, destinationIndex: 0, length: msgLen-1);


        return new KeyValueMsg(keyVal, value);
        /*return keyVal.Length != 2
            ? null
            : new KeyValueMsg(keyVal[0], keyVal[1]);*/
    }
}
