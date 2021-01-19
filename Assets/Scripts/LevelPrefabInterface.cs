using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class LevelPrefabInterface : MonoBehaviour
{
    [Header("Level Prefab Properties")]
    // If there aren't enough items in the array for the amount of times this prefab will be spawn in, then it'll repeat the first one.
    [SerializeField]
    Color[] LightingColours;

    /*PuzzleInfo currentPuzzle;
    public PuzzleInfo Puzzle
    {
        get { return currentPuzzle; }
        set
        {
            currentPuzzle = value;
            OnPuzzleSet();
        }
    }*/
    
    public Color[] GetLevelLighting()
    {
        return LightingColours;
    }

    public void SetLevelLighting(Color color)
    {
        Light2D l2d = GetL2D();
        l2d.color = color;
    }

    public Light2D GetL2D()
    {
        return transform.GetComponentInChildren<Light2D>();
    }

    public enum PuzzleType
    {
        Find, FindTool, ImageOrientation, PaintingFix
    }

    public void InitializePuzzleSteps(PuzzleType type)
    {
        // get transforms
        Transform[] allChilds = transform.GetComponentsInChildren<Transform>();
        // filter transforms that match tags
        List<Transform> filteredTransforms = new List<Transform>();
        foreach(Transform t in allChilds)
        {
            if (t.tag.ToLower() == "puzzleinteraction")
            {
                filteredTransforms.Add(t);
                Debug.Log($"Adding {t.name} to filteredTransforms");
            }
        }
        int numberOfSteps = 0;
        switch (type)
        {
            case PuzzleType.Find:
                numberOfSteps = filteredTransforms.Count;
                break;
            case PuzzleType.FindTool:
                numberOfSteps = 2; // one step will be to find the tool, the other will be to use the tool to find the key
                break;
            case PuzzleType.ImageOrientation:
                numberOfSteps = 1; // can either be bookshelf or painting (todo)
                break;
            case PuzzleType.PaintingFix:
                return;
                break;
        }
        Debug.Log($"numberOfSteps = {numberOfSteps}");

        // NOw got number of puzzle transforms that we will neeed
        // next step is to choose which Transforms to use

        List<Transform> puzzlePieces = new List<Transform>();
        for (int i = 0; i < numberOfSteps;i++)
        {
            // pick a random transform to do puzzle with
            Transform selected = filteredTransforms[UnityEngine.Random.Range(0, filteredTransforms.Count)];
            puzzlePieces.Add(selected);
            // remove that transform from the pool so it cannot be selected again
            filteredTransforms.Remove(selected);
        }
        // now we have chosen the puzzle transforms
        // gotta deactivate the unused transforms by removing certain components
        foreach(Transform t in filteredTransforms)
        {
            // remove collider
            Destroy(t.GetComponent<BoxCollider2D>());
            // remove interactability
            Destroy(t.GetComponent<InteractableObject>());
        }
        // now need to setup the remaining puzzle pieces and their components/properties
        Transform select;
        InteractableObject interact;
        switch(type)
        {
            case PuzzleType.Find:
                // set all the default properties for each puzzlepiece
                SetDefaultPuzzlePieces(puzzlePieces.ToArray());
                // now pick a random puzzle piece to hide the puzzleitem in
                select = puzzlePieces[UnityEngine.Random.Range(0, puzzlePieces.Count)];
                interact = select.GetComponent<InteractableObject>();
                interact.itemDrop = InteractableObject.itemType.Key;
                break;
            case PuzzleType.FindTool:
                // set all the default properties for each puzzlepiece
                SetDefaultPuzzlePieces(puzzlePieces.ToArray());
                // now pick a random piece to hide the tool in
                select = puzzlePieces[UnityEngine.Random.Range(0, puzzlePieces.Count)];
                puzzlePieces.Remove(select);
                interact = select.GetComponent<InteractableObject>();
                interact.itemDrop = InteractableObject.itemType.PryTool;
                // now pick a random piece to hide the other thing in
                select = puzzlePieces[UnityEngine.Random.Range(0, puzzlePieces.Count)];
                puzzlePieces.Remove(select);
                interact = select.GetComponent<InteractableObject>();
                interact.itemDrop = InteractableObject.itemType.Key;
                interact.itemRequirement = InteractableObject.itemType.PryTool;
                break;
        }
        
    }

    private void SetDefaultPuzzlePieces(Transform[] puzzlePieces)
    {
        foreach (Transform p in puzzlePieces)
        {
            InteractableObject interact = p.GetComponent<InteractableObject>();
            interact.itemDrop = interact.itemRequirement = InteractableObject.itemType.None;
            // set any other default values here
        }
    }

    /*public void SetPuzzle(PuzzlePreset preset)
    {
        PuzzleInfo info = new PuzzleInfo();
        
        info.Type = preset.Type;

        Puzzle = info;
    }*/

    /*void OnPuzzleSet()
    {
        // need to choose what GameObjects in the level to use as part of the puzzle
        Transform[] transforms = transform.GetComponentsInChildren<Transform>();
        List<Transform> filteredTransforms = new List<Transform>();
        foreach(Transform t in transforms)
        {
            if (t.tag.ToLower() == "puzzleinteraction")
            {
                filteredTransforms.Add(t);
            }
        }

        if (filteredTransforms.Count < 2) { throw new Exception("Not enough puzzle trigger pieces"); };
        Puzzle.CurrentPuzzleStage = 0;
        Puzzle.PuzzleTriggerStages = new Transform[UnityEngine.Random.Range(1, 3)];
        for (int i = 0; i < Puzzle.PuzzleTriggerStages.Length; i++)
        {
            Transform t = filteredTransforms[UnityEngine.Random.Range(0, filteredTransforms.Count)];
            Puzzle.PuzzleTriggerStages[i] = t;
            filteredTransforms.Remove(t);
        }
        *//* now got the transforms that are part of the puzzle
         * when the player clicks on one of these transforms, the PlayerController can ask this script "hey, is this transform part of the puzzle or just a random transformrmrm"
         *//*
    }

    public bool IsTransformPartOfPuzzle(Transform t)
    {
        if (Puzzle.PuzzleTriggerStages.Contains<Transform>(t))
        {
            Debug.Log("Contains");
            return true;
        }
        else
        {
            return false;
        }
    }*/
}

/*[Serializable]
public class PuzzleInfo
{ 
    *//* Find         Simply explore the level to find the puzzleitem
     * FindAndUseTool  Like find but requires you to find a tool and then use the tool
     * ImageOrientation     Rotate a sprite to a particular orientation
     * PaintingFix      The painting needs to be rebuilt and once that's done.
     *//*
    public enum PuzzleType
    {
        Find, FindAndUseTool, ImageOrientation, PaintingFix
    }

    public PuzzleType Type;

    int stage;
    public int CurrentPuzzleStage
    {
        get { return stage; }
        set
        {
            stage = value;
            OnPuzzleStageChange();
        }
    }

    public Transform[] PuzzleTriggerStages;

    void OnPuzzleStageChange()
    {
        Debug.Log("Stage Changed: " + stage);
    }
}
*/