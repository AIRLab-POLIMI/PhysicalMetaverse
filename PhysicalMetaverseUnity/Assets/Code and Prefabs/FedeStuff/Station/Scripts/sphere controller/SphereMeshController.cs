
using System.Collections;
using UnityEngine;


public class SphereMeshController : MonoBehaviour
{
    [SerializeField] private Renderer renderer; // new hides the parent <renderer> property.
    private Material _material;
    Color _emissionColor;
    
    private float _targetBrightness;
    private float _targetScale;
    
    private float _currentScale;
    private float _currentBrightness;
    
    private Color _currentEmissionColor;
    private Coroutine _colourCoroutine;
    private float _startScale;
    
    public bool UpdateColor { get; set; }
    
    
    private void Start()
    {
        _startScale = renderer.transform.localScale.x;
        
        // Gets access to the renderer and material components as we need to
        // modify them during runtime.
        _material = renderer.material;
        
        // Gets the initial emission colour of the material, as we have to store
        // the information before turning off the light.
        _emissionColor = _material.GetColor("_EmissionColor");
        _currentEmissionColor = _emissionColor;
        
        // Enables emission for the material, and make the material use
        // realtime emission.
        _material.EnableKeyword("_EMISSION");
        _material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        
        // hide it
        // Hide();

        _colourCoroutine = null;
        UpdateColor = true;
    }

    public void Init(float initScale, float initBrightness)
    {
        _currentScale = initScale;
        _targetScale = initScale;
        SetScale(initScale);
        
        // Show();
            
        _currentBrightness = initBrightness;
        _targetBrightness = initBrightness;
        
        // Debug.Log($" -----------INIT! initScale: {initScale} - initBrightness: {initBrightness}");
        SetBrightness(initBrightness);
    }

    
    public void Hide()
    {
        renderer.gameObject.SetActive(false);
    }
    
    public void Show()
    {
        renderer.gameObject.SetActive(true);
    }
    
    public void OnInputChanged(float newTargetBrightness, float newTargetScale)
    {
        // Debug.Log($"on input changed: _targetBrightness: {_targetBrightness} - _targetScale: {_targetScale}");
        _targetBrightness = newTargetBrightness;
        _targetScale = newTargetScale;
    }

    private void Update()
    {
        if (Mathf.Abs(_currentScale - _targetScale) > 0.01f)
        {
            _currentScale = Mathf.Lerp(_currentScale, _targetScale, 0.1f);
            SetScale(_currentScale);
            // debug the _currentScale
            // Debug.Log($"_currentScale : {_currentScale} - targetScale : {_targetScale}");
            
        }

        if (!UpdateColor)
            return;
        
        if (Mathf.Abs(_currentBrightness - _targetBrightness) > 0.05f)
        {
            // Debug.Log(" -----------SB!!");
            _currentBrightness = Mathf.Lerp(_currentBrightness, _targetBrightness, 0.1f);
            SetBrightness(_currentBrightness);
        }
    }

    private void SetBrightness(float intensity)
    {
        // Debug.Log(_material.color);
        
        // Update the emission color and intensity of the material.
        _material.SetColor("_EmissionColor", _currentEmissionColor * intensity);

        // Makes the renderer update the emission and albedo maps of our material.
        RendererExtensions.UpdateGIMaterials(renderer);

        // Inform Unity's GI system to recalculate GI based on the new emission map.
        DynamicGI.SetEmissive(renderer, _currentEmissionColor * intensity);
        DynamicGI.UpdateEnvironment();
    }

    public void SetColour(Color newColor)
    {
        _currentEmissionColor = newColor;
        _material.color = newColor;
    }
    
    public void SetScale(float newScale) => 
        renderer.transform.localScale = Vector3.one * _startScale * newScale;
    
    
    public void StartColorLerp(Color startColor, Color targetColor, float initIntensity, float targetIntensity, float duration, bool resumeLerpColorAfter = true)
    {
        if (_colourCoroutine != null)
            StopCoroutine(_colourCoroutine);
        
        _colourCoroutine = StartCoroutine(LerpColor(startColor, targetColor, initIntensity, targetIntensity, duration, resumeLerpColorAfter));
    }
    
    public void StartColorLerp(Color targetColor, float initIntensity, float targetIntensity, float duration, bool resumeLerpColorAfter = true)
    {
        if (_colourCoroutine != null)
            StopCoroutine(_colourCoroutine);
    
        _colourCoroutine = StartCoroutine(LerpColor(_material.color, targetColor, initIntensity, targetIntensity, duration, resumeLerpColorAfter));
    }
        
        
    // coroutine to lerp the colour of the sphere from the current colour to the target colour in a given duration
    private IEnumerator LerpColor(Color startColor, Color targetColor, float initIntensity, float targetIntensity, float duration, bool resumeLerpColorAfter)
    {
        UpdateColor = false;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            Color newColor = Color.Lerp(startColor, targetColor, elapsedTime / duration);
            float newEmission = Mathf.Lerp(initIntensity, targetIntensity, elapsedTime / duration);
            
            SetColour(newColor);
            SetBrightness(newEmission);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _material.color = targetColor;
        _colourCoroutine = null;
        
        UpdateColor = resumeLerpColorAfter;
    }


}
