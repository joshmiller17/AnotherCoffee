using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class display_state_controller : MonoBehaviour
{
	public GameObject bg_panel;
	public GameObject character_A_panel;
	public GameObject character_B_panel;
	public GameObject speech_A;
	public GameObject speech_B;
	public string dialogue;

	private string current_event_name;

    // Start is called before the first frame update
    void Start()
    {
        process_json_game_event("init_event");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void process_json_game_event(string path){
    	GameEventJSON g_ev = JsonUtility.FromJson<GameEventJSON>(read_json_file(path));
    }

    string get_next_event_from_name(){
    	return "";//TODO
    }

    string read_json_file(string path){
 		string filePath = "Events/" + path.Replace(".json", "");
		TextAsset targetFile = Resources.Load<TextAsset>(filePath);
  		return targetFile.text;
   	}

}

public class DisplayState{
	public string bg_panel;
	public string character_A_panel;
	public string character_B_panel;
	public string speech_bubble;
	public string active_speech_bubble;
}

public class GameEvent{
	public DisplayState display_state;
	public string[] next_event;
	public int event_time;
}

[System.Serializable]
public class DisplayStateJSON
{
	public string bg_panel;
	public string character_A_panel;
	public string character_B_panel;
	public string speech_bubble;
	public string active_speech_bubble;
}

[System.Serializable]
public class GameEventJSON
{
	public DisplayStateJSON display_state;
	public string[] next_event;
	public int event_time;
}
