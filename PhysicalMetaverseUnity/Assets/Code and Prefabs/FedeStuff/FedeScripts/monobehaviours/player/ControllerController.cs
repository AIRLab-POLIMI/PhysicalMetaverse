using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ControllerController : MonoBehaviour
{
    [SerializeField] string controllerName;
    [SerializeField] private TextMeshProUGUI text;
    
    void Update()
    {
        string msg = "Global angles - " + controllerName + ": " + transform.eulerAngles;
        // log the current global absolute angles
        Debug.Log(msg);
        text.text = msg + "\n";
    }
}
