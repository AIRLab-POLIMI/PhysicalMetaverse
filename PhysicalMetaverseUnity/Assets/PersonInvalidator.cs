using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonInvalidator : MonoBehaviour
{
    PoseReceiver _poseReceiver;
    Transform _pillarMeshDisabler;
    private float _baseScale;
    private float _initialDisablerScale;
    // Start is called before the first frame update
    void Start()
    {
        _poseReceiver = PoseReceiver.Instance;
        _pillarMeshDisabler = transform.GetChild(0);
        _baseScale = transform.localScale.x;
        _initialDisablerScale = _pillarMeshDisabler.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        float scale = _baseScale / transform.localScale.x;
        _pillarMeshDisabler.localScale = new Vector3(_initialDisablerScale * scale, _initialDisablerScale * scale, _initialDisablerScale * scale);
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
