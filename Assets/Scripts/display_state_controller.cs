using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class display_state_controller : MonoBehaviour
{
	public music_system music;
	public GameObject bg_panel;
	public GameObject dreamer_animation;
	public GameObject realist_animation;
	public GameObject speech_A;
	public GameObject speech_B;
	public GameObject dialogue_A;
	public GameObject dialogue_B;
    public GameObject[] thoughts;
    public GameObject DebugInfo;
	public string dialogue;

	public string current_event_name;
    public double next_event_timer = 10000;
    public double fade_timer = 10000;
    public double fade_rate = 1.5;

    public int awkward;
    public int tension;
    public int resolution;

    public float realist_talking_speed;
    public float dreamer_talking_speed;

    private GameEvent current_event;
    private DisplayState current_display;

    private static double text_to_time_ratio = 1.0 / 20.0;
    private static double fade_time = 1.0;

	

    private int choice_selected = 0;

    public void choose(int choice_id){
    	foreach(GameObject thought in thoughts){
        		hide_thought(thought);
        	}
        if(current_event.is_interrupt){
        	process_json_game_event(current_event.next_event[choice_id + 1]);
        }
        else{
        	choice_selected = choice_id + 1;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        initialize_displays();
        process_json_game_event("0_opening");
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

        Text debugInfo = DebugInfo.GetComponent<Text>();
        debugInfo.text = System.String.Format("Debug info:\nAwkward={0}\nTension={1}\nResolution={2}",
            awkward.ToString("F1"), tension.ToString("F1"), resolution.ToString("F1"));

    }

    void handle_music(){
    	int topic = int.Parse(current_event_name.Substring(0,1));
    	int character = 0;
    	if(current_event.display_state.talking.Equals("dreamer")){
    		character = 1;
    	}
    	float temp_tension = (float)(tension)/2.0f;
    	float temp_resolution = (float)(resolution)/1.0f;
    	float temp_awkwardness = (float)(awkward)/1.0f;
    	music.updateMusic(character, topic, temp_tension, temp_awkwardness, temp_resolution, current_event.is_interrupt);
    }

    string get_next_event_string(){
        if(current_event.next_event != null){
            string temp = current_event.next_event[choice_selected];
            choice_selected = 0;
            return temp;
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
        fade_timer = Time.time + game_event.wait_time + (text_to_time_ratio * game_event.dialogue.Length);
        next_event_timer = fade_timer + fade_time;
        Debug.Log("Fade timer: " + (fade_timer - Time.time).ToString());
        Debug.Log("next event: " + (next_event_timer - Time.time).ToString());
        handle_display(game_event.display_state, game_event.dialogue, game_event.text_speed);
        handle_thoughts(game_event.choices);
        handle_effects(game_event.effects);
    }

    void handle_effects(EffectJSON[] effects){
    	if(effects != null){
    		foreach(EffectJSON effect in effects){
    			if(effect != null){
    				handle_effect(effect);
    			}
    		}
    		handle_music();
    	}
    }

    void handle_effect(EffectJSON effect){
    	if(effect.awkward != null){
    		awkward += effect.awkward;
    	}
    	if(effect.tension != null){
    		awkward += effect.tension;
    	}
    	if(effect.resolution != null){
    		awkward += effect.resolution;
    	}
    }

    void handle_display(DisplayState maybe_display_state, string dialogue, float text_speed){
        DisplayState display_state = maybe_display_state;
        if(maybe_display_state==null){
            display_state = current_display;
        }
        current_display = display_state;
    	if(display_state.dreamer_animation != null) update_image(dreamer_animation, display_state.dreamer_animation);
    	if(display_state.realist_animation != null) update_image(realist_animation, display_state.realist_animation);
    	//update_image(bg_panel, display_state.bg_panel);
    	handle_bubbles(display_state.bubble, display_state.talking, dialogue, text_speed);
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

    void handle_bubbles(Sprite bubble, string talking, string dialogue, float text_speed){
    	if(talking.Equals("realist")){
            speech_A.transform.GetChild(0).GetComponent<FancySpeechBubble>().characterAnimateSpeed = realist_talking_speed * text_speed;
            update_image(speech_A, bubble);
            set_dialogue(dialogue_A, dialogue);
    		show_bubble(speech_A);
    		hide_bubble(speech_B);
    	}
        else if(talking.Equals("dreamer")){
            speech_B.transform.GetChild(0).GetComponent<FancySpeechBubble>().characterAnimateSpeed = dreamer_talking_speed * text_speed;
            update_image(speech_B, bubble);
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
        string talking = current_event.display_state.talking;
        if(talking.Equals("realist")){
            fade_bubble(speech_A);
        }
        else if(talking.Equals("dreamer")){
            fade_bubble(speech_B);
        }
        else{
            fade_bubble(speech_A);
            fade_bubble(speech_B);
        }
    }

    void fade_bubble(GameObject speech){
        speech.GetComponent<Image>().CrossFadeAlpha(0, (float) (fade_time/fade_rate), false);
        speech.transform.GetChild(0).GetComponent<Text>().CrossFadeAlpha(0, (float) (fade_time/fade_rate), false);
    }

    void initialize_displays(){
        hide_bubble_instant(speech_A);
        hide_bubble_instant(speech_B);
        for(int i=0; i<thoughts.Length; i++){
            thoughts[i].GetComponent<thought_behaviors>().choice_id = i;
            thoughts[i].GetComponent<thought_behaviors>().hide_instant();
        }
    }

    void update_image(GameObject obj, Sprite img){
    	obj.GetComponent<Image>().sprite = img;

    }

    string get_next_event_from_name(){
    	return current_event_name + "_I";
    }

    string read_json_file(string path){
        Debug.Log(path);
 		string filePath = "Events/" + path.Replace(".json", "");
		TextAsset targetFile = Resources.Load<TextAsset>(filePath);
		Debug.Log(targetFile.text);
  		return targetFile.text;
   	}

}

public class DisplayState{
	public Sprite bg_panel;
	public Sprite dreamer_animation;
	public Sprite realist_animation;
	public Sprite bubble;
	public string talking;

	static Sprite load_art(string path){
		string filePath = "Art/" + path.Replace(".jpg", "").Replace(".png", "");
		return Resources.Load<Sprite>(filePath);
	}

	public DisplayState(DisplayStateJSON js){
		//if(js==null){
		//	return null;
		//}
		//bg_panel =  load_art(js.bg_panel);
		dreamer_animation = load_art("dreamer_"+js.dreamer_animation);
		realist_animation = load_art("realist_"+js.realist_animation);
		talking = js.talking;
		Debug.Log("bubble_"+talking+"_"+js.bubble);
		bubble = load_art("bubble_"+talking+"_"+js.bubble);
	}

}

public class GameEvent{
	public DisplayState display_state;
	public string[] next_event;
    public string[] choices;
	public float text_speed;
	public string dialogue;
	public bool is_interrupt;
	public int wait_time;
	public EffectJSON[] effects;

	public GameEvent(GameEventJSON js){
		Debug.Log("game event made: " + js.dialogue);
		wait_time = js.wait_time;
		next_event = js.next_event;
        choices = js.choices;
		text_speed = js.text_speed;
		if(text_speed == null || text_speed == 0){
            text_speed = 1;
		}
		dialogue = js.dialogue;
		display_state = new DisplayState(js.display_state);
		is_interrupt = js.is_interrupt;
		if(is_interrupt == null){
			is_interrupt = false;
		}
		effects = js.effects;
	}
}

[System.Serializable]
public class EffectJSON
{
	public int awkward;
	public int tension;
	public int resolution;
}

[System.Serializable]
public class DisplayStateJSON
{
	//public string bg_panel;
	public string dreamer_animation;
	public string realist_animation;
	public string bubble;
	public string talking;
}

[System.Serializable]
public class GameEventJSON
{
	public DisplayStateJSON display_state;
	public EffectJSON[] effects;
	public string dialogue;
	public string[] next_event;
    public string[] choices;
	//public int event_time;
	public int wait_time;
	public bool is_interrupt;
    public bool interrupted;
    public float text_speed;
}
