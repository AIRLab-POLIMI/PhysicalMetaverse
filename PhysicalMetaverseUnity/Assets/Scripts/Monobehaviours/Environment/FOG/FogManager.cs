using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogManager : MonoBehaviour
{

    [SerializeField] private ParticleSystem FogLow;
    [SerializeField] private ParticleSystem FogTall;
    [SerializeField] private ParticleSystem FogCupola;

    [SerializeField] private IntSO poseMode;
    // Start is called before the first frame update
    void Start()
    {
        if (poseMode.runtimeValue == 4)
        {
            ParticleSystem.ShapeModule lowShape = FogLow.shape;
            lowShape.radius = 7;
            ParticleSystem.ShapeModule tallShape = FogTall.shape;
            tallShape.radius = 7;
            ParticleSystem.ShapeModule cupolaShape = FogCupola.shape;
            cupolaShape.radius = 7;
        }
    }
    
}
