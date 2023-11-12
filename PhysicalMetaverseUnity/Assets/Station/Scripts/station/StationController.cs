
using System;
using System.Collections;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;


public class StationController : MonoBehaviour
{
    // station manager is the scripts on the station gameobject. 
    // it references the colliders, and if the conditions are met, it triggers the methods of all the components: 
    
    [SerializeField] ColliderController interactionRangeCollider;
    [SerializeField] ColliderController activationRangeCollider;
    
    [SerializeField] HoleCoverController holeCoverController;
    
    [SerializeField] PetalsController petalsController;

    [Header("upper sphere")]
    [SerializeField] private SphereMeshController upperSphere;
    [SerializeField] private Color rightColor;
    [SerializeField] private Color wrongColor;
    [SerializeField] private Color startColor;
    [SerializeField] private float colorChangeDurationRight;
    [SerializeField] private float initBrightness;
    [SerializeField] private float endBrightness;
    [SerializeField] private int numIntermissionWrong;
    [SerializeField] private float colorChangeDurationWrong;
    
    [Space]
    
    [Header("Hole Cover")]
    [SerializeField] private MeshRenderer holeCoverMeshRenderer;
    [SerializeField] private Color activatedHoleCoverColor;
    [SerializeField] private float holeCoverColorChangeDuration;
    [SerializeField] private Material black;

    [Space] 
    
    [Header("Meshes")] 
    
    [SerializeField] private GameObject stationBody;
    [SerializeField] private GameObject stationContainer;
    [SerializeField] private float yOffsetHidden;

    [Space]
    
    [Header("Sound")]
    
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activateRightSound;
    [SerializeField] private AudioClip activateWrongSound;
    [SerializeField] private AudioClip hatchOpenSound;
    [SerializeField] private AudioClip hatchCloseSound;
    
    [Space] 
    
    [Header("Collisions")] 
    
    [SerializeField] private string sphereColliderTag;
    
    [SerializeField] private GameObject _station;
    
    private Material _holeCoverMaterial;
    
    private bool _isActivated = false;
    private bool _isInteractable = false;
    private bool _isHidden;
    
    // private array of the CoverPetalController
    private CoverPetalController[] _coverPetals;

    private float _startY;
    
    
    [Space] 
    
    [Header("Station")] 

    [SerializeField] private bool _isRight; //change this boolean to set if station is right or wrong
    private float _startActivationTime = 0;
    private bool _activationStarted = false;
    private float _activationPermanenceTime = 1f;
    private SphereController _sphereController;
    public void SetRight(bool right){
        _isRight = right;
    }
    
    public void Init()
    {
        _isActivated = false;
        _isInteractable = false;
        
        _startY = transform.position.y;
        
        _holeCoverMaterial = holeCoverMeshRenderer.sharedMaterial;
        
        // get the components on children of type CoverPetalController
        _coverPetals = GetComponentsInChildren<CoverPetalController>();

        _isHidden = false;
        
        petalsController.Init();

        interactionRangeCollider.Init(sphereColliderTag);
        activationRangeCollider.Init(sphereColliderTag);

        _sphereController = SphereController.Instance;

        Hide();
    }

    public void Hide()
    {
        if (_isHidden)
            return;
        
        _isHidden = true;
        
        stationBody.SetActive(false);
        petalsController.HidePetals();
        holeCoverController.Hide();
        upperSphere.Hide();
        
        foreach (var coverPetal in _coverPetals)
        {
            coverPetal.Hide();
        }
        
        // if audiosource is playing, stop it
        // if (audioSource.isPlaying)
        //     audioSource.Stop();
        
        // var pos = transform.position;
        // transform.position = new Vector3(pos.x, _startY + yOffsetHidden, pos.z);
        // _isInteractable = false;
        // stationContainer.SetActive(false);
    }
    
    public void Show()
    {
        if (!_isHidden)
            return;

        _isHidden = false;
        
        stationBody.SetActive(true);
        petalsController.ShowPetals();
        holeCoverController.Show();
        upperSphere.Show();
        
        foreach (var coverPetal in _coverPetals)
        {
            coverPetal.Show();
        }
        
        // var pos = transform.position;
        // transform.position = new Vector3(pos.x, _startY, pos.z);
        
        CheckInteractable();
    }
    
    
    private void Update()
    {
        //check if _station gameobject is active
        /*if(!_station.activeSelf){
            //disable this gameobject
            this.gameObject.SetActive(false);
        }*/
        
        if (_isHidden)
            return;
        
        CheckInteractable();
        CheckActivated();
    }
    
    
    private void OnInteractableEnter()
    {
        Debug.Log("StationManager: OnInteractableEnter");
        
        _isInteractable = true;
        
        upperSphere.Show();
        upperSphere.Init(1, initBrightness);
        upperSphere.SetScale(1);
        
        foreach (var coverPetal in _coverPetals)
        {
            coverPetal.OnInteractableEnter();
        }
        
        holeCoverController.StartBreathing();
        
        // if the audio source is playing, stop it
        if (audioSource.isPlaying)
            audioSource.Stop();
        
        // assign the hatch open sound and play it one shot
        audioSource.clip = hatchOpenSound;
        audioSource.PlayOneShot(audioSource.clip);
    }
    
    private void OnInteractableExit(bool petalsAnimation = true)
    {
        //Debug.Log("StationManager: OnInteractableExit");
        
        _isInteractable = false;

        holeCoverController.StopBreathing();

        if (!petalsAnimation)
            return;
        
        foreach (var coverPetal in _coverPetals)
        {
            coverPetal.OnInteractableExit();
        }
        
        if (_isActivated)
           return; 
        
        // if the audio source is playing, stop it
        if (audioSource.isPlaying)
            audioSource.Stop();

        // assign the hatch close sound and play it one shot
        audioSource.clip = hatchCloseSound;
        audioSource.PlayOneShot(audioSource.clip);
    }

    private void OnActivated()
    {
        Debug.Log("StationManager: OnActivated");

        CompleteStation();
        
        _isActivated = true;

        // if the audio source is playing, stop it
        if (audioSource.isPlaying)
            audioSource.Stop();
        
        // when the player hand enters the collider of the "activate range" AND the station is interactable
        if (_isRight)
        {
            petalsController.ActivatePetalsGood();
            upperSphere.StartColorLerp(rightColor, initBrightness, endBrightness, colorChangeDurationRight, false);
            
            // assign the activateRight sound to the audiosource
            audioSource.clip = activateRightSound;
        }
        else
        {
            petalsController.ActivatePetalsWrong();
            StartCoroutine(WrongSphereAnimation());
            
            // assign the activateWrong sound to the audiosource
            audioSource.clip = activateWrongSound;
        }

        StartCoroutine(LerpHoleCoverColor(activatedHoleCoverColor, holeCoverColorChangeDuration));
        
        // play the audiosource only once then stop
        audioSource.PlayOneShot(audioSource.clip);
        
        //RoutineController.Instance.Activate();
        
        // OnInteractableExit(false);
        OnInteractableExit();
    }

    private IEnumerator LerpHoleCoverColor(Color targetColor, float duration)
    {
        // float t = 0;
        // Color startHoleCoverColor = _holeCoverMaterial.GetColor("BaseColor");
        // while (t < duration)
        // {
        //     t += Time.deltaTime;
        //     _holeCoverMaterial.SetColor("BaseColor", Color.Lerp(startHoleCoverColor, targetColor, 
        //         t / duration));
        //     yield return null;
        // }
        yield return new WaitForSeconds(duration);
        
        holeCoverMeshRenderer.material = black;
    }
    
    private IEnumerator WrongSphereAnimation()
    {
        for (int i = 0; i < numIntermissionWrong; i++)
        {
            upperSphere.StartColorLerp(startColor, wrongColor, initBrightness, endBrightness, colorChangeDurationWrong/3, false);
            yield return new WaitForSeconds(colorChangeDurationWrong / 2);
            upperSphere.StartColorLerp(wrongColor, startColor, initBrightness, endBrightness, colorChangeDurationWrong/3, false);
            yield return new WaitForSeconds(colorChangeDurationWrong / 2);
        }
        upperSphere.StartColorLerp(startColor, wrongColor, initBrightness, endBrightness, colorChangeDurationWrong/3, false);
    }


    #region RANGE

        private void CheckInteractable()
        {
            
            // if its activated, do nothing
            if (_isActivated)
                return;
            
            // check if the sphere controller is within range of interaction (radius of interactable controller)
            bool inRange = interactionRangeCollider.SphereInRange;
            
            // if it's in range and it WASN't interactable, trigger interactionEnter
            if (inRange && !_isInteractable)
                OnInteractableEnter();

            // if it's NOT in range and it WAS interactable, trigger interactionExit
            if (!inRange && _isInteractable)
                OnInteractableExit();
        }

        public void SetActivationPermanenceTime(float time)
        {
            _activationPermanenceTime = time;
        }

        private bool _prevInRange = false;
        private void CheckActivated()
        {
            //if _station IsInvalidated() true return, can't activate a station if it is not valid
            if (_station.GetComponent<SingleStationManager>().IsInvalidated()){
                _activationStarted = false;
                return;
            }
            
            // if its activated, do nothing
            if (_isActivated)
                return;
            
            // check if the sphere controller is within range of activation (radius of activation controller)
            bool inRange = activationRangeCollider.SphereInRange;
            
            // Debug.Log($"CheckActivated - inRange: {inRange} - _isInteractable: {_isInteractable}");
            
            // if it's in range and it's interactable, trigger activated
            if (inRange && _isInteractable){
                _prevInRange = true;
                if (!_activationStarted)
                {
                    _activationStarted = true;
                    _startActivationTime = Time.time;
                    _sphereController.BlinkSphere();
                    RoutineController.Instance.Activate();
                }
                else
                {
                    ChannelDarkEnergy();
                    if (Time.time - _startActivationTime > _activationPermanenceTime)
                    {
                        OnActivated();
                    }
                }
            }
            else
            {
                if (_prevInRange){
                    _prevInRange = false;
                    _sphereController.StopBlink();
                    RoutineController.Instance.Stop();
                }
                _activationStarted = false;
            }
        }
            
    public void ChannelDarkEnergy(){
        Debug.Log("CHANNELING DARK ENERGY... " + (_activationPermanenceTime - (Time.time - _startActivationTime)));
    }

    #endregion

    public void ResetStation()
    {
        string data = "RESET";
        NetworkingManager.Instance.SendString(data, _stationIp);
    }
    public void CompleteStation()
    {
        BlinkStation();
        BlinkStation();
        BlinkStation();
        BlinkStation();
        BlinkStation();
        //IGNORE MESSAGES THAT ARE TOO CLOSE IN GAME MANAGER
        //id station isright is false send W:10
        if (!_isRight)
        {
            StationManager.Instance.CompleteWrongStation();
            StationManager.Instance.CompleteWrongStation();
            StationManager.Instance.CompleteWrongStation();
            StationManager.Instance.CompleteWrongStation();
            StationManager.Instance.CompleteWrongStation();
        }
        //else send R:1
        else
        {
            StationManager.Instance.CompleteRightStation();
            StationManager.Instance.CompleteRightStation();
            StationManager.Instance.CompleteRightStation();
            StationManager.Instance.CompleteRightStation();
            StationManager.Instance.CompleteRightStation();
        }
    }

    [SerializeField] private string _blinkMessage = "BLINK";
    private void BlinkStation(){
        NetworkingManager.Instance.SendString(_blinkMessage, _stationIp);
    }
    
    public void SetIp(string ip)
    {
        _stationIp = ip;
        ResetStation();
    }
    public string _stationIp;
    
}
