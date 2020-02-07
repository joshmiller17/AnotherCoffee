using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class thought_behaviors : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	public display_state_controller controller;
    public int choice_id = 0;
    //private static double growth_scale = 1.07;
    private static float base_alpha = 0.8f;

    private bool hidden;
    private bool selected;

    // Start is called before the first frame update
    void Start()
    {
        hidden = false;
        selected = false;
    }

    // Update is called once per frame
    void Update()
    {
        //
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.make_selected();
        controller.SFXSystem.GetComponent<SFX>().playMouseover();
    }
 
    public void OnPointerExit(PointerEventData eventData)
    {
        this.make_deselected();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selected && !hidden)
        {
            controller.choose(choice_id);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }

    public void hide(){
        hidden = true;
        gameObject.GetComponent<Image>().CrossFadeAlpha(0, 0.2f, false);
        gameObject.transform.GetChild(0).GetComponent<Text>().CrossFadeAlpha(0, 0.2f, false);
    }

    public void hide_instant(){
        hidden = true;
        gameObject.GetComponent<Image>().CrossFadeAlpha(0, 0.0f, false);
        gameObject.transform.GetChild(0).GetComponent<Text>().CrossFadeAlpha(0, 0.0f, false);
    }

    public void show(){
        hidden = false;
        if(selected){
            show_selected();
        }
        else{
            show_deselected();
        }
    }

    void make_selected(){
        selected = true;
        if(!hidden){
            show_selected();
        }
    }

    void make_deselected(){
        selected = false;
        if(!hidden){
            show_deselected();
        }
    }

    void show_selected(){
        gameObject.GetComponent<Image>().CrossFadeAlpha(1, 0.2f, false);
        gameObject.transform.GetChild(0).GetComponent<Text>().CrossFadeAlpha(1, 0.2f, false);
    }

    void show_deselected(){
        gameObject.GetComponent<Image>().CrossFadeAlpha(base_alpha, 0.2f, false);
        gameObject.transform.GetChild(0).GetComponent<Text>().CrossFadeAlpha(base_alpha, 0.2f, false);
    }

}
