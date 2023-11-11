using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeSwitcher : MonoBehaviour
{
    //Gameobject _personCollider
    public GameObject _personCollider;
    //Gameobject _personJumpDistance
    public GameObject _personJumpDistance;
    //Gameobject _virtualCamera
    public GameObject _virtualCamera;
    //Gameobject _virtualLidar
    public GameObject _virtualLidar;
    //Gameobject _simulation
    public GameObject _simulation;
    //gameobject _pillar
    public GameObject _pillar;
    //enum containing DEBUG, SIMULATION, REAL
    public enum Mode
    {
        DEBUG,
        SIMULATION,
        REAL
    }
    public Mode _mode;
    // Start is called before the first frame update
    void Awake()
    {
        //if _mode is DEBUG
        if(_mode == Mode.DEBUG){
            //enable _personCollider mesh
            _personCollider.GetComponent<MeshRenderer>().enabled = true;
            //enable _personJumpDistance mesh
            _personJumpDistance.GetComponent<MeshRenderer>().enabled = true;
            //enable _virtualCamera
            _virtualCamera.SetActive(false);
            //enable _virtualLidar
            _virtualLidar.SetActive(false);
            //disable _simulation
            _simulation.SetActive(true);
            
            _pillar.GetComponent<PillarManager>()._debugMaterial = true;
        }
        else if(_mode == Mode.SIMULATION){
            //enable _personCollider mesh
            _personCollider.GetComponent<MeshRenderer>().enabled = true;
            //enable _personJumpDistance mesh
            _personJumpDistance.GetComponent<MeshRenderer>().enabled = true;
            //enable _virtualCamera
            _virtualCamera.SetActive(true);
            //enable _virtualLidar
            _virtualLidar.SetActive(true);
            //disable _simulation
            _simulation.SetActive(true);
            //_rotate90 of PoseReceiver true
            PoseReceiver.Instance._rotate90 = true;

            _pillar.GetComponent<PillarManager>()._debugMaterial = true;
        }
        else if(_mode == Mode.REAL){
            //disable _personCollider mesh
            _personCollider.GetComponent<MeshRenderer>().enabled = false;
            //disable _personJumpDistance mesh
            _personJumpDistance.GetComponent<MeshRenderer>().enabled = false;
            //disable _virtualCamera
            _virtualCamera.SetActive(false);
            //disable _virtualLidar
            _virtualLidar.SetActive(false);
            //enable _simulation
            _simulation.SetActive(false);
            //disable _pillar
            _pillar.GetComponent<PillarManager>()._debugMaterial = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
