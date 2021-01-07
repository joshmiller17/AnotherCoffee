using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class display_state_controller : MonoBehaviour
{
	public music_system music;
    public GameObject SFXSystem;
	public GameObject bg_panel;
	public GameObject dreamer_animation;
	public GameObject realist_animation;
	public GameObject speech_A;
	public GameObject speech_B;
	public GameObject dialogue_A;
	public GameObject dialogue_B;
    public GameObject[] thoughts;
    public GameObject DebugInfo;
    public GameObject OptionsMenu;
    public GameObject textSpeedButton;
    public GameObject TutorialTipBox;
    public GameObject TutorialTipI;
    public GameObject TutorialTipText;
    public string opening_script;
    public float[] thought_color; //FFE7C9
    public float[] interrupt_color; //FD8B8C
    public string dialogue;
    public bool game_paused = false;

	public string current_event_name;
    public double fade_rate = 1.0;

    public float awkward;
    public float tension;
    public float resolution;
    public float resolution_this_event;

    public float realist_talking_speed;
    public float dreamer_talking_speed;

    public GameEvent current_event;

    private DisplayState current_display;

    enum TextSpeed { SLOW, NORMAL, FAST, SKIP };
    private TextSpeed textSpeed = TextSpeed.NORMAL;
    private double text_to_time_ratio = 1.0 / 15.0;
    private static double slow_text_speed = 1.0 / 7.5;
    private static double normal_text_speed = 1.0 / 15.0;
    private static double fast_text_speed = 1.0 / 40.0;
    private static double skip_text_speed = 1.0 / 90.0;
    private static int min_speech_length = 10;
    private double global_wait_time = 1.25;
    private static double global_choice_time = 4.0;
    private static double fade_time = 1.5;
    private static double fade_time_with_choice = 3.0;
    private double talk_timer; //time at which talking finishes
    private double wait_timer; //time at which to waiting finishes
    private double choice_timer; //time at which to choosing finishes
    private double fade_timer; //time at which to fading finishes

    private enum TimerState { TALKING, WAITING, CHOOSING, FADING };
    private TimerState dialogueState = TimerState.TALKING;
    private float text_speed_multiplier = 1f;

    private bool debug_verbose = false;
    private List<string> previous_events = new List<string>();

	

    private int choice_selected = 0;

    public void change_text_speed()
    {
        switch (textSpeed)
        {
            case TextSpeed.SLOW:
                textSpeed = TextSpeed.NORMAL;
                textSpeedButton.GetComponent<Text>().text = "Text Speed NORMAL";
                text_to_time_ratio = normal_text_speed;
                global_wait_time -= 0.6;
                text_speed_multiplier = 1.0f;
                break;
            case TextSpeed.SKIP:
            case TextSpeed.NORMAL:
                textSpeed = TextSpeed.FAST;
                textSpeedButton.GetComponent<Text>().text = "Text Speed FAST";
                text_to_time_ratio = fast_text_speed;
                global_wait_time -= 0.4;
                text_speed_multiplier = 2.5f;
                break;
            case TextSpeed.FAST:
                textSpeed = TextSpeed.SLOW;
                textSpeedButton.GetComponent<Text>().text = "Text Speed SLOW";
                text_to_time_ratio = slow_text_speed;
                text_speed_multiplier = 0.5f;
                global_wait_time += 1.2;
                break;
            default:
                textSpeed = TextSpeed.NORMAL;
                break;
        }
    }

    public void choose(int choice_id){

        game_paused = false;
        OptionsMenu.SetActive(false);

        //play particles
        foreach (Transform child in thoughts[choice_id].transform)
        {
            if (child.GetComponentInParent<ParticleSystem>())
            {
                child.GetComponentInParent<ParticleSystem>().Play();
            }
        }
        
    	foreach(GameObject thought in thoughts){
        		hide_thought(thought);
        	}
        if(current_event.is_interrupt){
        	process_json_game_event(current_event.next_event[choice_id + 1]);
            SFXSystem.GetComponent<SFX>().playInterrupt();
        }
        else{
        	choice_selected = choice_id + 1;
            SFXSystem.GetComponent<SFX>().playMenuAccept();

            //made our choice, move on
            dialogueState = TimerState.FADING;
            fade_timer = Time.time + 0.5;
        }
    }

    public void exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
         Application.OpenURL(webplayerQuitURL);
#else
         Application.Quit();
#endif
    }

    public void toggle_options_menu()
    {
        game_paused = !game_paused;

        if (game_paused)
        {
            OptionsMenu.SetActive(true);
        }
        else
        {
            OptionsMenu.SetActive(false);
        }
    }

    public string pop_events(List<string> l)
    {
        if (l.Count == 0)
        {
            return "0_opening";
        }
        string ret = l[l.Count - 1];
        l.RemoveAt(l.Count - 1);
        return ret;
    }

    // Start is called before the first frame update
    void Start()
    {
        debug_verbose = false;

        bool do_validation = false; //validate by hand for now

        if (do_validation)
        {
            if (validate_scripts(opening_script, "opening script pointer", new Hashtable(), new List<string>())) //debug only, comment out of final game
            {
                Debug.Log("All scripts OK!");
            }
            else
            {
                Debug.LogError("Script validation failed!");
            }
        }

        initialize_displays();
        debug_verbose = true;
        process_json_game_event(opening_script);
        //DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        string speech_state = "";

        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape)) //alternatives for pause
        {
            toggle_options_menu();
        }

#if UNITY_EDITOR
        //CHEAT KEYS
        if (Input.GetKeyDown(KeyCode.N)) //next
        {
            Debug.Log("N");
            textSpeed = TextSpeed.SKIP;
            text_to_time_ratio = skip_text_speed;
            global_wait_time = 0;
            text_speed_multiplier = 6.0f;
        }
        else if (Input.GetKey(KeyCode.N))
        {
            talk_timer -= Time.deltaTime;
            wait_timer -= Time.deltaTime;
            choice_timer -= Time.deltaTime;
            fade_timer -= Time.deltaTime;
        }
        else if (Input.GetKeyDown(KeyCode.M)) //superskip, only use when there's no choice to be made
        {
            talk_timer = 0;
            wait_timer = 0;
            choice_timer = 0;
            fade_timer = 0;
        }
        else if (Input.GetKeyDown(KeyCode.B)) //back
        {
            foreach (GameObject thought in thoughts)
            {
                hide_thought(thought);
            }
            process_json_game_event(pop_events(previous_events), true);
        }
        else if (Input.GetKeyUp(KeyCode.N))
        {
            change_text_speed();
        }
#endif


        if (game_paused)
        {
            talk_timer += Time.deltaTime;
            wait_timer += Time.deltaTime;
            choice_timer += Time.deltaTime;
            fade_timer += Time.deltaTime;
        }
        else
        {

            switch (dialogueState)
            {
                case TimerState.TALKING:
                    speech_state = System.String.Format("Talking for {0:0.##}", talk_timer - Time.time);
                    if (Time.time > talk_timer)
                    {
                        dialogueState = TimerState.WAITING;
                        handle_thoughts(current_event.choices); //trigger non-interrupt choices
                    }
                    break;

                case TimerState.WAITING:
                    speech_state = System.String.Format("Waiting for {0:0.##}", wait_timer - Time.time);

                    if (Time.time > wait_timer)
                    {
                        dialogueState = TimerState.CHOOSING;
                    }
                    break;

                case TimerState.CHOOSING:
                    speech_state = System.String.Format("Choosing for {0:0.##}", choice_timer - Time.time);

                    if (Time.time > choice_timer)
                    {
                        start_fade();
                        dialogueState = TimerState.FADING;
                    }
                    break;

                case TimerState.FADING:
                    speech_state = System.String.Format("Fading for {0:0.##}", fade_timer - Time.time);
                    if (Time.time > fade_timer)
                    {
                        string eventName = get_next_event_string();
                        if (eventName == "endgame")
                        {
                            UnityEngine.SceneManagement.SceneManager.LoadScene("Credits");
                        }
                        else
                        {
                            process_json_game_event(eventName);
                        }
                    }
                    break;
                default:
                    Debug.LogError("Invalid TimerState");
                    break;

            }
        }


        Text debugInfo = DebugInfo.GetComponent<Text>();
        if (!debugInfo.text.Contains("Invalid") && !debugInfo.text.Contains("Error"))
        {
            debugInfo.text = System.String.Format("Debug info:\nAwkward={0}\nTension={1}\nResolution={2}\nScript={3}\nMusic={4}\n{5}",
                awkward.ToString("F1"), tension.ToString("F1"), resolution.ToString("F1"), current_event_name, music.getMusicPlaying(), speech_state);
        }

    }

    // recursive checks that all script paths are valid; returns success
    bool validate_scripts(string script, string calling_script, Hashtable seen, List<string> traversal)
    {
        traversal.Add(script);
        if (script == "endgame" || script== "" /*dummy*/ || script == "3_opening") // TODO remove 3_opening to continue
        {
            return true; //good!
        }

        if (seen.Contains(script))
        {
            int count = (int)seen[script];
            seen[script] = count + 1;
            return true; //FIXME remove this and fix the test below
            //if (traversal.Count > 100)
            //{
            //    Debug.LogWarning("Warning: Potentially infinite loop from scripts: " + string.Join(" -> ", traversal));
            //    return false;
            //}   
        }
        else { 
            seen.Add(script, 1);
        }

        GameEvent gameEvent;

        string jsonData;
        try
        {
           jsonData = read_json_file(script);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error: MISSING script: " + script + " referenced in " + calling_script);
            Debug.Log(e);
            return false;
        }
        try
        {
            GameEventJSON gameEventJSON = JsonUtility.FromJson<GameEventJSON>(jsonData);
            gameEvent = new GameEvent(gameEventJSON);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error: INVALID script: " + script);
            Debug.Log(e);
            return false;
        }


        bool success = true;
        foreach (string s in gameEvent.next_event)
        {
            List<string> trav = new List<string>();
            foreach (string st in traversal){
                trav.Add(st);
            }
            success = success && validate_scripts(s, script, seen, trav);
        }
        return success;
    }

    void handle_music(){
    	int topic = int.Parse(current_event_name.Substring(0,1));
    	int character = 0;
    	if(current_event.display_state.talking.Equals("dreamer")){
    		character = 1;
    	}
    	float temp_tension = (float)(Mathf.Max(tension,0))/2.0f;
    	float temp_resolution = (float)(Mathf.Max(resolution_this_event, 0)) /1.0f;
    	float temp_awkwardness = (float)(Mathf.Max(awkward, 0)) /1.0f;
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

    void process_json_game_event(string path, bool from_previous=false){

        //get rid of old bubbles if they exist
        hide_bubble(speech_A);
        hide_bubble(speech_B);

        if (!from_previous)
        {
            previous_events.Add(current_event_name);
        }
        current_event_name = path;
        try
        {
            GameEventJSON json = JsonUtility.FromJson<GameEventJSON>(read_json_file(path));
            Debug.Log("event: " + json.dialogue);
            current_event = new GameEvent(json);
            handle_event(new GameEvent(json));
        }
        catch (System.Exception e)
        {
            Text debugInfo = DebugInfo.GetComponent<Text>();
            debugInfo.text = "Invalid JSON script: " + path;
            Canvas.ForceUpdateCanvases();
            Debug.Log("Invalid JSON script: " + e.ToString());
            return;
        }
    }

    void handle_tutorial_event(GameEvent game_event)
    {
        if (game_event.tutorial != "")
        {
            TutorialTipBox.GetComponent<Image>().CrossFadeAlpha(1f, 0f, false);
            TutorialTipI.GetComponent<Image>().CrossFadeAlpha(1f, 0f, false);
            TutorialTipText.GetComponent<Text>().CrossFadeAlpha(1f, 0f, false);
            TutorialTipText.GetComponent<Text>().text = game_event.tutorial;
        }
        else
        {
            TutorialTipBox.GetComponent<Image>().CrossFadeAlpha(0f, 1f, false);
            TutorialTipI.GetComponent<Image>().CrossFadeAlpha(0f, 1f, false);
            TutorialTipText.GetComponent<Text>().CrossFadeAlpha(0f, 1f, false);
        }
    }

    void handle_event(GameEvent game_event){
        handle_tutorial_event(game_event);
        talk_timer = Time.time + (text_to_time_ratio * Mathf.Max(game_event.dialogue.Length, min_speech_length));
        wait_timer = talk_timer + game_event.wait_time + global_wait_time;
        choice_timer = wait_timer;
        if (!game_event.is_interrupt && game_event.choices != null && game_event.choices.Length > 1)
        {
            choice_timer += global_choice_time;
            fade_timer = choice_timer + fade_time_with_choice;
        }
        else
        {
            fade_timer = choice_timer + fade_time;
        }
        dialogueState = TimerState.TALKING;
        GetComponent<DrawShakyText>().wobble = game_event.wobble; // set wobble factor
        float text_speed = game_event.text_speed * text_speed_multiplier;
        handle_display(game_event.display_state, game_event.dialogue, text_speed);
        handle_thoughts(game_event.choices);
        handle_effects(game_event.effects);
    }

    void handle_effects(EffectJSON[] effects){
        resolution_this_event = 0;
        if (effects != null){
    		foreach(EffectJSON effect in effects){
    			if(effect != null){
    				handle_effect(effect);
    			}
    		}
    		handle_music();
    	}


        //awkward decays over time
        awkward = Mathf.Max(0, awkward - 0.2f);

        float awkwardVolume = Mathf.Min(1.0f, 0.5f + (awkward / 2.0f));
        SFXSystem.GetComponent<SFX>().UpdateAmbientVolume(awkwardVolume);
    }

#pragma warning disable CS0472
    void handle_effect(EffectJSON effect){
        if (effect.awkward != null)
        {
            awkward += effect.awkward;
        }

        if (effect.tension != null){
    		tension += effect.tension;
            if (effect.tension < 0)
            {
                resolution_this_event -= effect.tension;
            }
            Debug.Log("Tension is now " + tension.ToString());

        }
        // The result of the expression is always the same since a value of this type is never equal to 'null'
        if (effect.resolution != null)
        {

            // The result of the expression is always the same since a value of this type is never equal to 'null'
            resolution += effect.resolution;
            resolution_this_event += effect.resolution;
            Debug.Log("Resolution is now " + resolution.ToString());

        }
    }
#pragma warning restore CS0472

    void handle_display(DisplayState maybe_display_state, string dialogue, float text_speed){
        DisplayState display_state = maybe_display_state;
        if(maybe_display_state==null){
            display_state = current_display;
        }
        current_display = display_state;
    	if(display_state.dreamer_animation != null) update_image(dreamer_animation, display_state.dreamer_animation);
    	if(display_state.realist_animation != null) update_image(realist_animation, display_state.realist_animation);
        if (display_state.dreamer_state == "awkward" || display_state.realist_state == "awkward")
        {
            SFXSystem.GetComponent<SFX>().playSlurp();
        }
        else
        {
            //Debug.Log("Dreamer state: " + display_state.dreamer_state);
            //Debug.Log("Realist state: " + display_state.realist_state);
        }
        //update_image(bg_panel, display_state.bg_panel);
        handle_bubbles(display_state.bubble, display_state.talking, dialogue, text_speed);
    }

    void handle_thoughts(string[] choices){
        if (dialogueState == TimerState.TALKING && !current_event.is_interrupt
            || dialogueState == TimerState.WAITING && current_event.is_interrupt
            )
        {
            return;
        }

        if(choices==null){
            foreach(GameObject thought in thoughts){
                hide_thought(thought);
            }
        }
        else{
            for(int i=0; i<thoughts.Length; i++){
                if(i < choices.Length){
                    if (choices[i] != "")
                    {
                        set_thought(thoughts[i], choices[i]);
                        show_thought(thoughts[i]);

                        if (current_event.is_interrupt)
                        {
                            thoughts[i].GetComponent<Image>().color = new Color(interrupt_color[0], interrupt_color[1], interrupt_color[2], 1.0f);
                        }
                        else
                        {
                            thoughts[i].GetComponent<Image>().color = new Color(thought_color[0], thought_color[1], thought_color[2], 1.0f);
                        }
                    }
                }
                else{
                    hide_thought(thoughts[i]);
                }
            }
        }
    }

    void handle_bubbles(Sprite bubble, string talking, string dialogue, float text_speed){
        if (talking.Equals("realist") && dialogue != ""){
            speech_A.transform.GetChild(0).GetComponent<FancySpeechBubble>().characterAnimateSpeed = realist_talking_speed * text_speed;
            update_image(speech_A, bubble);
            set_dialogue(dialogue_A, dialogue);
    		show_bubble(speech_A);
    		hide_bubble(speech_B);
            SFXSystem.GetComponent<SFX>().playBubbleAppear();
    	}
        else if(talking.Equals("dreamer") && dialogue != "")
        {
            speech_B.transform.GetChild(0).GetComponent<FancySpeechBubble>().characterAnimateSpeed = dreamer_talking_speed * text_speed;
            speech_B.transform.GetChild(0).GetComponent<FancySpeechBubble>().isDreamer = true;
            update_image(speech_B, bubble);
            set_dialogue(dialogue_B, dialogue);
            show_bubble(speech_B);
            hide_bubble(speech_A);
            SFXSystem.GetComponent<SFX>().playBubbleAppear();
        }
    	else{
            if (dialogue == "")
            {
                Debug.Log("Warning: dialogue empty");
            }
            else
            {
                Debug.LogWarning("Warning: neither talking for this bubble: " + dialogue);
            }
    		hide_bubble(speech_A);
    		hide_bubble(speech_B);
            //SFXSystem.GetComponent<SFX>().playBubbleDisappear();
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
        speech.GetComponent<Image>().CrossFadeAlpha(0.0f, (float) (fade_time/fade_rate), false);
        speech.transform.GetChild(0).GetComponent<Text>().CrossFadeAlpha(0.0f, (float) (fade_time/fade_rate), false);
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
        if (debug_verbose)
        {
            Debug.Log(path);
        }
 		string filePath = "Events/" + path.Replace(".json", "");
		TextAsset targetFile = Resources.Load<TextAsset>(filePath);
        if (debug_verbose)
        {
            Debug.Log("Reading " + filePath);
        }
  		return targetFile.text;
   	}

}

public class DisplayState{
	public Sprite bg_panel;
	public Sprite dreamer_animation;
	public Sprite realist_animation;
    public string dreamer_state;
    public string realist_state;
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
        dreamer_state = js.dreamer_animation;
        realist_state = js.realist_animation;
        talking = js.talking;
		bubble = load_art("bubble_"+talking+"_"+js.bubble);
	}

}

public class GameEvent{
	public DisplayState display_state;
	public string[] next_event;
    public string[] choices;
	public float text_speed;
	public string dialogue;
    public string tutorial = "";
	public bool is_interrupt;
	public int wait_time;
    public float wobble;
	public EffectJSON[] effects;

	public GameEvent(GameEventJSON js){
		wait_time = js.wait_time;
		next_event = js.next_event;
        choices = js.choices;
		text_speed = js.text_speed;
		if(text_speed == null || text_speed == 0){
            text_speed = 1;
		}
        wobble = js.wobble;  //deprecated DO NOT USE!
        if (wobble == null || wobble == 0)
        {
            wobble = 1f; //multiplier
        }
		dialogue = js.dialogue;
        if (js.tutorial != null)
        {
            tutorial = js.tutorial;
        }
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
	public float awkward;
	public float tension;
	public float resolution;
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
    public int wobble; //deprecated DO NOT USE!
	public bool is_interrupt;
    public bool interrupted;
    public string tutorial;
    public float text_speed;
}
