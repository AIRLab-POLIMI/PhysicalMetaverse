
using System;
using System.Collections;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;


public class StationController : MonoBehaviour
{
    // station manager is the scripts on the station gameobject. 
    // it references the colliders, and if the conditions are met, it triggers the methods of all the components: 
    
    [SerializeField] SphereCollider interactionRangeCollider;
    [SerializeField] SphereCollider activationRangeCollider;
    
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
    
    
    
    
    private Material _holeCoverMaterial;
    
    private bool _isActivated = false;
    private bool _isInteractable = false;
    private bool _isHidden;
    
    // private array of the CoverPetalController
    private CoverPetalController[] _coverPetals;

    private float _startY;
    
    
    [Space] 
    
    [Header("Station")] 

    public bool isRight; //change this boolean to set if station is right or wrong
    
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
        Debug.Log("StationManager: OnInteractableExit");
        
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
        if (isRight)
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
        
        RoutineController.Instance.Activate();
        
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
            bool inRange = Vector3.Magnitude(
                SphereController.Instance.transform.position - transform.position) < interactionRangeCollider.radius * transform.localScale.x;
            
            // if it's in range and it WASN't interactable, trigger interactionEnter
            if (inRange && !_isInteractable)
                OnInteractableEnter();

            // if it's NOT in range and it WAS interactable, trigger interactionExit
            if (!inRange && _isInteractable)
                OnInteractableExit();
        }
        
        private void CheckActivated()
        {
            // if its activated, do nothing
            if (_isActivated)
                return;
            
            // check if the sphere controller is within range of activation (radius of activation controller)
            bool inRange = Vector3.Magnitude(
                SphereController.Instance.transform.position - transform.position) < activationRangeCollider.radius * transform.localScale.x;
            
            // Debug.Log($"CheckActivated - inRange: {inRange} - _isInteractable: {_isInteractable}");
            
            // if it's in range and it's interactable, trigger activated
            if (inRange && _isInteractable)
                OnActivated();
        }
            
        
    #endregion

    public void ResetStation()
    {
        string data = "RESET";
        NetworkingManager.Instance.SendString(data, _stationIp);
    }
    public void CompleteStation()
    {
        //data is "BLINK" in bytes
        string data = "BLINK";

        //key is (char)195
        NetworkingManager.Instance.SendString(data, _stationIp);
        string gamemanagerIp = NetworkingManager.Instance._gameManagerPythonIP;
        //id station isright is false send W:10
        if (!isRight)
        {
            data = "W:10";
            NetworkingManager.Instance.SendString(data, gamemanagerIp);
        }
        //else send R:1
        else
        {
            data = "R:1";
            NetworkingManager.Instance.SendString(data, gamemanagerIp);
            NetworkingManager.Instance._completedStations++;
        }
        //if completed stations is 6 send G:1
        if(NetworkingManager.Instance._completedStations == NetworkingManager.Instance._AMOUNTOFCOMPLETE)
        {
            data = "G:1";
            NetworkingManager.Instance.SendString(data, gamemanagerIp);
            //pannello win lose
            NetworkingManager.Instance.WinPanel(true);
        }
        
    }
    
    public void SetIp(string ip)
    {
        _stationIp = ip;
        ResetStation();
    }
    public string _stationIp;
    
}
