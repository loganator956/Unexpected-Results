using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GUIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public PlayerController pc;

    bool cursorIsOnElement = false;
    public bool IsDraggable = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        cursorIsOnElement = true;
        pc.mouseOverGUI = cursorIsOnElement;
        pc.currentHoveredElement = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        cursorIsOnElement = false;
        pc.mouseOverGUI = cursorIsOnElement;
    }

    void Update()
    {
        if (cursorIsOnElement) { pc.mouseOverGUI = cursorIsOnElement; };
    }
}
