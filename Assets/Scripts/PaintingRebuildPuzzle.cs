using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PaintingRebuildPuzzle : MonoBehaviour
{
    PlayerController pc;
    PaintingJoint[] joints;
    // Start is called before the first frame update
    void Start()
    {
        pc = GameObject.Find("Player").GetComponent<PlayerController>();
        joints = transform.GetComponentsInChildren<PaintingJoint>();


        // randomise the points
        List<Vector3> oldPosList = new List<Vector3>();
        List<Transform> paintingPieces = new List<Transform>();
        foreach (PaintingJoint joint in joints)
        {
            if (!paintingPieces.Contains(joint.transform.parent)) { paintingPieces.Add(joint.transform.parent); };
        }
        foreach(Transform t in paintingPieces)
        {
            oldPosList.Add(t.position);
        }
        foreach(Transform t in paintingPieces)
        {
            Vector3 newPos = oldPosList[UnityEngine.Random.Range(0, oldPosList.Count)];
            oldPosList.Remove(newPos);
            t.transform.position = newPos;
        }
    }
    bool foundDisconnected = false;
    // Update is called once per frame
    void Update()
    {
        foundDisconnected = false;
        foreach(PaintingJoint joint in joints)
        {
            Transform t1 = joint.transform;
            Transform t2 = GetJointTargetTransform(joint);
            float distance = Vector3.Distance(t1.position, t2.position);
            if (distance < 0.1f)
            {
                joint.Status = PaintingJoint.JointStatus.Connected;
                if (!Input.GetKey(KeyCode.Mouse0))
                {
                    /* Get which side needs to snap to which 
                     * Do that by determining which piece has the least already attached (Like proper)
                     */
                    int t1Total = 0;
                    int t2Total = 0;
                    PaintingJoint[] t1Joints = t1.GetComponentsInChildren<PaintingJoint>();
                    foreach(PaintingJoint j in t1Joints)
                    {
                        if (Vector3.Distance(j.transform.position, GetJointTargetTransform(j).position) < 0.02f)
                        {
                            t1Total++;
                        }
                    }
                    PaintingJoint[] t2Joints = t2.GetComponentsInChildren<PaintingJoint>();
                    foreach(PaintingJoint j in t2Joints)
                    {
                        if (Vector3.Distance(j.transform.position, GetJointTargetTransform(j).position) < 0.02f)
                        {
                            t2Total++;
                        }
                    }
                    // snapper is the transform that'll snap onto the other. The joint that we are snapping is joint
                    if (t1Total < t2Total)
                    {
                        // t1 is snapper, snap t1 to t2
                        t1.parent.position += t2.position - t1.position;
                    }
                    else
                    {
                        // t2 is snapper, snap t2 to t1
                        t2.parent.position += t1.position - t2.position;
                    }

                    

                }
            }
            else
            {
                joint.Status = PaintingJoint.JointStatus.Disconnected;
            }

            if (joint.Status == PaintingJoint.JointStatus.Disconnected) { foundDisconnected = true; };
        }
        if (!foundDisconnected)
        {
            /*throw new NotImplementedException("Begin end screen now, not implemented yet tho");*/
            SceneManager.LoadScene(3);
        }
    }

    private void OnDrawGizmos()
    {
        Color gizmoDisconnectColor = Color.red;
        Color gizmoConnectColor = Color.green;
        try
        {
            foreach (PaintingJoint joint in joints)
            {
                switch (joint.Status)
                {
                    case PaintingJoint.JointStatus.Connected:
                        Gizmos.color = gizmoConnectColor;
                        Gizmos.DrawSphere(joint.transform.position, 0.1f);
                        break;
                    case PaintingJoint.JointStatus.Disconnected:
                        Gizmos.color = gizmoDisconnectColor;
                        Gizmos.DrawSphere(joint.transform.position, 0.1f);
                        Gizmos.DrawLine(joint.transform.position, GetJointTargetTransform(joint).position);
                        break;
                }
            }
        }
        catch(NullReferenceException e)
        {
            
        }
    }

    private Transform GetJointTargetTransform(PaintingJoint joint)
    {
        foreach(PaintingJoint j in joints)
        {
            // compare
            if (j.ID == joint.PartnerID)
            {
                // is partner
                /*Debug.Log(joint.ID + " is partnered with " + j.ID)*/
                return j.transform;
            }
        }
        throw new System.Exception("Cannot find partner joint");
    }
}
