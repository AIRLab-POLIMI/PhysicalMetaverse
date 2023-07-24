using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleRepresentation : MonoBehaviour
{
    private const int arraySize = 3;

    private GameObject[] _points;
    
    [SerializeField] private DoubleFloatSO[] poseKeypoins;

    private const int cameraDegrees = 64;
    
    [SerializeField] private FloatSO distanceFromCenter;
    
    private float scaling = 5.0f;

    [SerializeField] private GameObject UpperTriangle;
    [SerializeField] private GameObject LowerTriangle;

    private Mesh upperMesh;
    private Mesh lowerMesh;

    private MeshRenderer upperMeshRenderer;
    private MeshRenderer lowerMeshRenderer;

    private void Start()
    {
        upperMesh = new Mesh();
        UpperTriangle.GetComponent<MeshFilter>().mesh = upperMesh;
        upperMeshRenderer = UpperTriangle.GetComponent<MeshRenderer>();

        lowerMesh = new Mesh();
        LowerTriangle.GetComponent<MeshFilter>().mesh = lowerMesh;
        lowerMeshRenderer = LowerTriangle.GetComponent<MeshRenderer>();
    }

    public void UpdateRep()
    {
        if (poseKeypoins[0].runtimeValue1 == -1)
        {
            //Head
            UpperTriangle.SetActive(false);
            LowerTriangle.SetActive(false);
            return;
        }

        if (poseKeypoins[1].runtimeValue1 == -1)
        {
            //Left arm
            UpperTriangle.SetActive(false);
            LowerTriangle.SetActive(false);
            return;
        }

        if (poseKeypoins[2].runtimeValue1 == -1)
        {
            //Right arm
            UpperTriangle.SetActive(false);
            LowerTriangle.SetActive(false);
            return;
        }
        
        

        Vector3[] vertices = new Vector3[3];

        for (int i = 0; i < arraySize; i++)
        {
            float circleposition = (Mathf.Lerp(0f, (float) cameraDegrees, poseKeypoins[i].runtimeValue2) - 32f) / 360f;
            float x = Mathf.Sin(circleposition * Mathf.PI * 2.0f) * (distanceFromCenter.runtimeValue - 1.0f);
            float z = Mathf.Cos(circleposition * Mathf.PI * 2.0f) * (distanceFromCenter.runtimeValue - 1.0f);

            vertices[i] = new Vector3(x, (1 - poseKeypoins[i].runtimeValue1) * scaling, z);
        }

        lowerMesh.vertices = vertices;

        upperMesh.vertices = vertices;

        lowerMesh.triangles = new int[] {0, 2, 1};

        upperMesh.triangles = new int[] {0, 1, 2};

        lowerMesh.colors = new Color[] {Color.green, Color.green, Color.green};
        
        upperMesh.colors = new Color[] {Color.green, Color.green, Color.green};
        
        lowerMesh.RecalculateNormals();
        
        upperMesh.RecalculateNormals();
        
        lowerMesh.RecalculateBounds();
        
        upperMesh.RecalculateBounds();
        
        Material material = new Material(Shader.Find("Standard"));
        material.color = Color.green;

        lowerMeshRenderer.material = material;

        upperMeshRenderer.material = material;
        
        UpperTriangle.SetActive(true);
        LowerTriangle.SetActive(true);
    }

}
