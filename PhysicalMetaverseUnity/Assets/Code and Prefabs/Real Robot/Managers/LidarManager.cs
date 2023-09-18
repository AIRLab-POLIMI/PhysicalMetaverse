using System;
using System.Linq;
using Core;
using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class LidarManager : Monosingleton<LidarManager>
{

    [SerializeField] private GameObject lidarPoint;

    [SerializeField] private GameObject lidarPoint2;

    [SerializeField] private IntSO lidarMode;

    private GameObject[] _points;

    private int[] _measurements;

    private  int arraySize = 360;

    private float minDistValue = 1.0f;
    private float maxDistValue = 20.0f;

    private int minMeasure = 0;
    private int maxMeasure = 5000;

    private Vector3 defaultHidePosition = new Vector3(-0.5f, -10f, -12.0f);
    //private Vector3 defaultScale = new Vector3()

    [SerializeField] private FloatSO distanceFromCamera; //In centimeters, positive if lidar is behind camera, negative if lidar is in frontm of the camera

    [SerializeField] private FloatSO poseDistance;

    private float defaultPoseDistance = 10.0f;
    
    private int nOfLidarDegrees = 0;
    private int[] trackedCameraDegrees;

    private PoseManager _poseManager = null;

    [SerializeField] private IntSO MaxConvertedAngle;
    [SerializeField] private IntSO MinConvertedAngle;

    public void Setup()
    {
        Debug.Log("[Lidar Manager setup]");

        _poseManager = FindObjectOfType<PoseManager>();

        nOfLidarDegrees = LidarToCameraRange.LidarDegreesBasedOnDistance(distanceFromCamera.runtimeValue);

        _points = new GameObject[arraySize];

        _measurements = new int[arraySize];

        //Points are spawned as inactive, because prefab is inactive. 
        SpawnPoints();
    }

    public void ActivatePoints()
    {
        for (int i = 0; i < arraySize; i++)
        {
            _points[i].SetActive(true);
        }
    }

    [SerializeField] private float newTolerance = 1.2f; //1 is no tolerance
    private int[] currentPositions;

    //range _aggregatePointsTolerance from 0 to 100
    [Range(1, 2)] [SerializeField] private float _aggregatePointsTolerance = 1.1f; //1 is no tolerance
    public int _skippablePoints = 0;

    public bool _mergeWalls = true;
    private List<GameObject> blobs = new List<GameObject>();

    public void OnMsgRcv(byte[] msg)
    {
        int[] bytesAsInts = new int[arraySize];
        Buffer.BlockCopy(msg, 0, bytesAsInts, 0, msg.Length);
        //log bytesAsInts
        //Debug.Log(bytesAsInts);

        trackedCameraDegrees = CopyTrackedDegrees(bytesAsInts, nOfLidarDegrees);
        
        //var sb = new StringBuilder("new int[] { ");

        int i = 0;

        if(currentPositions == null){
            currentPositions = new int[arraySize];
            foreach (int n in bytesAsInts)
            {
                UpdatePosition(i, n);
                i++;
            }
        }
        else
        {
            foreach (int n in bytesAsInts)
            {
                //if max between current and new divided by min between current and new is greater than tolerance then update
                float relativeDistance = (float)Math.Max(currentPositions[i], n) / (float)Math.Min(currentPositions[i], n);
                if(!(currentPositions[i] == 0 && n == 0) && relativeDistance > newTolerance)
                {
                    UpdatePosition(i, n);
                    currentPositions[i] = n;
                }
                //sb.Append(n + ", ");
                //UpdatePosition(i, n);
                i++;
            }
        }

        if(_mergeWalls)
            WallsUsingDistances();
        if(!_mergeWalls){
            //destroy all elements in blobs
            foreach (GameObject blob in blobs)
            {
                Destroy(blob);
            }
        }
    }

    void WallsUsingDistances(){
        int skippedPoints = 0;
        int lastValid = 0;
        int newStart = 0;
        float _debugLineTime = 0.5f;
        //destroy all blobs
        foreach (GameObject blob in blobs)
        {
            Destroy(blob);
        }
        //for first 10 points print their distance difference if more than aggregatePointsTolerance
        for (int j = 0; j < 360; j++)
        {
            //check distance difference between current and next
            int next = j + 1;
            if (next == 360)
            {
                Debug.DrawLine(_points[newStart].transform.position, _points[j].transform.position, Color.red, _debugLineTime);
                //spawn cube along line stretching it
                SpawnWall(newStart, j);

                break;
            }
            float relativeDistance = (float)Math.Max(currentPositions[j], currentPositions[next]) / (float)Math.Min(currentPositions[j], currentPositions[next]);

            //if another obstacle is found, not infinity
            if (relativeDistance > _aggregatePointsTolerance + _aggregatePointsTolerance / 10 && relativeDistance < _aggregatePointsTolerance * 10){
                Debug.Log("New blob found, blob ended");
                //draw last valid point
                Debug.DrawLine(_points[newStart].transform.position, _points[lastValid].transform.position, Color.red, _debugLineTime);
                //spawn cube along line stretching it
                SpawnWall(newStart, j);
                //break;
                newStart = next;
                lastValid = next;
                skippedPoints = 0;
            }
            else if (relativeDistance > _aggregatePointsTolerance)
            {
                skippedPoints++;
                Debug.Log("Distance difference between " + j + " and " + next + " is " + relativeDistance);
                //draw a debug line that goes from _point 0 to point j
            }
            else
            {
                if(j > 0 && skippedPoints == 0)
                    lastValid = j-1;
                skippedPoints = 0;
            }
            if (relativeDistance > _aggregatePointsTolerance * 10)
            {
                if(skippedPoints > _skippablePoints)
                {
                    Debug.Log("Too far, blob ended");
                    //draw last valid point
                    Debug.DrawLine(_points[newStart].transform.position, _points[lastValid].transform.position, Color.red, _debugLineTime);
                    //spawn cube along line stretching it
                    SpawnWall(newStart, j);
                    //break;
                    newStart = next;
                    lastValid = next;
                    skippedPoints = 0;
                }
            }
        }
        
        //////// GPT ALTERNATIVE, add epsilon to avoid zero check
        /*
        //init currentPositions outside
        foreach (int n in bytesAsInts)
            {
                //if max between current and new divided by min between current and new is greater than tolerance then update
                float relativeDistance = (float)Math.Max(currentPositions[i] +1, n +1) / (float)Math.Min(currentPositions[i] +1, n +1);
                if(relativeDistance > newTolerance)
                {
                    UpdatePosition(i, n);
                    currentPositions[i] = n;
                }

                i++;
            }
        */

        //UpdatePoseDistance(); //MAYBE IMPORTANT BUT NOT WORKING

        //sb.Append("}");
        //Debug.Log(sb.ToString());
    }
    private void SpawnWall(int start, int end){
        Vector3 startVec = _points[start].transform.position;
        Vector3 endVec = _points[end].transform.position;
        Vector3 middleVec = (startVec + endVec) / 2;
        float length = Vector3.Distance(startVec, endVec);
        Vector3 scaleVec = new Vector3(0.1f, 6.0f, length + length*0.1f );
        Vector3 directionVec = endVec - startVec;
        Quaternion rotation = Quaternion.LookRotation(directionVec);
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = middleVec;
        wall.transform.localScale = scaleVec;
        wall.transform.rotation = rotation;
        blobs.Add(wall);

    }

    private void SpawnWallQuaternion(int start, int end){
        //rotations
        List<Quaternion> rotations = new List<Quaternion>();
        Vector3 startVec = new Vector3(0,0,0);
        Vector3 endVec = new Vector3(0,0,0);
        //spawn cube along line stretching it, consider all points in the middle too
        for(int q = start; q < end; q++){
            //calculate direction of q and q+1
            startVec = _points[q].transform.position;
            endVec = _points[q+1].transform.position;
            Vector3 directionVec = endVec - startVec;
            Quaternion rotation = Quaternion.LookRotation(directionVec);
            rotations.Add(rotation);
        }
        //get average rotation quaternion
        Quaternion averageRotation = new Quaternion(0,0,0,0);
        foreach(Quaternion q in rotations){
            averageRotation = Quaternion.Lerp(averageRotation, q, 1.0f/rotations.Count);
        }


        //spawn cube
        startVec = _points[start].transform.position;
        endVec = _points[end].transform.position;
        Vector3 middleVec = (startVec + endVec) / 2;
        float length = Vector3.Distance(startVec, endVec);
        Vector3 scaleVec = new Vector3(0.1f, 6.0f, length + length*0.1f );
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = middleVec;
        wall.transform.localScale = scaleVec;
        wall.transform.rotation = averageRotation;
        blobs.Add(wall);


    }

    private void UpdatePoseDistance()
    {
        //No pose detected or invalid pose
        if (MinConvertedAngle.runtimeValue == -1 || MaxConvertedAngle.runtimeValue == -1)
        {
            //poseDistance.runtimeValue = defaultPoseDistance;
            //Debug.Log(poseDistance.runtimeValue);
        }
        else
        {
            int nOfPoseDegrees = MaxConvertedAngle.runtimeValue - MinConvertedAngle.runtimeValue + 1;
            Debug.Log(nOfPoseDegrees);
            int halfDegrees = (nOfPoseDegrees + 1) / 2;

            if (halfDegrees == 0)
                halfDegrees = 1;
            
            int[] arrayCopy = new int[nOfPoseDegrees];

            int j = 0;
            for (int i = MinConvertedAngle.runtimeValue; i <= MaxConvertedAngle.runtimeValue; i++)
            {
                arrayCopy[j] = trackedCameraDegrees[i];
                j++;
            }
            
            Array.Sort(arrayCopy);

            int sum = 0;
            
            for (int i = 0; i < halfDegrees; i++)
            {
                sum += arrayCopy[i];
            }

            int littleMean = sum / halfDegrees;

            int tolerance = 750; //CM

            sum = 0;

            j = 0;
            
            for (int i = MinConvertedAngle.runtimeValue; i <= MaxConvertedAngle.runtimeValue; i++)
            {
                int angle = trackedCameraDegrees[i];

                if (angle > (littleMean + tolerance) || angle < (littleMean - tolerance))
                {
                    //skip this angle
                    
                }
                else
                {
                    sum += angle;
                    j++;
                }
            }

            /* (int i = lastConvertedAngles[0]; i <= lastConvertedAngles[1]; i++)
            {
                sum += trackedCameraDegrees[i];
            }
            
            float mean = sum / (lastConvertedAngles[1] - lastConvertedAngles[0] + 1);*/

            if (j != 0)
            {
                if (sum == 0)
                {
                    //poseDistance.runtimeValue = defaultPoseDistance;
                    return;
                }
                float mean = sum / j;
            
                //Debug.Log(lastConvertedAngles[1] - lastConvertedAngles[0] + 1);
            
                poseDistance.runtimeValue = mean/100.0f;
                //Debug.Log(poseDistance.runtimeValue); 
            }
        }
    }

    private int[] CopyTrackedDegrees(int[] inputArray, int trackedDegrees)
    {
        int[] outputArray = new int[trackedDegrees];

        int isOdd = trackedDegrees % 2;

        int halfTrackedArray = trackedDegrees / 2;

        int start = inputArray.Length - (halfTrackedArray + isOdd);

        int j = 0;
        
        for (int i = start; i < inputArray.Length; i++)
        {
            outputArray[j] = inputArray[i];
            j++;
        }

        for (int i = 0; i < halfTrackedArray; i++)
        {
            outputArray[j] = inputArray[i];
            j++;
        }

        return outputArray;
    }

    private void SpawnPoints()
    {
        GameObject toSpawn;
        switch (lidarMode.runtimeValue)
        {
            case 1:
                toSpawn = lidarPoint;
                break;
            case 2:
                toSpawn = lidarPoint2;
                break;
            default:
                toSpawn = lidarPoint;
                break;
        }
        
        float radius = 10f;

        for (int i = 0; i < arraySize; ++i)
        {
            float circleposition = (float)i / (float)arraySize;
            float x = Mathf.Sin(circleposition * Mathf.PI * 2.0f) * radius;
            float z = Mathf.Cos(circleposition * Mathf.PI * 2.0f) * radius;
            GameObject obj = Instantiate(toSpawn, new Vector3(x, 0.0f, z), Quaternion.LookRotation(new Vector3(x,0.0f,z)));
            //GameObject obj = Instantiate(lidarPoint, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.LookRotation(new Vector3(x, 0.0f, z)));
            obj.transform.parent = transform;
            //obj.transform.position += obj.transform.forward*8.0f;
            _points[i] = obj;
        }
    }

    private void UpdatePosition(int pos, int value)
    {
        if (value == 0) //Default invalid value
        {
            StartCoroutine(FadeOutPoint(_points[pos]));
            //_points[pos].transform.position = defaultHidePosition;
            //_points[pos].transform.localScale = defaultScale;
        }
        else
        {
            StartCoroutine(FadeInPoint(_points[pos]));
            if ((ConvertAngleTo360(pos) >= MinConvertedAngle.runtimeValue && ConvertAngleTo360(pos) <= MaxConvertedAngle.runtimeValue) && (MinConvertedAngle.runtimeValue != -1)) //if one of the angles where Pose is
            {
                _points[pos].GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                if(!_mergeWalls)
                    _points[pos].GetComponent<MeshRenderer>().enabled = true;
                else
                    _points[pos].GetComponent<MeshRenderer>().enabled = false;
            }
            
            //float convertedValue = ConvertRange(value);
            float convertedValue = (((float) value) / 100.0f)-0.5f;
            float circleposition = (float)pos / (float)arraySize;
            float x = Mathf.Sin(circleposition * Mathf.PI * 2.0f) * convertedValue;
            float z = Mathf.Cos(circleposition * Mathf.PI * 2.0f) * convertedValue;
            _points[pos].transform.position = new Vector3(x, 0.0f, z);

            _points[pos].transform.localScale = new Vector3(convertedValue / 10, _points[pos].transform.localScale.y,
                convertedValue / 10);
            //if point pos is first 15 or last 15 then scale y to 0.1
            //if (pos < 30 || pos > 330)
            //{
            //    _points[pos].transform.localScale = new Vector3(_points[pos].transform.localScale.x, 0.5f,
            //        _points[pos].transform.localScale.z);
            //}
        }
    }

    
    [SerializeField] float fadeDuration = 0.05f; // Duration of the fade-out effect in seconds

    private IEnumerator FadeOutPoint(GameObject point)
    {
        Renderer renderer = point.GetComponent<Renderer>();
        Material material = renderer.material;

        Color originalColor = material.color;
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // Fade out to fully transparent

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration; // Normalized time value (0 to 1)
            //fadeout linear duration/elapsed
            material.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the material color is set to the target color at the end of the fade-out
        material.color = targetColor;
    }

    private IEnumerator FadeInPoint(GameObject point)
    {
        Renderer renderer = point.GetComponent<Renderer>();
        Material material = renderer.material;

        Color originalColor = material.color;
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 1f); // Fade in to fully opaque

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration; // Normalized time value (0 to 1)
            //fadeout linear duration/elapsed
            material.color = new Color(originalColor.r, originalColor.g, originalColor.b, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the material color is set to the target color at the end of the fade-in
        material.color = targetColor;
    }

    private float ConvertRange(int oldValue)
    {
        float newValue = ((((float) oldValue - (float) minMeasure) * (maxDistValue - minDistValue)) /
                    (maxMeasure - minMeasure)) + minDistValue;
        return newValue;
    }

    private int ConvertAngleTo360(int oldAngle)
    {
        int newAngle = oldAngle + (nOfLidarDegrees / 2);
        if (newAngle > 359)
        {
            newAngle = newAngle - 360;
        }

        return newAngle;
    }
}