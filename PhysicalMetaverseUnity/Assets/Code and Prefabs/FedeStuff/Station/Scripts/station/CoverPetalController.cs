
using System.Collections;
using UnityEngine;


public class CoverPetalController : MonoBehaviour
{
    [SerializeField] Transform petalTransform;
    [SerializeField] Transform targetTransform;
    
    [SerializeField] GameObject petalMesh;
    
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private Vector3 _startScale;
    private Vector3 _targetScale;

    private Coroutine _currentCoroutine;
    
    private void Awake()
    {
        // get the initial position and rotation of the petal
        _startPosition = petalTransform.localPosition;
        _startRotation = petalTransform.localRotation;
        
        // get the target position and rotation of the petal
        _targetPosition = targetTransform.localPosition;
        _targetRotation = targetTransform.localRotation;
        
        // get the initial scale of the petal
        _startScale = petalTransform.localScale;
        
        // get the target scale of the petal
        _targetScale = targetTransform.localScale;
    }

    public void Hide()
    {
        petalMesh.SetActive(false);
    }
    
    public void Show()
    {
        petalMesh.SetActive(true);
    }
    
    // move to the TARGET position and rotation
    public void OnInteractableEnter() => 
        MoveToTarget(_targetPosition, _targetRotation, _targetScale);
    
    // move to the INITIAL position and rotation
    public void OnInteractableExit() => 
        MoveToTarget(_startPosition, _startRotation, _startScale);
    
    
    // coroutine that moves smoothly the position and orientation of the petal to the target position and orientation
    // the coroutine is called by the PetalsController, which is called by the StationController
    // the coroutine is called when the petal is activated
    private void MoveToTarget(Vector3 targetPosition, Quaternion targetRotation, Vector3 targetScale)
    {
        // Debug.Log("MoveToTarget");
        
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);
        
        _currentCoroutine = StartCoroutine(MoveToTargetCoroutine(targetPosition, targetRotation, targetScale));
    }
    
    private IEnumerator MoveToTargetCoroutine(Vector3 targetPosition, Quaternion targetRotation, Vector3 targetScale)
    {

        Vector3 currentPosition = petalTransform.localPosition;
        Quaternion currentRotation = petalTransform.localRotation;
        Vector3 currentScale = petalTransform.localScale;
        
        // get the initial time
        float elapsedTime = 0f;
        
        // get the duration of the movement
        float movementDuration = 1.0f;
        
        // while the time is less than the duration of the movement
        while (elapsedTime < movementDuration)
        {
            // interpolate position, rotation, and scale smoothly
            float t = elapsedTime / movementDuration;
            petalTransform.localPosition = Vector3.Lerp(currentPosition, targetPosition, Mathf.SmoothStep(0f, 1f, t));
            petalTransform.localRotation = Quaternion.Slerp(currentRotation, targetRotation, Mathf.SmoothStep(0f, 1f, t));
            petalTransform.localScale = Vector3.Lerp(currentScale, targetScale, Mathf.SmoothStep(0f, 1f, t));
            
            // increment the time
            elapsedTime += Time.deltaTime;
            
            // wait for the next frame
            yield return null;
        }
    }
}
