using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualBump : MonoBehaviour
{
    public VirtualJetson _jetson;
    public bool _colliding = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
    }

    [SerializeField] private bool _lidarMode = false;
    [SerializeField] private float _bumpCooldown = 1f;
    private float _prevBumpTime = 0f;
    //if triggering with something not ground send bump
    void OnTriggerEnter(Collider collision)
    {
        if(_lidarMode){
            if(collision.gameObject.tag == "Pillar"){
                if(Time.time - _prevBumpTime < _bumpCooldown)
                    return;
                _jetson.Send(new byte[]{0x01}, 0xf0);
                //Debug.Log("BUMP");
                _colliding = true;
                _prevBumpTime = Time.time;
            }
        }
        else if (collision.gameObject.tag != "Ground")
            {
                //if collision tag is not FedeHand send
                if(collision.gameObject.tag != "FedeHand"){
                    _jetson.Send(new byte[]{0x01}, 0xf0);
                    //Debug.Log("BUMP");
                    _colliding = true;
                }
            }
    }

    //check trigger collision
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag != "Ground")
        {
            _jetson.Send(new byte[]{0x00}, 0xf0);
            //Debug.Log("END BUMP");
            _colliding = false;
        }
    }


}
