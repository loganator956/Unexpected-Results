using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    PlayerController pc;

    public enum InteractableType
    {
        Door, Collectable, Note, PuzzleTrigger, PuzzleDraggable
    }
    public InteractableType interactableType; 

    bool cursorIsOnObject = false;

    public enum itemType
    {
        None, PryTool, Key
    }
    public itemType itemDrop;
    public itemType itemRequirement;

    public ItemType typeOfItem;
    
    void Start()
    {
        pc = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    public void OnPointerEnter(PointerEventData data)
    {
        cursorIsOnObject = true;
        pc.mouseOverInteractable = cursorIsOnObject;
        pc.currentHoveredObject = this;
        /*Debug.Log("YES");*/
    }

    public void OnPointerExit(PointerEventData data)
    {
        cursorIsOnObject = false;
        pc.mouseOverInteractable = cursorIsOnObject;
    }

    public void SpawnObject()
    {
        Debug.Log("Spawning object " + itemDrop.ToString());
        if (itemDrop != itemType.None)
        {
            // we actually spawning something here
            ItemType[] items = pc.gameManager.GetComponent<InventoryManager>().ItemPool;
            List<ItemType> filteredList = new List<ItemType>();
            foreach(ItemType item in items)
            {
                if (item.Type == itemDrop)
                {
                    filteredList.Add(item);
                }
            }
            ItemType chosenType = filteredList[UnityEngine.Random.Range(0, filteredList.Count)];
            GameObject newSpawn = Instantiate(chosenType.Prefab);
            newSpawn.transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
            newSpawn.GetComponent<InteractableObject>().typeOfItem = chosenType;
            Destroy(transform.GetComponent<BoxCollider2D>());
        }
    }


    void Update()
    {
        if (cursorIsOnObject) { pc.mouseOverInteractable = cursorIsOnObject; };
    }
}
