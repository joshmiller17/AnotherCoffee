using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class music_system : MonoBehaviour
{
    public AudioMixer MasterMixer;
    public AudioClip[] stingers;
    public string[] MusicNames;
    public bool musicEnabled = true;
    public GameObject musicToggle;

    AudioSource[] audiosources;
    string[] snapshots = { "Dreamer", "Realist" };

    List<float[]> parameters = new List<float[]>();

    private static float PHRASE_LEN = 6.99F;

    float[] phraseLen = { PHRASE_LEN, PHRASE_LEN }; //this determines how long the tracks run for each phrase
    float phraseNum = 0;
    int prevSource = 1;
    // 0 = dreamer
    // 1 = realist
    float prevTime = 0;
    int prevClip = 32;
    int twoCipsAgo = 0;

    bool interrupt = false;

    private int currentIndex;
    /*
    Each parameter is represented by these variables.

    To indicate the current player speaking:
        character
        0 = Realist (player character)
        1 = Dreamer

    To indicate the current topic:
        topic
        0 = introduction
        1 = careers
        2 = childhood memories
        3 = relationships

    For the conversation's tension:
        tension
        0 = low tension
        1 = high tension

    For the conversation's awkwardness:
        awkwardness
        0 = low awkwardness
        1 = high awkwardness

    For the resolution between the two characters:
        resolution
        0 = low resolution
        1 = high resolution

    To indicate if there was an interruption:
        interrupt
        false = no interruption (duh)
        true = interruption
    */

    public void updateMusic(int character, int topic,
                            float tension, float awkwardness,
                            float resolution, bool newInterrupt){

        if (topic == 1)
        {
            Debug.Log("Missing Topic 1 music, using Topic 0 instead");
            topic = 0; //FIXME no music for topic 1 yet, so repurpose topic 0
        }

        parameters.Add(new float[] {
                //safety first
                Mathf.Clamp(character, 0, 1),
                Mathf.Clamp(topic, 0, 3),
                Mathf.Clamp(tension, 0, 100),
                Mathf.Clamp(awkwardness, 0, 100),
                Mathf.Clamp(resolution, 0, 100),
                (float)(newInterrupt ? 1 : 0)
            });

        interrupt = newInterrupt;
        Debug.Log("Param list " + parameters.Count.ToString()); // test
    }

    public string getMusicPlaying()
    {
        return stingers[currentIndex].name;
        //return MusicNames[currentIndex];
    }

    int fetchCue(float[] newParams) {
        int index = 0;
        index += (int)newParams[0]*16; //character
        float biggest = 0;
        int tempIndex = 0;
        for (int i = 0; i < 3; i++){
            if (newParams[i + 2] > biggest){
                biggest = newParams[i + 2];
                tempIndex = i;
            }
        }

        if (newParams[tempIndex + 2] >= 0.5){ //threshold to change tone
            index += (int)(tempIndex + 1)*4;
        }

        index += (int)newParams[1]; //topic
        Debug.Log("Considered queueing track: " + index.ToString());
        return index;
    }

    void Start() {
        //DontDestroyOnLoad(this.gameObject);

        audiosources = GetComponents<AudioSource>();
        audiosources[0].loop = true;
        audiosources[1].loop = true;
        // updateMusic(0, 2, 0, 0, 1, false);
        // updateMusic(1, 2, 0, 1, 0, false);
        // updateMusic(0, 2, 0, 0, 1, false);
        // updateMusic(1, 2, 0, 0, 1, false);
        // updateMusic(0, 2, 0, 0, 1, false);
        prevTime = -10;
    }

    public void toggleMusicEnabled()
    {
        musicEnabled = !musicEnabled;

        if (musicEnabled)
        {
            audiosources[0].volume = 0.5f;
            audiosources[1].volume = 0.5f;
            musicToggle.GetComponent<Text>().text = "Music ON";
        }
        else
        {
            audiosources[0].volume = 0.0f;
            audiosources[1].volume = 0.0f;
            musicToggle.GetComponent<Text>().text = "Music OFF";
        }
    }

    void Update() {

        float delay = 1.2F;
        while (parameters.Count > 2)
        {
            Debug.Log("Too many music cues, paring down");
            parameters.RemoveAt(1); // skip ahead to the most recent cue
        }
        if (parameters.Count > 0 || phraseNum < 4){ //added phrasenum < 4, continue existing track
            if (parameters.Count == 0)
            {
                //continue track
                if (Time.time - prevTime >= phraseLen[prevSource] - delay)
                {
                    prevTime = Time.time;
                    phraseNum = (phraseNum + 1) % 4;
                }
                return;
            }
            if (interrupt) {
                int currentSource = (int)parameters[parameters.Count - 1][0];
                currentIndex = fetchCue(parameters[parameters.Count - 1]);
                audiosources[currentSource].clip = stingers[currentIndex];
                audiosources[currentSource].PlayDelayed(3);
                // audiosources[prevSource].Stop();
                prevSource = currentSource;
                parameters.Clear();
                interrupt = false;
                prevTime = Time.time + 3;
                MasterMixer.FindSnapshot(snapshots[currentSource]).TransitionTo(0.5F);
            } else {
                //if the previous cue is at the end of its phrase

                if (Time.time - prevTime >= phraseLen[prevSource] - delay) {
                    Debug.Log("Prev clip is " + prevClip.ToString());
                    int currentSource = (int)parameters[0][0];
                    twoCipsAgo = prevClip;
                    prevClip = currentIndex;
                    currentIndex = fetchCue(parameters[0]);

                    if (currentSource == prevSource && (currentIndex == prevClip
                        || (System.Array.IndexOf(new int[] {0, 1}, currentIndex) != -1 &&
                            System.Array.IndexOf(new int[] {0, 1}, prevClip) == -1
                        ))) // don't switch to neutral intro if something better is going on
                    {
                        Debug.Log("Continuing current track");
                        prevTime = Time.time;
                        parameters.RemoveAt(0);
                    }
                    else {
                        MasterMixer.FindSnapshot(snapshots[currentSource]).TransitionTo(delay);
                        audiosources[currentSource].clip = stingers[currentIndex];
                        float tempTime = (phraseLen[currentSource]*phraseNum) - delay;
                        if (tempTime < 0){
                            tempTime += audiosources[currentSource].clip.length;
                        }
                        audiosources[currentSource].time = tempTime;
                        phraseNum = (phraseNum + 1) % 4;
                        audiosources[currentSource].Play();
                        prevSource = currentSource;
                        parameters.RemoveAt(0);
                        prevTime = Time.time + delay;
                        MasterMixer.FindSnapshot(snapshots[currentSource]).TransitionTo(delay);
                    }
                }
            }
        }else{
            if (Time.time - prevTime >= phraseLen[prevSource] - delay){
                MasterMixer.FindSnapshot("fade").TransitionTo(delay);
                prevClip = -1;
            }
        }
    }

}
