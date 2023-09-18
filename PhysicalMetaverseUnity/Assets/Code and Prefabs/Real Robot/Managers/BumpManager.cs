
using System;
using System.Collections.Generic;
using Core;
using UnityEngine;

public class BumpManager : Monosingleton<BumpManager>
{

    [SerializeField] private FlashImage _flashImage = null;

    [SerializeField] private IntSO numOfBump;
    
    public void Setup()
    {
        Debug.Log("[Bump Manager setup]");
        numOfBump.runtimeValue = 0;
    }
    

    public void OnMsgRcv(byte[] msg)
    {
        var value = msg[0];
        if (value == 0x00)
        {
            Debug.Log("END BUMP");    
        }else if (value == 0x01)
        {
            Debug.Log("BUMP");
            _flashImage.StartFlash(0.25f, 0.5f, Color.red);
            numOfBump.runtimeValue++;
        }
        else
        {
            Debug.Log("INVALID BUMP MSG");
        }
    }
}
