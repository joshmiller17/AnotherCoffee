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

    private Vector3 min_scale;
    private Vector3 max_scale;
    private Color deselected_color;
    private Color selected_color;
    private bool hidden;
    private bool selected;

    // Start is called before the first frame update
    void Start()
    {
        hidden = true;
        selected = false;

        hide();
    }

    // Update is called once per frame
    void Update()
    {
        //
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.make_selected();
    }
 
    public void OnPointerExit(PointerEventData eventData)
    {
        this.make_deselected();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(selected && !hidden){
            controller.choose(choice_id);
        }
    }

    public void hide(){
        hidden = true;
        gameObject.GetComponent<Image>().CrossFadeAlpha(0, 0.2f, false);
        gameObject.transform.GetChild(0).GetComponent<Text>().CrossFadeAlpha(0, 0.2f, false);
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
