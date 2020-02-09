using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeginGame : MonoBehaviour
{
    public GameObject MusicSystem;
    public GameObject TitleText;

    private Text t;
    private bool fading = false;
    private float fadeTime = 1.25f;

    // Start is called before the first frame update
    void Start()
    {
        t = TitleText.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            fading = true;
            
        }
        else if (!MusicSystem.GetComponent<AudioSource>().isPlaying)
        {
            fading = true;
        }

        if (fading)
        {
            FadeOut(MusicSystem.GetComponent<AudioSource>(), fadeTime);
        }
    }    

    public void FadeOut(AudioSource audioSource, float FadeTime)
    {
        float startVolume = audioSource.volume;
        float startAlpha = t.color.a;

        if (audioSource.volume > 0.01 || t.color.a > 0.01)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
            t.color = new Color(t.color.r, t.color.g, t.color.b, t.color.a - startAlpha * Time.deltaTime / FadeTime);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("AnotherCoffee");
        }
    }
}
