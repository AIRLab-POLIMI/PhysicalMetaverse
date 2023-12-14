using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QRCodeSetter : MonoBehaviour
{
    //serialize field qr code material
    [SerializeField] private Material _qrCodeMaterial = null;
    // Start is called before the first frame update
    void Start()
    {
        //for each child containing "Plane" in its name set material to _qrCodeMaterial
        foreach(Transform child in transform){
            if(child.name.Contains("Plane")){
                child.GetComponent<Renderer>().material = _qrCodeMaterial;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
