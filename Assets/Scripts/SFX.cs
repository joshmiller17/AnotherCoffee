using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFX : MonoBehaviour
{
    public display_state_controller controller;

    public AudioSource sfxSource;
    public AudioSource ambSource;
    public AudioSource typingSource;

    public AudioClip[] sips;
    public AudioClip[] slurps;
    public AudioClip[] typing;

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


    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        ambSource.clip = ambienceLoud;
        ambSource.volume = 0.9f;
        ambSource.Play();
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
        sfxSource.clip = chosenClip;
        sfxSource.Play();
    }

    public void playSlurp()  //use bag to control variance
    {
        if (bagOfSlurps.Count == 0)
        {
            bagOfSlurps = fillClipQueueShuffled(slurps);
        }
        AudioClip chosenClip = bagOfSlurps.Dequeue();
        sfxSource.clip = chosenClip;
        sfxSource.Play();
    }

    public void playTypingSound(char letter)
    {
        int index = letter % 26;
        index = index % typing.Length;
        typingSource.clip = typing[index];
        typingSource.volume = Mathf.Clamp(0.25f + 0.30f * controller.tension, 0, 1);
        typingSource.Play();
    }

    public void playBubbleAppear()
    {
        sfxSource.clip = dialogueAppear;
        sfxSource.Play();
    }

    public void playBubbleDisappear()
    {
        sfxSource.clip = dialogueDisappear;
        sfxSource.Play();
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
        sfxSource.Play();
    }

    public void playMenuAccept()
    {
        sfxSource.clip = menuAccept;
        sfxSource.Play();
    }
}
