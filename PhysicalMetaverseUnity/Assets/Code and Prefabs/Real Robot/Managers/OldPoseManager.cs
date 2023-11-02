using System;
using System.Linq;
using Core;
using UnityEngine;
using System.Text;


//This class handles the reception of the pose message. It:
//-Updates the Array of ScriptableObjets corresponding to all the keypoints
public class OldPoseManager : Monosingleton<OldPoseManager>
{
    private const int arraySize = 18;

    [SerializeField] private FloatSO distanceFromCenter;

    [SerializeField] private FloatSO lidarCameraDistance;

    private int cameraDegrees = 64;

    private int lastMinAngle = -1;

    private int lastMaxAngle = -1;

    private int nOfLidarDegrees;

    [SerializeField] private DoubleFloatSO[] poseKeypoints;

    [SerializeField] private IntSO poseMode; //1 default all points - 2 triangle - 3 coral entity - 4 abstract

    [SerializeField] private GameObject[] poseRepresentations;

    [SerializeField] private IntSO MaxConvertedAngle;
    [SerializeField] private IntSO MinConvertedAngle;

    [SerializeField] private FloatSO LookingAt;

    [SerializeField] private FloatSO QtyOfMovement;

    private float old_left_elbow_1 = 0f;
    private float old_left_elbow_2 = 0f;
    
    private float old_right_elbow_1 = 0f;
    private float old_right_elbow_2 = 0f;
    
    private float old_left_hand_1 = 0f;
    private float old_left_hand_2 = 0f;
    
    private float old_right_hand_1 = 0f;
    private float old_right_hand_2 = 0f;

    private float old_neck_1 = 0f;
    private float old_neck_2 = 0f;
    
    public void Setup()
    {
        Debug.Log("[Pose Manager setup]");
        
        nOfLidarDegrees = LidarToCameraRange.LidarDegreesBasedOnDistance(lidarCameraDistance.runtimeValue);

        InitializePoseMode();
    }

    private void InitializePoseMode()
    {
        switch (poseMode.runtimeValue)
        {
            case 1:
                poseRepresentations[0].SetActive(true);
                break;
            case 2:
                poseRepresentations[1].SetActive(true);
                break;
            case 3:
                poseRepresentations[2].SetActive(true);
                break;
            case 4:
                poseRepresentations[3].SetActive(true);
                break;
            default:
                poseRepresentations[0].SetActive(true);
                break;
        }
    }

    public void ConvertedDegrees()
    {
        if (lastMinAngle == -1 || lastMaxAngle == -1)
        {
            MinConvertedAngle.runtimeValue = -1;
            MaxConvertedAngle.runtimeValue = -1;
        }
        else
        {
            
            float ratio = (float)nOfLidarDegrees / (float)cameraDegrees;
            //Debug.Log(ratio);
            //Debug.Log(lastMinAngle * ratio);
            //Debug.Log(lastMaxAngle * ratio);
            MinConvertedAngle.runtimeValue = (int) ((float)lastMinAngle * (float)ratio);
            MaxConvertedAngle.runtimeValue = (int) ((float)lastMaxAngle * (float)ratio);
        }
    }

    public void OnMsgRcv(byte[] msg)
    {
        //Parse the first byte, corresponding to the number of measurements
        int[] numberOfValues = new int[1];
        Buffer.BlockCopy(msg, 0, numberOfValues, 0, 4);

        //Parse the measurements
        float[] bytesAsFloat = new float[numberOfValues[0]];
        Buffer.BlockCopy(msg, 4, bytesAsFloat, 0, msg.Length - 4);

        //UpdatePositions(bytesAsFloat, numberOfValues[0]);
        UpdateKeypoints(bytesAsFloat, numberOfValues[0]);
    }

    private void UpdateKeypoints(float[] array, int len) //Updates also min ad max X
    {
        float minX = 2.0f;
        float maxX = -1.0f;
        
        bool[] moved = new bool[arraySize];

        for (int i = 0; i < len; i += 3)
        {
            
            if (array[i] == 5.0f) //left shoulder
            {
                minX = array[i + 2];
            }
            if (array[i] == 6.0f) //right shoulder
            {
                maxX = array[i + 2];
            }
            
            /*//Update min and max
            if (array[i + 2] > maxX)
                maxX = array[i + 2];

            if (array[i + 2] < minX)
                minX = array[i + 2];*/
            
            int index = (int) array[i];
            poseKeypoints[index].runtimeValue1 = array[i + 1];
            poseKeypoints[index].runtimeValue2 = array[i + 2];
            moved[index] = true;
        }

        for (int i = 0; i < arraySize; i++)
        {
            if (!moved[i])
            {
                poseKeypoints[i].runtimeValue1 = -1.0f;
                poseKeypoints[i].runtimeValue2 = -1.0f;
            }
        }
        
        UpdateMinAndMaxConvertedInDegrees(minX, maxX);
        ConvertedDegrees();
        CalculateLookingAt();
        UpdateRepresentation();
        CalculateQtyOfMotion();
    }

    private void UpdateRepresentation()
    {
        switch (poseMode.runtimeValue)
        {
            case 1:
                poseRepresentations[0].GetComponent<DefaultRepresentation>().UpdateRep();
                break;
            case 2:
                poseRepresentations[1].GetComponent<TriangleRepresentation>().UpdateRep();
                break;
            case 3:
                poseRepresentations[2].GetComponent<EntityRepresentation>().UpdateRep();
                break;
            case 4:
                //poseRepresentations[3].SetActive(true);
                break;
            default:
                poseRepresentations[0].GetComponent<DefaultRepresentation>().UpdateRep();
                break;
        }
    }
    
    private void UpdateMinAndMaxConvertedInDegrees(float min, float max)
    {
        //If at least one of the values is invalid, set to invalid
        if (min > 1.0f || max < 0.0f)
        {
            lastMaxAngle = -1;
            lastMinAngle = -1;
        }
        else
        {
            if (min > max)
            {
                //switch
                (min, max) = (max, min);
            }
            lastMinAngle = (int) Mathf.Lerp(0.0f, (float) cameraDegrees, min);
            lastMaxAngle = (int) Mathf.Lerp(0.0f, (float) cameraDegrees, max);
        }
    }

    private void CalculateLookingAt()
    {
        if (poseKeypoints[0].runtimeValue1 == -1.0f) //nose point
        {
            return;
        }
        if (poseKeypoints[1].runtimeValue1 == -1.0f) //left eye point
        {
            return;
        }
        if (poseKeypoints[2].runtimeValue1 == -1.0f) //right eye point
        {
            return;
        }

        LookingAt.runtimeValue = LinearConversion.LinearConversionFloat(poseKeypoints[0].runtimeValue2,
            poseKeypoints[1].runtimeValue2, poseKeypoints[2].runtimeValue2, 0.0f, 1.0f);
    }

    private void CalculateQtyOfMotion()
    {
        if (poseKeypoints[7].runtimeValue1 == -1.0f) //left elbow
        {
            QtyOfMovement.runtimeValue = 0f;
            return;
        }
        if (poseKeypoints[8].runtimeValue1 == -1.0f) //right elbow
        {
            QtyOfMovement.runtimeValue = 0f;
            return;
        }
        if (poseKeypoints[9].runtimeValue1 == -1.0f) //left hand
        {
            QtyOfMovement.runtimeValue = 0f;
            return;
        }
        if (poseKeypoints[10].runtimeValue1 == -1.0f) //right hand
        {
            QtyOfMovement.runtimeValue = 0f;
            return;
        }
        if (poseKeypoints[17].runtimeValue1 == -1.0f) //neck
        {
            QtyOfMovement.runtimeValue = 0f;
            return;
        }

        float left_elbow_dist = CalculateEuclideanDistance(old_left_elbow_1, old_left_elbow_2, poseKeypoints[7]);
        float right_elbow_dist = CalculateEuclideanDistance(old_right_elbow_1, old_right_elbow_2, poseKeypoints[8]);
        float left_hand_dist = CalculateEuclideanDistance(old_left_hand_1, old_left_hand_2, poseKeypoints[9]);
        float right_hand_dist = CalculateEuclideanDistance(old_right_hand_1, old_right_hand_2, poseKeypoints[10]);
        float neck_dist = CalculateEuclideanDistance(old_neck_1, old_neck_2, poseKeypoints[17]);

        //float avg_dist = (left_elbow_dist + right_elbow_dist + left_hand_dist + right_hand_dist) / 4;

        //QtyOfMovement.runtimeValue = (left_elbow_dist - avg_dist) + (right_elbow_dist - avg_dist) +
        //                             (left_hand_dist - avg_dist) + (right_hand_dist - avg_dist);

        //QtyOfMovement.runtimeValue = left_elbow_dist + right_elbow_dist + left_hand_dist + right_hand_dist;
        
        QtyOfMovement.runtimeValue = (left_elbow_dist - neck_dist) + (right_elbow_dist - neck_dist) +
                                     (left_hand_dist - neck_dist) + (right_hand_dist - neck_dist);


        old_left_elbow_1 = poseKeypoints[7].runtimeValue1;
        old_left_elbow_2 = poseKeypoints[7].runtimeValue2;
        
        old_right_elbow_1 = poseKeypoints[8].runtimeValue1;
        old_right_elbow_2 = poseKeypoints[8].runtimeValue2;
        
        old_left_hand_1 = poseKeypoints[9].runtimeValue1;
        old_left_hand_2 = poseKeypoints[9].runtimeValue2;
        
        old_right_hand_1 = poseKeypoints[10].runtimeValue1;
        old_right_hand_2 = poseKeypoints[10].runtimeValue2;

        old_neck_1 = poseKeypoints[17].runtimeValue1;
        old_neck_2 = poseKeypoints[17].runtimeValue2;

    }

    private float CalculateEuclideanDistance(float oldValue_1, float oldValue_2, DoubleFloatSO newValue)
    {
        return Mathf.Sqrt(Mathf.Pow(oldValue_1 - newValue.runtimeValue1, 2) +
                         Mathf.Pow(oldValue_2 - newValue.runtimeValue2, 2));
    }
}
