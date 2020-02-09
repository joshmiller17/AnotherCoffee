using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitAfterMusic : MonoBehaviour
{
    public GameObject MusicSystem;

    // Start is called before the first frame update
    void Start()
    {
        MusicSystem.GetComponent<AudioSource>().Play();
        Invoke("doQuit", MusicSystem.GetComponent<AudioSource>().clip.length);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void doQuit()
    {
        // save any game data here
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
