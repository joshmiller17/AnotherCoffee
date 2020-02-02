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
	public GameObject dialogue_A;
	public GameObject dialogue_B;
    public GameObject[] thoughts;
	public string dialogue;

	public string current_event_name;
	private GameEvent current_event;
    private DisplayState current_display;

    private static double text_to_time_ratio = 1.0/15.0;
    private static double fade_time = 3.0;

	public double next_event_timer = 10000;
    public double fade_timer = 10000;

    public void choose(int choice_id){
        process_json_game_event(current_event.next_event[choice_id + 1]);
    }

    // Start is called before the first frame update
    void Start()
    {
        initialize_displays();
        process_json_game_event("init_event");
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > next_event_timer){
        	process_json_game_event(get_next_event_string());
        }
        else if(Time.time > fade_timer){
            start_fade();
        }
    }

    string get_next_event_string(){
        if(current_event.next_event != null){
            return current_event.next_event[0];
        }
        else{
            return get_next_event_from_name();
        }
    }

    void process_json_game_event(string path){
    	current_event_name = path;
    	GameEventJSON json = JsonUtility.FromJson<GameEventJSON>(read_json_file(path));
    	current_event = new GameEvent(json);
    	handle_event(new GameEvent(json));
    }

    void handle_event(GameEvent game_event){
    	next_event_timer = Time.time + fade_time + (text_to_time_ratio * game_event.dialogue.Length * game_event.event_time);
        fade_timer = Time.time + (text_to_time_ratio * game_event.dialogue.Length * game_event.event_time);
    	handle_display(game_event.display_state, game_event.dialogue);
        handle_thoughts(game_event.choices);
    }

    void handle_display(DisplayState maybe_display_state, string dialogue){
        DisplayState display_state = maybe_display_state;
        if(maybe_display_state==null){
            display_state = current_display;
        }
        current_display = display_state;
    	update_image(character_A_panel, display_state.character_A_panel);
    	update_image(character_B_panel, display_state.character_B_panel);
    	update_image(bg_panel, display_state.bg_panel);
    	handle_speech_bubbles(display_state.speech_bubble, display_state.active_speech_bubble, dialogue);
    }

    void handle_thoughts(string[] choices){
        if(choices==null){
            foreach(GameObject thought in thoughts){
                hide_thought(thought);
            }
        }
        else{
            for(int i=0; i<thoughts.Length; i++){
                if(i < choices.Length){
                    set_thought(thoughts[i], choices[i]);
                    show_thought(thoughts[i]);
                }
                else{
                    hide_thought(thoughts[i]);
                }
            }
        }
    }

    void handle_speech_bubbles(Sprite speech_bubble, string active_speech_bubble, string dialogue){
    	if(active_speech_bubble.Equals("A")){
    		//update_image(speech_A, speech_bubble);
            set_dialogue(dialogue_A, dialogue);
    		show_bubble(speech_A);
    		hide_bubble(speech_B);
    	}
        else if(active_speech_bubble.Equals("B")){
            //update_image(speech_B, speech_bubble);
            set_dialogue(dialogue_B, dialogue);
            show_bubble(speech_B);
            hide_bubble(speech_A);
        }
    	else{
    		hide_bubble(speech_A);
    		hide_bubble(speech_B);
    	}
    	
    }

    void set_dialogue(GameObject dialogue_box, string dialogue){
        dialogue_box.GetComponent<FancySpeechBubble>().Set(dialogue);
    }

    void set_thought(GameObject thought, string choice){
        thought.transform.GetChild(0).GetComponent<Text>().text = choice;
    }

    void show_bubble(GameObject speech){
    	speech.GetComponent<Image>().CrossFadeAlpha(1, 0.3f, false);
        speech.transform.GetChild(0).GetComponent<Text>().CrossFadeAlpha(1, 0.3f, false);
    }

    void show_thought(GameObject thought){
        thought.GetComponent<thought_behaviors>().show();
    }

    void hide_thought(GameObject thought){
        thought.GetComponent<thought_behaviors>().hide();
    }

    void hide_bubble(GameObject speech){
        speech.GetComponent<Image>().CrossFadeAlpha(0, 0.1f, false);
        speech.transform.GetChild(0).GetComponent<Text>().CrossFadeAlpha(0, 0.1f, false);
    }

    void hide_bubble_instant(GameObject speech){
        speech.GetComponent<Image>().CrossFadeAlpha(0, 0.0f, false);
        speech.transform.GetChild(0).GetComponent<Text>().CrossFadeAlpha(0, 0.0f, false);
    }

    void start_fade(){
        string active_speech_bubble = current_event.display_state.active_speech_bubble;
        if(active_speech_bubble.Equals("A")){
            fade_bubble(speech_A);
        }
        else if(active_speech_bubble.Equals("B")){
            fade_bubble(speech_B);
        }
        else{
            fade_bubble(speech_A);
            fade_bubble(speech_B);
        }
    }

    void fade_bubble(GameObject speech){
        double fade_rate = 1.5;
        speech.GetComponent<Image>().CrossFadeAlpha(0, (float) (fade_time/fade_rate), false);
        speech.transform.GetChild(0).GetComponent<Text>().CrossFadeAlpha(0, (float) (fade_time/fade_rate), false);
    }

    void initialize_displays(){
        hide_bubble_instant(speech_A);
        hide_bubble_instant(speech_B);
        for(int i=0; i<thoughts.Length; i++){
            thoughts[i].GetComponent<thought_behaviors>().choice_id = i;
        }
    }

    void update_image(GameObject obj, Sprite img){
    	obj.GetComponent<Image>().sprite = img;

    }

    string get_next_event_from_name(){
    	return current_event_name + "_I";
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
		active_speech_bubble =  js.active_speech_bubble;
	}

}

public class GameEvent{
	public DisplayState display_state;
	public string[] next_event;
    public string[] choices;
	public int event_time;
	public string dialogue;

	public GameEvent(GameEventJSON js){
		next_event = js.next_event;
        choices = js.choices;
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
    public string[] choices;
	public int event_time;
}
