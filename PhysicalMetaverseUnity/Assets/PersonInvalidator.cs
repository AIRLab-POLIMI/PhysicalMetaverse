using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonInvalidator : MonoBehaviour
{
    PoseReceiver _poseReceiver;
    // Start is called before the first frame update
    void Start()
    {
        _poseReceiver = PoseReceiver.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //on collision with tag PCA
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("PIA"))
        {
            //if not persondetected set poseinvalidated
            if (!_poseReceiver.GetPersonDetected())
            {
                //set poseinvalidated
                _poseReceiver.SetPoseInvalidated();
            }
        }
    }
}
