using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingText : MonoBehaviour
{
    private TextMeshProUGUI _text;
    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }
    
    public void StartCycle()
    {
        StartCoroutine(CycleDots());
    }

    public IEnumerator CycleDots()
    {
        while (true)
        {
            //Debug.Log("QUI");
            _text.SetText("LOADING");
            yield return new WaitForSeconds(1);
            _text.SetText("LOADING.");
            yield return new WaitForSeconds(1);
            _text.SetText("LOADING..");
            yield return new WaitForSeconds(1);
            _text.SetText("LOADING...");
            yield return new WaitForSeconds(1);
        }
    }
}
