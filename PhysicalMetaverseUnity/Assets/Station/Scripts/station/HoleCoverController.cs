
using System.Collections;
using UnityEngine;


public class HoleCoverController : MonoBehaviour
{
    // coroutine that simulates a continuous breathing movement using scale, from the start scale to the target scale

    [Range(0.8f, 1f)]
    public float targetScaleFactor;
    
    public float maxDuration = 2.0f;

    [SerializeField] private Transform coverMesh;
    
    private Coroutine _breathingCoroutine;
    private Vector3 _startScale;
    private Vector3 _targetScale;
    
    private Vector3 _tempStartScale;
    private Vector3 _tempTargetScale;

    private bool _isBreathing;
    
    
    
    
    private void Start()
    {
        _startScale = coverMesh.localScale;
        _targetScale = _startScale * targetScaleFactor;

        _tempStartScale = _startScale;
        _tempTargetScale = _targetScale;
        
        _isBreathing = false;
    }

    public void Hide()
    {
        coverMesh.gameObject.SetActive(false);
    }
    
    public void Show()
    {
        coverMesh.gameObject.SetActive(true);
    }
    
    public void StopBreathing()
    {
        _isBreathing = false;
    }
    
    public void StartBreathing()
    {
        if (_breathingCoroutine == null)
        {
            _breathingCoroutine = StartCoroutine(Breathe());
        }

        _isBreathing = true;
    }

    IEnumerator Breathe()
    {
        float elapsedTime = 0f;
        // generate a random duration between duration/2 and duration
        float tempDuration = Random.Range(this.maxDuration / 2, this.maxDuration);
        
        while (elapsedTime < tempDuration)
        {
            coverMesh.localScale = Vector3.Lerp(_tempStartScale, _tempTargetScale, elapsedTime / tempDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure that the target scale is reached
        coverMesh.localScale = _tempTargetScale;

        // Reverse the breathing movement by swapping start and target scales
        Vector3 temp = _tempStartScale;
        _tempStartScale = _tempTargetScale;
        _tempTargetScale = temp;

        // Start another breathing cycle if it should
        _breathingCoroutine = null;
        if (_isBreathing)
            StartBreathing();
    }
}
