using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawShakyText : MonoBehaviour
{
    public Material text_material;
    public GameObject shakeToggle;

    public float timer_max;         /* How many update cycles need to run before the displacement seed gets updated */
    public float offset_amount;     /* How much the shader offsets the map according to the seed, probably fine to leave this as is */
    public float shift_intensity;   /* How far things will get displaced by the shader, should be changed if there are significant changes to text size */
    public bool easy_mode; /*Turn on for a simpler font rendering*/
    public bool shaky_enabled;

    private float timer = 0.0f;
    private float seed = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void toggle()
    {
        shaky_enabled = !shaky_enabled;
        if (shaky_enabled)
        {
            shakeToggle.GetComponent<Text>().text = "Shaky text ON";
        }
        else
        {
            shakeToggle.GetComponent<Text>().text = "Shaky text OFF";
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer++;
        if (timer >= timer_max)
        {
            timer = 0.0f;
            seed += 0.3f; /* This is an arbitrary increase, any amount that isn't too small will produce equally 'jittery' results */
        }
        if (shaky_enabled)
        {
            /* Set values to the shader/material at run-time */
            text_material.SetFloat("_DisplacementSeed", seed);
            text_material.SetFloat("_OffsetAmount", offset_amount);
            text_material.SetFloat("_ShiftIntensity", shift_intensity);
            text_material.SetFloat("_SafeMode", (easy_mode == true ? 1.0f : 0.0f));
        }
        else
        {
            if (shaky_enabled)
            {
                text_material.SetFloat("_OffsetAmount", 0f);
                text_material.SetFloat("_ShiftIntensity", 0f);
            }
        }
    }

    void OnGUI()
    {
        

    }
}
