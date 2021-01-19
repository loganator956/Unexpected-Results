using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUIEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Colours")]
    public Color normalColour;
    public Color hoveredColour;

    public float hoverColourFrequency = 0.5f;
    public AnimationCurve colourCurve;

    enum ButtonStatus
    {
        None, Hovered
    }

    ButtonStatus currentButtonStatus = ButtonStatus.None;

    void Start()
    {
        currentButtonStatus = ButtonStatus.None;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Enter");
        currentButtonStatus = ButtonStatus.Hovered;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Exit");
        currentButtonStatus = ButtonStatus.None;
    }

    bool dirSw = false;
    float timer = 0f;

    void Update()
    {
        if (dirSw) { timer -= Time.deltaTime * hoverColourFrequency; }
        else { timer += Time.deltaTime * hoverColourFrequency; };

        if (timer < 0f) { dirSw = false; } else if (timer > 1f) { dirSw = true; };

        switch (currentButtonStatus)
        {
            case ButtonStatus.None:
                 gameObject.GetComponent<Text>().color = normalColour; 
                break;
            case ButtonStatus.Hovered:
                gameObject.GetComponent<Text>().color = Color.Lerp(normalColour, hoveredColour, colourCurve.Evaluate(Mathf.Clamp(timer, 0f, 1f)));
                break;
        }
    }
}
