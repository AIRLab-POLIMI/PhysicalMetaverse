
using System.Collections;
using UnityEngine;


public class PetalsController : MonoBehaviour
{   
    // controls all the upper petals of the station
    // petals have 4 states, which are called by this manager which calls it for all the petals
     
    // - not interactable not activated (neutral)
    // - interactable not activated (neutral)
    // - not interactable activated GOOD
    // - not interactable activated WRONG

    // once activated, the petal's state does not change anymore. 

    [Header("RIGHT STATION ACTIVATION ANIMATION PARAMETERS")]
    
        [SerializeField] private float targetGoodAngle = 63;
        [SerializeField] private float minJitterTime = 0.5f;    
        [SerializeField] private float maxJitterTime = 2f;
        [SerializeField] private float minElapsedTime = 0.2f;
        [SerializeField] private float maxElapsedTime = 0.6f;
        [SerializeField] private float minOpeningTime = 1.5f;
        [SerializeField] private float maxOpeningTime = 2f;


    [Header("RIGHT STATION ACTIVATION ANIMATION PARAMETERS")] 
        
        [SerializeField] private float minJitterTimeWrong = 0.5f;    
        [SerializeField] private float maxJitterTimeWrong = 2f;
        [SerializeField] private float minGlitchOffset;
        [SerializeField] private float maxGlitchOffset;
        
        
    private PetalController[] _petals;

    private void Awake()
    {
        // get component in children for the children petals
        _petals = GetComponentsInChildren<PetalController>();
    }

    public void ActivatePetalsGood()
    {
        // activate all the petals in the GOOD state
        float nextWaitTime = 0;
        
        foreach (var petal in _petals)
        {
            petal.ActivateGood(
                nextWaitTime, 
                Random.Range(minJitterTime, maxJitterTime), 
                targetGoodAngle, 
                Random.Range(minOpeningTime, maxOpeningTime));
            
            nextWaitTime += Random.Range(minElapsedTime, maxElapsedTime);
        }
    }

    public void ActivatePetalsWrong()
    {
        foreach (var petal in _petals)
        {
            petal.ActivateWrong(
                Random.Range(minElapsedTime, maxElapsedTime), 
                Random.Range(minJitterTimeWrong, maxJitterTimeWrong), 
                new Vector3(
                    Random.Range(minGlitchOffset, maxGlitchOffset), 
                    Random.Range(minGlitchOffset, maxGlitchOffset), 
                    Random.Range(minGlitchOffset, maxGlitchOffset)), 
                Random.rotation.eulerAngles,
                Random.Range(minOpeningTime, maxOpeningTime));
        }
    }
    
    public void HidePetals()
    {
        foreach (var petal in _petals)
        {
            petal.Hide();
        }
    }
    
    public void ShowPetals()
    {
        foreach (var petal in _petals)
        {
            petal.Show();
        }
    }
}
