using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    int level = 0; // the amount of level 'refreshes' the player has had. 
    int levelPropertyIndex = 0;

    public int Level
    {
        get { return level; }
    }

    public PlayerController pc;

    public GameObject levelGameObject;

    public AnimationCurve exitCurve;
    public AnimationCurve entryCurve;

    [Header("Level Pool")]
    public GameObject[] levels;

    public AudioSource unResBGM;
    public bool[] bgmLevels;

    /*public PuzzlePreset[] puzzlesPresets;*/

    public LevelPrefabInterface.PuzzleType[] puzzleTypes;

    [Header("Texts")]
    public Font font;
    [TextArea]
    public string[] levelNotes = new string[5];

    // Start is called before the first frame update
    void Start()
    {
        
    }

    bool isTransition = false;

    float timer = 0f;

    bool flipFlop = false;

    // Update is called once per frame
    void Update()
    {
        #region Transition
        if (isTransition)
        {
            timer += Time.deltaTime;
            if (timer < 1)
            { 
                // exit
                levelGameObject.transform.localScale = new Vector3(1, exitCurve.Evaluate(timer), 1);
            }
            else if (timer < 2)
            {
                // creating things
                // check if the levelPropertyIndex = -1, if so use the next level prefab
                if (!flipFlop)
                {
                    Destroy(levelGameObject);
                    if (level + 1 < 4)
                    {
                        levelGameObject = Instantiate(levels[0]);
                        levelGameObject.transform.localScale = new Vector3(1, 0, 1);
                        Debug.Log("New level is of tpye 0");
                    }
                    else
                    {
                        levelGameObject = Instantiate(levels[1]);
                        levelGameObject.transform.localScale = new Vector3(1, 0, 1);
                        Debug.Log("New level is of tpye 1");
                        levelPropertyIndex = 0;
                    }
                    flipFlop = true;
                        LevelPrefabInterface lpi = levelGameObject.GetComponent<LevelPrefabInterface>();
                        Color[] colours = lpi.GetLevelLighting();
                        lpi.GetL2D().color = colours[levelPropertyIndex];
                        levelPropertyIndex++;
                        if (levelPropertyIndex >= colours.Length) { levelPropertyIndex = -1; };
                }
            }
            else if (timer < 3)
            {
                // entry
                levelGameObject.transform.localScale = new Vector3(1, entryCurve.Evaluate(timer - 2), 1);
            }
            else
            {
                // finished transition
                flipFlop = false;
                timer = 0;
                isTransition = false;
                levelGameObject.transform.localScale = new Vector3(1, 1, 1);
                pc.SetTargetPos(new Vector3(-3.5f, 0, 0));
                OnNewLevel();
            }
        }
        #endregion
    }

    public void NewLevel()
    {
        isTransition = true;
        timer = 0;
    }

    private void OnNewLevel()
    {
        level++;
        Debug.Log($"OnNewLevel: {level}");
        if (level == 1 && !unResBGM.isPlaying) { unResBGM.Play(); };
        // will be replaced with this below
        /*if (bgmLevels[level] && !unResBGM.isPlaying) { unResBGM.Play(); };*/

        LevelPrefabInterface lvlInterface = levelGameObject.GetComponent<LevelPrefabInterface>();
        /*lvlInterface.SetPuzzle(puzzlesPresets[level]);*/
        /*lvlInterface.InitializePuzzleSteps(LevelPrefabInterface.PuzzleType.Find);*/
        lvlInterface.InitializePuzzleSteps(puzzleTypes[level]);

        // if no note, destroy note
        if (levelNotes[level] == "no")
        {
            InteractableObject[] childrenInteractions = levelGameObject.GetComponentsInChildren<InteractableObject>();
            foreach(InteractableObject i in childrenInteractions)
            {
                if (i.interactableType == InteractableObject.InteractableType.Note) { Destroy(i.gameObject); };
            }
        }
    }

    public string GetLevelNote()
    {
        return levelNotes[level];
    }


    public bool CheckDoorEligibility()
    {
        bool eligible = false;
        if (pc.gameManager.GetComponent<InventoryManager>().Contains(InteractableObject.itemType.Key))
        {
            // contains a key
            eligible = true;
        }
        else
        {
            // doesn't contain key, but check if it is the first level
            if (level == 0)
            {
                eligible = true;
            }
        }
        return eligible;
    }

    /*public bool CheckTransform(Transform t)
    {
        if (levelGameObject.GetComponent<LevelPrefabInterface>().IsTransformPartOfPuzzle(t))
        {
            return true;
        }
        else
        {
            return false;
        }
    }*/
}

/*[Serializable]
public class PuzzlePreset
{
    public PuzzleInfo.PuzzleType Type;
}*/