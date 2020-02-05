using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFX : MonoBehaviour
{
    public AudioSource sfxSource;
    public AudioSource ambSource;

    public AudioClip[] sips;
    public AudioClip[] slurps;
    public AudioClip[] typing;

    public AudioClip ambience;
    public AudioClip ambienceLoud;
    public AudioClip menuAccept;
    public AudioClip mouseover;
    public AudioClip dialogueAppear;
    public AudioClip dialogueDisappear;
    public AudioClip interrupt;
    public AudioClip select;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playTypingSound(char letter)
    {
        int index = letter % 26;
        index = index % typing.Length;
        sfxSource.clip = typing[index];
        sfxSource.Play();
    }
}
