
using System;
using System.Collections;
using UnityEngine;


public static class CoroutineHelper
{
    private const float _defaultRotPlaceholder = -1000.0f;
    
    public static IEnumerator RotateToEulerAngles(Transform targetTransform, 
                                                float duration, 
                                                float xRotTarget = _defaultRotPlaceholder,
                                                float yRotTarget = _defaultRotPlaceholder, 
                                                float zRotTarget = _defaultRotPlaceholder)
    {
        // for each euler angle, if NULL it'll use the current euler angles
        Vector3 targetEulerAngles = targetTransform.rotation.eulerAngles;
            
        // this nasty hack on the CHECK is to avoid strange behaviour resulting from wrong float comparison
        xRotTarget = xRotTarget <= _defaultRotPlaceholder + 0.1f ? targetEulerAngles.x : xRotTarget;
        yRotTarget = yRotTarget <= _defaultRotPlaceholder + 0.1f ? targetEulerAngles.y : yRotTarget;
        zRotTarget = zRotTarget <= _defaultRotPlaceholder + 0.1f ? targetEulerAngles.z : zRotTarget;
        
        // targetEulerAngles =
        //     new Vector3(xRotTarget, yRotTarget, zRotTarget);

        targetEulerAngles = new Vector3(xRotTarget, yRotTarget, zRotTarget);
        
        Quaternion startRotation = targetTransform.rotation;
        float t = 0.0f;

        // Quaternion targetRot = Quaternion.LookRotation(targetEulerAngles);
        Quaternion targetRot = Quaternion.Euler(targetEulerAngles);

        while (t < duration || TrigonometryHelper.QuaternionDifference(targetRot, targetTransform.rotation) > 0.001f)
        // while (t < duration)
        {
            t += Time.deltaTime;
            
            targetTransform.rotation = Quaternion.Slerp(
                startRotation,
                targetRot,
                t / duration);

            yield return null;
        }
    }
    
    
    public static IEnumerator RotateByEulerAngles(Transform targetTransform, 
                                                float duration, 
                                                float xRot = 0.0f, 
                                                float yRot = 0.0f, 
                                                float zRot = 0.0f)
    {
        // compute the euler angles of the targetTransform, add the x, y and z rot values, and convert back to quaternion
        Vector3 targetEulerAngles = targetTransform.rotation.eulerAngles;
        targetEulerAngles =
            new Vector3(targetEulerAngles.x + xRot, targetEulerAngles.y + yRot, targetEulerAngles.z + zRot);
        
        Quaternion startRotation = targetTransform.rotation;
        float t = 0.0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            
            Quaternion targetRot = Quaternion.LookRotation(targetEulerAngles);

            targetTransform.rotation = Quaternion.Slerp(
                startRotation,
                targetRot,
                t / duration);

            yield return null;
        }
    }
    
    
    public static IEnumerator RotateByLocalEulerAngles(Transform targetTransform, 
        float duration, 
        float xRot = 0.0f, 
        float yRot = 0.0f, 
        float zRot = 0.0f)
    {
        // compute the local euler angles of the targetTransform, add the x, y and z rot values, and convert back to quaternion

        Quaternion initialRotation = targetTransform.localRotation;
        
        Vector3 targetEulerAngles = initialRotation.eulerAngles;
        targetEulerAngles =
            new Vector3(targetEulerAngles.x + xRot, targetEulerAngles.y + yRot, targetEulerAngles.z + zRot);
        
        Quaternion targetRot = Quaternion.Euler(targetEulerAngles);
        
        float t = 0.0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            
            targetTransform.localRotation = Quaternion.Slerp(
                initialRotation,
                targetRot,
                t / duration);

            yield return null;
        }
    }
    
    
    public static IEnumerator Rotate (Transform targetTransform, Vector3 targetRotation, float duration)
    {
        Quaternion startRotation = targetTransform.rotation;
        float t = 0.0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            
            Quaternion targetRot = Quaternion.LookRotation(targetRotation);

            targetTransform.rotation = Quaternion.Slerp(
                startRotation,
                targetRot,
                t / duration);

            yield return null;
        }
    }
    
    public static IEnumerator Translate (Transform targetTransform, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = targetTransform.position;
        float t = 0.0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            targetTransform.position = Vector3.Slerp(startPosition, targetPosition, t / duration);
       
            yield return null;
        }
    }
    
    
    #region FADE

    public static IEnumerator FadeOut(SpriteRenderer spriteRenderer, Action OnComplete = null) => 
        Fade(spriteRenderer, 0.0f, OnComplete);


    public static IEnumerator FadeIn(SpriteRenderer spriteRenderer, Action OnComplete = null)
    {
        // set it to Transparent first
        spriteRenderer.material.color = new Color(1, 1, 1, 0);

        return Fade(spriteRenderer, 1.0f, OnComplete);
    }

    
        public static IEnumerator Fade(SpriteRenderer spriteRenderer, float targetAlpha, Action OnComplete = null)
    {
        float startAlpha = spriteRenderer.material.color.a;
        float duration = 1.1f;
        float t = 0.0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            spriteRenderer.material.color = new Color(1, 1, 1, Mathf.Lerp(startAlpha, targetAlpha, t / duration));

            yield return null;
        }

        OnComplete?.Invoke();
    }
    
    #endregion
}
