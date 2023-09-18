using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blink : MonoBehaviour
{
    //blink bool variable
    public bool blink = false;
    //blinked times
    public int blinked = 0;
    public int blinkTime = 30;
    public float blinkMax = 0.8f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //if variable blink is true increase alpha linearly to 0.5 and then go back to zero, twice
        if (blink)
        {
            //if mesh renderer is not esabled enable it
            if (!this.GetComponent<Renderer>().enabled)
            {
                this.GetComponent<Renderer>().enabled = true;
            }
            blinked++;
            if (blinked < blinkTime)
            {
                this.GetComponent<Renderer>().material.color = new Color(this.GetComponent<Renderer>().material.color.r , this.GetComponent<Renderer>().material.color.g, this.GetComponent<Renderer>().material.color.b, blinkMax * blinked / blinkTime);
            }
            else if (blinked < blinkTime * 2)
            {
                this.GetComponent<Renderer>().material.color = new Color(this.GetComponent<Renderer>().material.color.r , this.GetComponent<Renderer>().material.color.g, this.GetComponent<Renderer>().material.color.b, blinkMax * (blinkTime * 2 - blinked) / blinkTime);
            }
            else if (blinked < blinkTime * 3)
            {
                this.GetComponent<Renderer>().material.color = new Color(this.GetComponent<Renderer>().material.color.r , this.GetComponent<Renderer>().material.color.g, this.GetComponent<Renderer>().material.color.b, blinkMax * (blinked - blinkTime * 2) / blinkTime);
            }
            else
            {
                blink = false;
                blinked = 0;
                this.GetComponent<Renderer>().material.color = new Color(this.GetComponent<Renderer>().material.color.r , this.GetComponent<Renderer>().material.color.g, this.GetComponent<Renderer>().material.color.b, 0);
            }
        }
        else{
            //if mesh renderer is not disabled disable it
            if (this.GetComponent<Renderer>().enabled)
            {
                this.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    public void BlinkTwice()
    {
        blink = true;
    }
}
