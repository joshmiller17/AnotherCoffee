using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RollCredits : MonoBehaviour
{
    public GameObject Credits;

    private Text creditsText;
    private const float CYCLE_LENGTH = 6.75f; // length of one music beat
    private float cycle_time_left = 6.75f;
    private const float FADE_TIME = 0.5f;
    private const float BLACK_SCREEN_TIME = 1.0f;
    private bool fadingIn = true;
    private int state = 0; // fading in -> on screen -> fading out -> black -> repeat

    private string[] credits_list;
    private int credits_index = 0;

    // Start is called before the first frame update
    void Start()
    {
        creditsText = Credits.GetComponent<Text>();
        TextAsset credits_asset = Resources.Load("Credits") as TextAsset;
        string credits_string = credits_asset.text;
        Debug.Log(credits_string);
        credits_list = credits_string.Split('\n');
        creditsText.text = credits_list[credits_index];
    }

    IEnumerator ColorTransition(Color start, Color end, float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float normalizedTime = t / duration;
            //right here, you can now use normalizedTime as the third parameter in any Lerp from start to end
            creditsText.color = Color.Lerp(start, end, normalizedTime);
            yield return null;
        }
        creditsText.color = end; //without this, the value will end at something like 0.9992367
    }

    // Update is called once per frame
    void Update()
    {
        cycle_time_left -= Time.deltaTime;
        switch (state)
        {
            case 0: // fading in
                Color original = creditsText.color;
                Color end = new Color(original.r, original.g, original.b, 1);
                StartCoroutine(ColorTransition(original, end, FADE_TIME));
                state += 1;
                break;

            case 1: // on screen
                if (cycle_time_left - FADE_TIME - BLACK_SCREEN_TIME < 0)
                {
                    state += 1;
                }
                break;

            case 2: // fading out
                Color white = creditsText.color;
                Color black = new Color(white.r, white.g, white.b, 0);
                StartCoroutine(ColorTransition(white, black, FADE_TIME));
                state += 1;
                break;

            case 3: // black
                if (cycle_time_left < 0)
                {
                    credits_index += 1;
                    state += 1;
                    if (credits_index < credits_list.Length)
                    {
                        state = 0;
                        creditsText.text = credits_list[credits_index];
                        cycle_time_left = CYCLE_LENGTH;
                    }
                }
                break;
           
            default: // out of credits
                break;
        }
    }
}
