using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
[RequireComponent(typeof(ContentSizeFitter))]
public class FancySpeechBubble : MonoBehaviour {

    const int NORMAL_WIDTH = 550;
    const int SMALL_WIDTH = 225;
    const int NORMAL_HEIGHT = 250;
    const int FONT_SIZE_BIG = 80;
    const int FONT_SIZE_NORMAL = 50;

    /// <summary>
    /// Character start font size.
    /// </summary>
    public int characterStartSize = 1;

    /// <summary>
    /// Character size animate speed.
    /// Unit: delta font size / second
    /// </summary>
    public float characterAnimateSpeed;

    /// <summary>
    /// The bubble background (OPTIONAL).
    /// </summary>
    public Image bubbleBackground;

    /// <summary>
    /// Minimum height of background.
    /// </summary>
    public float backgroundMinimumHeight;

    /// <summary>
    /// Vertical margin (top + bottom) between label and background (OPTIONAL).
    /// </summary>
    public float backgroundVerticalMargin;

    /// <summary>
    /// A copy of raw text.
    /// </summary>
    private string _rawText;
    public string rawText {
        get { return _rawText; }
    }

    /// <summary>
    /// Processed version of raw text.
    /// </summary>
    private string _processedText;
    public string processedText {
        get { return _processedText; }
    }

    /// <summary>
    /// Set the label text.
    /// </summary>
    /// <param name="text">Text.</param>
    public void Set (string text) {
        StopAllCoroutines();
        StartCoroutine(SetRoutine(text));
    }   

    /// <summary>
    /// Set the label text.
    /// </summary>
    /// <param name="text">Text.</param>
    public IEnumerator SetRoutine (string text) 
    {
        _rawText = text;
        TestFit();
        yield return StartCoroutine(CharacterAnimation());
    }

    /// <summary>
    /// Test fit candidate text,
    /// set intended label height,
    /// generate processed version of the text.
    /// </summary>
    private void TestFit () 
    {
        // prepare targets
        Text label = GetComponent<Text>();
        ContentSizeFitter fitter = GetComponent<ContentSizeFitter>();

        // change label alpha to zero to hide test fit
        float alpha = label.color.a;
        //label.color = new Color(label.color.r, label.color.g, label.color.b, 0f);

        // configure fitter and set label text so label can auto resize height
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        label.text = _rawText;

        if (_rawText.Length < 9 || (_rawText.Length < 15 && !System.Text.RegularExpressions.Regex.IsMatch(_rawText, "[a-z][A-Z][0-9]")))
        {
            label.alignment = TextAnchor.UpperCenter;
            label.fontSize = FONT_SIZE_BIG;
        }
        else
        {
            label.alignment = TextAnchor.UpperLeft;
            label.fontSize = FONT_SIZE_NORMAL;
        }

        Canvas.ForceUpdateCanvases(); //this replaces old need for waiting for end of frame

        // need to wait for a frame before label's height is updated
        // yield return new WaitForEndOfFrame();
        // make sure label is anchored to center to measure the correct height
        float totalHeight = label.rectTransform.sizeDelta.y;

        // (OPTIONAL) set bubble background
        if (bubbleBackground != null) {
            bubbleBackground.rectTransform.sizeDelta = new Vector2(
                bubbleBackground.rectTransform.sizeDelta.x, 
                Mathf.Max(totalHeight + backgroundVerticalMargin, backgroundMinimumHeight));
        }

        // now it's time to test word by word
        _processedText = "";
        string buffer = "";
        string line = "";
        int lineCount = 1;
        // yes, sorry multiple spaces
        foreach (string word in _rawText.Split(' '))
        {
            
            buffer += word + " ";
            label.text = _processedText + buffer;
            Canvas.ForceUpdateCanvases(); //this replaces old need for waiting for end of frame
            if (lineCount != label.cachedTextGenerator.lineCount)
            {
                //wraps to new line
                lineCount = label.cachedTextGenerator.lineCount + 1;
                if (line != "")
                {
                    _processedText += line.TrimEnd() + "\n";
                }
                line = word + " ";
            }
            else
            {
                line += word + " ";
            }
        }
        _processedText += line.TrimEnd();
        // prepare fitter and label for character animation
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        label.text = "";
        label.rectTransform.sizeDelta = new Vector2(label.rectTransform.sizeDelta.x, totalHeight);
        label.color = new Color(label.color.r, label.color.g, label.color.b, alpha);
        label.enabled = true;
    }

    private IEnumerator CharacterAnimation () 
    {
        // prepare target
        Text label = GetComponent<Text>();

        // go through character in processed text
        string prefix = "";
        foreach (char c in _processedText.ToCharArray()) {
            // animate character size
            int size = characterStartSize;
            while (size < label.fontSize) {
                size += (int)(Time.deltaTime * characterAnimateSpeed);
                size = Mathf.Min(size, label.fontSize);
                label.text = prefix;//+ "<size=" + size + ">" + c + "</size>";
                yield return new WaitForEndOfFrame();
            }
            prefix += c;
        }

        // set processed text
        label.text = _processedText;
    }

}
