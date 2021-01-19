using System;
using System.Runtime.InteropServices.ComTypes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Don't fiddle with these")]
    public bool mouseOverGUI = false;
    public GUIElement currentHoveredElement;

    public bool mouseOverInteractable = false;
    public InteractableObject currentHoveredObject;

    bool isShowingNote = false;

    /*bool levelComplete = false;*/

    public GameObject lastPuzzlePrefab;

    [Header("Player Properties")]
    public float moveSpeed = 5f;
    public CursorInfo[] cursorPool; // set in editor

    [Header("Constraints")]
    public Vector3[] playerWalkBounds = new Vector3[2]; // topleft-bottom right corners of the hypothetical box where the player can walk - preventing flying
    public float moveClickThreshold = 0.5f; // maximum area from where the player can click

    [Header("Riddles")]
    [TextArea]
    public string[] riddles;

    [Header("References")]
    public Animator animator;
    public GameObject dragger;
    float defaultDraggerZ;
    public GameObject gameManager;
    public GameObject noteViewerParent;

    CursorInfo currentCursorInf;

    bool levelCompleteMode = false;

    Vector3 targetPosition; // the position where the player will walk to, the player will always try to walk to this position
    float characterZDepth; // don't need to change, will automatically just see it's default Z set in the editor
    float characterDefaultY; 
    // Start is called before the first frame update
    void Start()
    {
        // setting the default values of variables
        targetPosition = transform.position;
        characterZDepth = targetPosition.z;
        characterDefaultY = transform.position.y;

        // setting the default cursor
        try { currentCursorInf = cursorPool[0]; }catch (Exception e) { Debug.Log(e.Message); };

        defaultDraggerZ = dragger.transform.position.z;
    }

    bool tgtSwitch = false;

    float cursorTimer = 0f;
    int cursorIndex = 0;

    bool isDragging = false;
    GameObject originalObject;

    bool cursorChanged = false;

    bool destinationAction = false;
    OnReachDestinationInfo destinationInstructions;

    bool isDraggingPuzzle = false;
    GameObject currentDraggedPuzzlePiece;

    bool paintingFix = false;

    // Update is called once per frame
    void Update()
    {
        #region Update Cursor Animation
        if (cursorChanged)
        {
            cursorTimer = currentCursorInf.animationTiming + 1;
            cursorChanged = false;
        }
        try
        {
            cursorTimer += Time.deltaTime;
            if (cursorTimer > currentCursorInf.animationTiming)
            {
                // change next cursor
                cursorIndex++;
                if (cursorIndex >= currentCursorInf.textures.Length)
                {
                    cursorIndex = 0;
                }
                cursorTimer = 0;
                Cursor.SetCursor(currentCursorInf.textures[cursorIndex], currentCursorInf.hotspot, CursorMode.Auto);
            }
        }
        catch(Exception e) { Debug.Log($" Exception: {e.Message} - Are there any cursors set?"); };
        #endregion

        #region Player Movement
        if (Input.GetKeyDown(KeyCode.Mouse0) && !mouseOverGUI && !isDragging && !mouseOverInteractable && !isShowingNote)
        {
            if (IsVectorInBox(Camera.main.ScreenToWorldPoint(Input.mousePosition), playerWalkBounds))
            {
                // clicked within the walk clickable region
                targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                targetPosition.y = characterDefaultY;
                targetPosition.z = characterZDepth;
                tgtSwitch = false;
                destinationAction = false;
            }
            else
            {
                Debug.Log("Clicked somewhere else, can't walk");
            }
        }

        Vector3 direction = targetPosition - transform.position; // getting the direction vector to the target from player
        direction = direction.normalized; // normalize the vector (get rid of magnitude, just pure direction)

        if (Vector3.Distance(transform.position, targetPosition) > 0.05f) // check if above a particular threshold (might not be important? but maybe, who knows)
        {
            transform.position += direction * moveSpeed * Time.deltaTime; // move in direction towards target
            animator.SetBool("IsMoving", true);
            if (targetPosition.x > transform.position.x) { animator.transform.localScale = new Vector3(1, 1, 1); } else { animator.transform.localScale = new Vector3(-1, 1, 1); };
        }
        else
        {
            animator.SetBool("IsMoving", false);
            if (!tgtSwitch) // making sure it'll only happen once (The first frame it has arrived within the threshold of the target)
            {
                tgtSwitch = true;
                OnReachDestination(targetPosition, destinationInstructions);
            }
        }
        #endregion

        #region GUI Click-n-Drag
        if (mouseOverGUI && currentHoveredElement.IsDraggable)
        {
            // hovered over an element that is draggable
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                // began dragging the element. 
                /* Get element's image and match it to the 'dragger'
                 * Continually update the information needed to do things with it when the player releases 
                 * Detect when player releases and do things with the information
                 */
                isDragging = true;
                originalObject = currentHoveredElement.gameObject;
                dragger.SetActive(true);
                Image i = originalObject.GetComponent<Image>();
                Image di = dragger.GetComponent<Image>();
                di.color = i.color;
                di.sprite = i.sprite;
                di.material = i.material;
            }
        }

        if (isDragging)
        {
            Vector3 newPos = Input.mousePosition;
            newPos.z = defaultDraggerZ;
            dragger.transform.position = newPos;
        }

        if (isDragging && Input.GetKeyUp(KeyCode.Mouse0))
        {
            isDragging = false;
            // finish dragging
            OnDragFinish(Camera.main.ScreenToWorldPoint(Input.mousePosition), originalObject);
            dragger.SetActive(false);
        }
        #endregion

        #region Setting Cursor
        if (/*!mouseOverGUI && */!mouseOverInteractable) { SetCursor("default"); };
        #endregion

        #region Closing the note reader
        if (isShowingNote && Input.GetKeyDown(KeyCode.Mouse0))
        {
            noteViewerParent.SetActive(false);
            noteViewerParent.GetComponentInChildren<Text>().text = "this text should be hidden rn, if it isn't then something's gone wrong";
            isShowingNote = false;
        }
        #endregion

        #region What is cursor overrr?
        if (!mouseOverGUI && mouseOverInteractable && !isShowingNote)
        {
            InteractableObject.InteractableType it = currentHoveredObject.interactableType;
            // doing cursor changing stuff
            switch (it)
            {
                case InteractableObject.InteractableType.Collectable:
                    SetCursor("collectable");
                    break;
                case InteractableObject.InteractableType.Door:
                    /*SetCursor("door");*/
                    // check if door is eligible to open
                    LevelManager manager = gameManager.GetComponent<LevelManager>();
                    bool eligible = manager.CheckDoorEligibility();
                    if (eligible || levelCompleteMode)
                    {
                        SetCursor("door");
                    }
                    else
                    {
                        SetCursor("no");
                    }
                    break;
                case InteractableObject.InteractableType.Note:
                    SetCursor("interact");
                    break;
                case InteractableObject.InteractableType.PuzzleTrigger:
                    SetCursor("interact");
                    break;
            }

            // doing things to do with clicking n stuff
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Debug.Log($"Player clicked {it}");

                switch (it)
                {
                    case InteractableObject.InteractableType.Collectable:
                        /*throw new NotImplementedException("Not done this bit yet");*/
                        targetPosition = new Vector3(currentHoveredObject.transform.position.x + 1, characterDefaultY, characterZDepth);
                        tgtSwitch = false;
                        destinationAction = true;
                        destinationInstructions = new OnReachDestinationInfo();
                        destinationInstructions.DestinationAction = OnReachDestinationInfo.Action.Collect;
                        destinationInstructions.ObjectTransform = currentHoveredObject.transform;
                        break;
                    case InteractableObject.InteractableType.Door:
                        // make the player go runners through the door. 
                        // should check if the player is eligible to go through the door, if yes run through, else run up to it and indicate that it won't work

                        // check if the player has the key in the inventory
                        LevelManager manager = gameManager.GetComponent<LevelManager>();
                        bool eligible = manager.CheckDoorEligibility();
                        if (eligible || levelCompleteMode)
                        {
                            // player eligible to go through door
                            // level has been completed and will be allowed to progress
                            targetPosition = new Vector3(currentHoveredObject.transform.position.x + 2, characterDefaultY, characterZDepth);
                            tgtSwitch = false;
                            destinationAction = true;
                            destinationInstructions = new OnReachDestinationInfo();
                            destinationInstructions.DestinationAction = OnReachDestinationInfo.Action.NewLevel;
                        }
                        else
                        {
                            // level has not been completed
                            targetPosition = new Vector3(currentHoveredObject.transform.position.x - 2, characterDefaultY, characterZDepth);
                            tgtSwitch = false;

                            // show a message that the door is locked
                            throw new NotImplementedException("Door locked message");
                        }
                        break;
                    case InteractableObject.InteractableType.Note:
                        /*Debug.Log("Show note");
                        noteViewerParent.SetActive(true);
                        noteViewerParent.GetComponentInChildren<Text>().text = gameManager.GetComponent<LevelManager>().GetLevelNote();
                        isShowingNote = true;*/
                        targetPosition = new Vector3(currentHoveredObject.transform.position.x, characterDefaultY, characterZDepth);
                        tgtSwitch = false;
                        destinationAction = true;
                        destinationInstructions = new OnReachDestinationInfo();
                        destinationInstructions.DestinationAction = OnReachDestinationInfo.Action.Note;
                        break;
                    case InteractableObject.InteractableType.PuzzleTrigger:
                        // walk to object
                        LevelManager lvlM = gameManager.GetComponent<LevelManager>();
                        bool finalLevel = false;
                        if (lvlM.Level == 4)
                        {
                            finalLevel = true;
                        }

                        if (finalLevel)
                        {
                            if (!paintingFix)
                            {
                                Debug.Log("Spawning Final Puzzle");
                                Destroy(currentHoveredObject.gameObject);
                                GameObject newPuzzle = Instantiate(lastPuzzlePrefab);
                                newPuzzle.transform.position = new Vector3(0, 0, -5);
                                newPuzzle.transform.localScale = new Vector3(1, 1, 1);
                                paintingFix = true;
                            }
                        }
                        else
                        {
                            targetPosition = new Vector3(currentHoveredObject.transform.position.x + 2, characterDefaultY, characterZDepth);
                            tgtSwitch = false;
                            destinationAction = true;
                            destinationInstructions = new OnReachDestinationInfo();
                            destinationInstructions.DestinationAction = OnReachDestinationInfo.Action.Puzzle;
                            destinationInstructions.ObjectTransform = currentHoveredObject.transform;
                        }

                        /*LevelManager lvlMan = gameManager.GetComponent<LevelManager>();
                        *//*Debug.Log("Checking transform for in puzzle "  + lvlMan.CheckTransform(currentHoveredObject.transform));*//*
                        if (lvlMan.CheckTransform(currentHoveredObject.transform))
                        {
                            // the transform selected is part of the puzzle
                            LevelPrefabInterface lvlInterface = lvlMan.levelGameObject.GetComponent<LevelPrefabInterface>();
                            // Get the puzzle type
                            PuzzleInfo.PuzzleType puzzleType = lvlInterface.Puzzle.Type;
                            switch (puzzleType)
                            {
                                case PuzzleInfo.PuzzleType.Find:
                                    Debug.Log("Find");
                                    *//* Check if there is more transforms as part of the puzzle,
                                     *  if yes - drop a tool
                                     *  if no - drop the puzzleitem
                                     *//*
                                    if (lvlInterface.Puzzle.CurrentPuzzleStage == 0 && lvlInterface.Puzzle.PuzzleTriggerStages.Length >=2)
                                    {
                                        // there are more stages to the puzzle, drop tool
                                        Vector3 toolSpawnPos = currentHoveredObject.transform.position;

                                    }
                                    break;

                            }
                        }*/
                        break;
                    case InteractableObject.InteractableType.PuzzleDraggable:
                        isDraggingPuzzle = true;
                        currentDraggedPuzzlePiece = currentHoveredObject.gameObject;
                        break;

                }
            }
        }
        #endregion

        #region Dragging Puzzle Piece
        if (isDraggingPuzzle)
        {
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                isDraggingPuzzle = false;
            }
            else
            {
                Vector3 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                newPos.z = -2f;
                currentDraggedPuzzlePiece.transform.position = newPos;

            }
        }
        #endregion
    }

    public void ToggleLevelCompletion()
    {
        levelCompleteMode = !levelCompleteMode;
    }

    public void SetTargetPos(Vector3 Target)
    {
        targetPosition = new Vector3(Target.x, characterDefaultY, characterZDepth);
        tgtSwitch = false;
        destinationAction = false;
    }

    private void OnDragFinish(Vector3 position, GameObject originalItem)
    {
        Debug.Log($"Finish point: {position}, Original GameObject Name: {originalItem.name}");
        // attempt to use the item, if success, remove the item from inventory, if not just leave it there and move on.
    }

    public bool IsVectorInBox(Vector3 a, Vector3[] box)
    {
        /*Debug.Log($"{a} | {box[0]} , {box[1]}");*/
        bool isIn = false;
        if (a.x > box[0].x && a.x < box[1].x && a.y < box[0].y && a.y > box[1].y) { isIn = true; };
        return isIn;
    }

    /// <summary>
    /// This function will be called when the player reaches the target destination
    /// </summary>
    /// <param name="position"></param>
    private void OnReachDestination(Vector3 position, OnReachDestinationInfo info)
    {
        /*Debug.Log($"Reached Destination {position}");*/
        InventoryManager inventory = gameManager.GetComponent<InventoryManager>();
        animator.SetBool("IsMoving", false);
        if (destinationAction)
        {
            switch (info.DestinationAction)
            {
                case OnReachDestinationInfo.Action.NewLevel:
                    gameManager.GetComponent<LevelManager>().NewLevel();
                    destinationAction = false;
                    inventory = gameManager.GetComponent<InventoryManager>();
                    if (inventory.Contains(InteractableObject.itemType.Key)) { inventory.RemoveItem(InteractableObject.itemType.Key); };
                    break;
                case OnReachDestinationInfo.Action.Message:
                    throw new NotImplementedException("Messages haven't been done yet: Here is the message: " + info.Message);
                    break;
                case OnReachDestinationInfo.Action.Note:
                    Debug.Log("Show note");
                    noteViewerParent.SetActive(true);
                    noteViewerParent.GetComponentInChildren<Text>().text = gameManager.GetComponent<LevelManager>().GetLevelNote();
                    isShowingNote = true;
                    break;
                case OnReachDestinationInfo.Action.Puzzle:
                    Debug.Log($"Puzzle Transform: {info.ObjectTransform.name}");
                    InteractableObject interact = info.ObjectTransform.GetComponent<InteractableObject>();
                    // determine if the player can interact with the object
                    bool qualified = false;
                    if (interact.itemRequirement == InteractableObject.itemType.None) { qualified = true; }
                    else
                    {
                        inventory = gameManager.GetComponent<InventoryManager>();
                        if (inventory.Contains(interact.itemRequirement))
                        {
                            qualified = true;
                        }
                    }
                    if (qualified) { interact.SpawnObject(); };
                    break;
                case OnReachDestinationInfo.Action.Collect:
                    // pickup the collectable
                    InteractableObject intObj = info.ObjectTransform.GetComponent<InteractableObject>();
                    InventoryManager inv = gameManager.GetComponent<InventoryManager>();
                    inv.AddItem(intObj.typeOfItem);
                    Destroy(intObj.gameObject);
                    break;
            }
        }
    }

    /// <summary>
    /// Set the cursor
    /// </summary>
    /// <param name="ci">The texture to set the cursor to</param>
    private void SetCursor(CursorInfo ci)
    {
        currentCursorInf = ci;

        Debug.Log($"Setting cursor {ci.Name}");
    }

    /// <summary>
    /// Set the cursor
    /// </summary>
    /// <param name="name">The name of the cursor to set to</param>
    private void SetCursor(string name)
    {
        /*Debug.Log("SETTING CURSOR " + name);*/
        bool foundCursor = false;
        foreach(CursorInfo ci in cursorPool)
        {
            if (name.ToLower() == ci.Name.ToLower())
            {
                // found cursor
                currentCursorInf = ci;
                foundCursor = true;
                cursorChanged = true;
                break;
            }
        }
        if (foundCursor) { return; };
        throw new Exception($"Couldn't find cursor of name {name}");
    }
}

[System.Serializable]
public class CursorInfo
{
    public string Name;
    public Texture2D[] textures;
    public Vector2 hotspot;
    public float animationTiming;
}

public class OnReachDestinationInfo
{
    public enum Action
    {
        NewLevel, Message, Note, Puzzle, Collect
    }
    public Action DestinationAction;
    public string Message;
    public Transform ObjectTransform;
}