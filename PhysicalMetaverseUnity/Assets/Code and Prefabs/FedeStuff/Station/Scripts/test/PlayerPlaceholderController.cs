using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlaceholderController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private float rotateSpeed = 1.0f;
    
    // Update is called once per frame
    void Update()
    {
        // move the attached gameobject along the local forward direction according to the moveSpeed
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += -transform.forward * moveSpeed * Time.deltaTime;
        }
        // rotate the attached gameobject in left and right direction according to the rotateSpeed
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(transform.up, -moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(transform.up, moveSpeed * Time.deltaTime);
        }
    }
}
