using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureUpdater : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //resize rawimage to size of plane
        this.GetComponent<RawImage>().rectTransform.sizeDelta = new Vector2(this.transform.localScale.x, this.transform.localScale.y);
        //set this gameobject's rawimage as texture of the renderer
        this.GetComponent<Renderer>().material.mainTexture = this.GetComponent<RawImage>().texture;
    }
}
