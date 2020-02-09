using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFX : MonoBehaviour
{
    public display_state_controller controller;

    public AudioSource sfxSource;
    public AudioSource ambSource;
    public AudioSource typingSource;
    public AudioSource coffeeSource;

    public AudioClip[] sips;
    public AudioClip[] slurps;
    public AudioClip[] typing;
    public AudioClip[] dreamerTyping;

    public AudioClip ambience; //CURRENTLY UNUSED
    public AudioClip ambienceLoud;
    
    public AudioClip dialogueAppear;
    public AudioClip dialogueDisappear;
    public AudioClip interrupt;
    public AudioClip select; //CURRENTLY UNUSED
    public AudioClip menuAccept;
    public AudioClip mouseover;

    private Queue<AudioClip> bagOfSips = new Queue<AudioClip>();
    private Queue<AudioClip> bagOfSlurps = new Queue<AudioClip>();

    public int dreamerTypingRate;
    private int lettersTilNextDreamerType;

    public int realistTypingRate;
    private int lettersTilNextRealistType;


    // Start is called before the first frame update
    void Start()
    {
        //DontDestroyOnLoad(this.gameObject);
        ambSource.clip = ambienceLoud;
        ambSource.volume = 0.9f;
        ambSource.Play();
        lettersTilNextDreamerType = dreamerTypingRate;
        lettersTilNextRealistType = realistTypingRate;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Queue<AudioClip> fillClipQueueShuffled(AudioClip[] clips) //this shuffles the clips, but I think that's okay
    {
        //randomize array
        for (int i = clips.Length - 1; i > 0; i--)
        {
            int r = Random.Range(0, i);
            AudioClip tmp = clips[i];
            clips[i] = clips[r];
            clips[r] = tmp;
        }

        Queue<AudioClip> queue = new Queue<AudioClip>();
        foreach (AudioClip c in clips){
            queue.Enqueue(c);
        }

        return queue;
    }

    public void playSip() //use bag to control variance
    {
        if (bagOfSips.Count == 0)
        {
            bagOfSips = fillClipQueueShuffled(sips);
        }
        AudioClip chosenClip = bagOfSips.Dequeue();
        coffeeSource.clip = chosenClip;
        coffeeSource.Play();
    }

    public void playSlurp()  //use bag to control variance
    {
        if (bagOfSlurps.Count == 0)
        {
            bagOfSlurps = fillClipQueueShuffled(slurps);
        }
        AudioClip chosenClip = bagOfSlurps.Dequeue();
        coffeeSource.clip = chosenClip;
        coffeeSource.Play();
    }

    public void playTypingSound(char letter)
    {

        int index = letter % 26;
        typingSource.volume = Mathf.Clamp(0.25f + 0.30f * Mathf.Max(controller.tension, 0), 0, 1);
        if (controller.current_event.display_state.talking.Equals("realist"))
        {
            lettersTilNextRealistType -= 1;
            if (lettersTilNextRealistType < 1)
            {
                index = index % typing.Length;
                typingSource.clip = typing[index];
                typingSource.Play();
                lettersTilNextRealistType = realistTypingRate;
            }

        }
        else
        {
            lettersTilNextDreamerType -= 1;
            if (lettersTilNextDreamerType < 1)    //(!System.Text.RegularExpressions.Regex.IsMatch(letter.ToString(), "[a-z][A-Z][0-9]"))
            {
                index = Random.Range(0, typing.Length); //FIXME return to dreamerTyping for other SFX
                typingSource.clip = typing[index]; //FIXME return to dreamerTyping for other SFX
                typingSource.Play();
                lettersTilNextDreamerType = dreamerTypingRate;
            }

        }
}

    public void playBubbleAppear()
    {
        sfxSource.clip = dialogueAppear;
        if (!sfxSource.isPlaying)
        {
            sfxSource.Play();
        }
    }

    public void playBubbleDisappear()
    {
        sfxSource.clip = dialogueDisappear;
        if (!sfxSource.isPlaying)
        {
            sfxSource.Play();
        }
    }
     public void playInterrupt()
    {
        sfxSource.clip = interrupt;
        sfxSource.Play();
    }

    public void playSelect()
    {
        sfxSource.clip = select;
        sfxSource.Play();
    }

    public void playMouseover()
    {
        sfxSource.clip = mouseover;
        if (!sfxSource.isPlaying)
        {
            sfxSource.Play();
        }
    }

    public void playMenuAccept()
    {
        sfxSource.clip = menuAccept;
        sfxSource.Play();
    }
}
