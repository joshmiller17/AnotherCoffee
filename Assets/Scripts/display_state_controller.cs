using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class display_state_controller : MonoBehaviour
{
	public GameObject bg_panel;
	public GameObject character_A_panel;
	public GameObject character_B_panel;
	public GameObject speech_A;
	public GameObject speech_B;
	public Text dialogue_A;
	public Text dialogue_B;
	public string dialogue;

	public string current_event_name;
	private GameEvent current_event;

	public double next_event_timer = 10000;

    // Start is called before the first frame update
    void Start()
    {
        process_json_game_event("init_event");
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > next_event_timer){
        	//sad
        }
    }

    void process_json_game_event(string path){
    	current_event_name = path;
    	GameEventJSON json = JsonUtility.FromJson<GameEventJSON>(read_json_file(path));
    	current_event = new GameEvent(json);
    	handle_event(new GameEvent(json));
    }

    void handle_event(GameEvent game_event){
    	dialogue_A.text = game_event.dialogue;
    	dialogue_B.text = game_event.dialogue;
    	next_event_timer = Time.time + game_event.event_time;
    	handle_display(game_event.display_state);
    }

    void handle_display(DisplayState display_state){
    	update_image(character_A_panel, display_state.character_A_panel);
    	update_image(character_B_panel, display_state.character_B_panel);
    	update_image(bg_panel, display_state.bg_panel);
    	handle_speech_bubbles(display_state.speech_bubble, display_state.active_speech_bubble);
    }

    void handle_speech_bubbles(Sprite speech_bubble, string active_speech_bubble){
    	if(active_speech_bubble.Equals("A")){
    		update_image(speech_A, speech_bubble);
    		show_bubble(speech_A);
    		hide_bubble(speech_B);
    	}
    	else{
    		update_image(speech_B, speech_bubble);
    		show_bubble(speech_B);
    		hide_bubble(speech_A);
    	}
    	
    }

    void show_bubble(GameObject speech){
    	speech.SetActive(true);
    }

    void hide_bubble(GameObject speech){
    	speech.SetActive(false);
    }

    void update_image(GameObject obj, Sprite img){
    	obj.GetComponent<Image>().sprite = img;

    }

    string get_next_event_from_name(){
    	return "";
    }

    string read_json_file(string path){
 		string filePath = "Events/" + path.Replace(".json", "");
		TextAsset targetFile = Resources.Load<TextAsset>(filePath);
  		return targetFile.text;
   	}

}

public class DisplayState{
	public Sprite bg_panel;
	public Sprite character_A_panel;
	public Sprite character_B_panel;
	public Sprite speech_bubble;
	public string active_speech_bubble;

	static Sprite load_art(string path){
		string filePath = "Art/" + path.Replace(".jpg", "").Replace(".png", "");
		return Resources.Load<Sprite>(filePath);
	}

	public DisplayState(DisplayStateJSON js){
		bg_panel =  load_art(js.bg_panel);
		character_A_panel =  load_art(js.character_A_panel);
		character_B_panel =  load_art(js.character_B_panel);
		speech_bubble =  load_art(js.speech_bubble);
		active_speech_bubble =  js.bg_panel;
	}

}

public class GameEvent{
	public DisplayState display_state;
	public string[] next_event;
	public int event_time;
	public string dialogue;

	public GameEvent(GameEventJSON js){
		next_event = js.next_event;
		event_time = js.event_time;
		dialogue = js.dialogue;
		display_state = new DisplayState(js.display_state);
	}
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
	public string dialogue;
	public string[] next_event;
	public int event_time;
}
