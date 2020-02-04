using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawShakyText : MonoBehaviour
{
    public Material text_material;

    public float timer_max;         /* How many update cycles need to run before the displacement seed gets updated */
    public float offset_amount;     /* How much the shader offsets the map according to the seed, probably fine to leave this as is */
    public float shift_intensity;   /* How far things will get displaced by the shader, should be changed if there are significant changes to text size */

    private float timer = 0.0f;
    private float seed = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
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
    }

    void OnGUI()
    {
        /* Set values to the shader/material at run-time */
        text_material.SetFloat("_DisplacementSeed", seed);
        text_material.SetFloat("_OffsetAmount", .02f);//offset_amount); //TODO FIXME
        Debug.Log("Shaky offset" + offset_amount.ToString());
        text_material.SetFloat("_ShiftIntensity", .035f);//shift_intensity); //TODO FIXME
        Debug.Log("Shaky intense" + shift_intensity.ToString());

    }
}
