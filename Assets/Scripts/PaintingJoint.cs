using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingJoint : MonoBehaviour
{
    public int ID;
    public int PartnerID;

    PaintingRebuildPuzzle puzzleManager;

    private void Start()
    {
        puzzleManager = transform.parent.parent.GetComponent<PaintingRebuildPuzzle>();
    }

    public enum JointStatus
    {
        Disconnected, Connected
    }

    public JointStatus Status;
}
