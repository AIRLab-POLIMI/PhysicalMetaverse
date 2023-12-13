
using System.Collections;
using UnityEngine;


public class PetalController : MonoBehaviour
{
    
    [Header("Jitter Animation")]
        
        public float maxPositionOffset = 0.01f;
        public float maxRotationOffset = 5f;
        public float maxScaleOffset = 0.5f;
        public float minJitterDuration = 0.005f;
        [Range(0.0f, 1.0f)]
        public float maxJitterDuration = 0.09f;
        
    [Header("Good Activated Animation")]
        
        public float rotationSpeed = 20.0f; // Adjust the rotation speed as needed
        public float smoothness = 2.0f; // Adjust the smoothness of the rotation

    [Header("refs")]
        
        [SerializeField] private GameObject _petalMesh;
    
    
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Vector3 _startScale;
    
    private Quaternion _targetGoodRotation;
    private bool _isRotating = false;

    
    // make it so the jitter OFFSETS have each their runtime counterpart:
    // this counterpart is scaled between the original value and 0, to be able to smooth out and compress the movement if needed. 
    // a method receives as input a value in [0, 1], which scales ALL the jitter offsets.
    // A value of 0 also means the max jitter time is MAX (1 sec)

    private Coroutine _currentCoroutine;
    
    private void Start()
    {
        // Store the initial position, rotation, and scale.
        _startPosition = transform.localPosition;
        _startRotation = transform.localRotation;
        _startScale = transform.localScale;

        // good rotation
        _isRotating = false;
    }


    public void Hide()
    {
        _petalMesh.SetActive(false);
    }
    
    public void Show()
    {
        _petalMesh.SetActive(true);
    }
    
        
    public void ActivateWrong(float waitTime, float jitterTime, Vector3 targetPosOffset, Vector3 targetRot, float glitchDuration) => 
        StartCoroutine(OnActivatedWrong(waitTime, jitterTime, targetPosOffset, targetRot, glitchDuration));
    
    private IEnumerator OnActivatedWrong(float waitTime, float jitterTime, Vector3 targetPosOffset, Vector3 targetRot, float glitchDuration)
    {
        // wait for WaitTime
        yield return new WaitForSeconds(waitTime);
        
        // start the JitterCoroutine and wait for it to end
        _currentCoroutine = StartCoroutine(JitterCoroutine(jitterTime, maxJitterDuration*5, minJitterDuration*2));
        yield return _currentCoroutine;
        
        // snap the petal to target position and target rotation
        transform.localPosition = _startPosition + targetPosOffset;
        transform.localRotation = Random.rotation;
    }
    
    
    public void ActivateGood(float waitTime, float jitterTime, float targetAngle, float rotationDuration) => 
        StartCoroutine(OnActivatedGood(waitTime, jitterTime, targetAngle, rotationDuration));
    
    private IEnumerator OnActivatedGood(float waitTime, float jitterTime, float targetAngle, float rotationDuration)
    {
        // wait for WaitTime
        yield return new WaitForSeconds(waitTime);
        
        // start the JitterCoroutine and wait for it to end
        _currentCoroutine = StartCoroutine(JitterCoroutine(jitterTime, maxJitterDuration, maxJitterDuration*20f));
        yield return _currentCoroutine;
        
        // move the petal from the current position to the target good position
        _targetGoodRotation = Quaternion.Euler(targetAngle, _startRotation.eulerAngles.y, _startRotation.eulerAngles.z); // Create the target rotation
        StartCoroutine(RotateObjectCoroutine(rotationDuration));
    }
    
    private IEnumerator JitterCoroutine(float jitterTime, float minStartMaxJitterElapsed, float maxStartMaxJitterElapsed)
    {
        float startTime = Time.time;
        float jitterElapsed = 0;
        float tempMaxJitterDuration = minStartMaxJitterElapsed;

        // var startPosition = transform.localPosition;
        // var startRotation = transform.localPosition;
        // var startScale = transform.localScale;
        
        while (Time.time - startTime < jitterTime)
        {
            // Generate random target positions, rotations, and scales.
            Vector3 targetPosition = _startPosition + Random.insideUnitSphere * maxPositionOffset;
            Quaternion targetRotation = Random.rotation;
            Vector3 targetScale = _startScale + Random.insideUnitSphere * maxScaleOffset;

            float elapsedTime = 0f;
            float jitterDuration = Random.Range(minJitterDuration, tempMaxJitterDuration);

            // Clamp the target rotation to respect maxRotationOffset.
            float angle = Quaternion.Angle(_startRotation, targetRotation);
            if (angle > maxRotationOffset)
            {
                float t = maxRotationOffset / angle;
                targetRotation = Quaternion.Slerp(_startRotation, targetRotation, t);
            }
            
            while (elapsedTime < jitterDuration)
            {
                // Interpolate position, rotation, and scale smoothly.
                float t = elapsedTime / jitterDuration;
                transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Mathf.SmoothStep(0f, 1f, t));
                transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Mathf.SmoothStep(0f, 1f, t));
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Mathf.SmoothStep(0f, 1f, t));

                elapsedTime += Time.deltaTime;
                jitterElapsed += Time.deltaTime;
                yield return null;
            }
            
            tempMaxJitterDuration = Mathf.Lerp(minStartMaxJitterElapsed, maxStartMaxJitterElapsed, Mathf.SmoothStep(0f, 1f, jitterElapsed/jitterTime));
            
            // Ensure we reach the exact target values at the end of the iteration.
            transform.localPosition = targetPosition;
            transform.localRotation = targetRotation;
            transform.localScale = targetScale;
        }
    }
    
    
    private IEnumerator RotateObjectCoroutine(float rotationDuration)
    {
        float rotationStartTime = Time.time;
        _isRotating = true;
        
        while (Time.time - rotationStartTime < rotationDuration)
        {
            float t = (Time.time - rotationStartTime) / rotationDuration;
            transform.localRotation = Quaternion.Slerp(_startRotation, _targetGoodRotation, t);
            yield return null;
        }

        // Ensure that the rotation ends exactly at the target angle
        transform.localRotation = _targetGoodRotation;
        _isRotating = false;
    }
}
