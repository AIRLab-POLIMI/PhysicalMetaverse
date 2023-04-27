using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BumpCounter : MonoBehaviour
{

    private TextMeshProUGUI bumpText;

    [SerializeField] private IntSO NumOfBumps;
    // Start is called before the first frame update
    void Start()
    {
        bumpText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        bumpText.SetText("Magic Number:" + NumOfBumps.runtimeValue.ToString());
    }
}
