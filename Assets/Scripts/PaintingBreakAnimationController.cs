using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingBreakAnimationController : MonoBehaviour
{
    private void Start()
    {
        transform.GetComponent<Animator>().SetBool("Break", true);
    }

    bool isTiming = true;
    float timer = 0f;
    private void Update()
    {
        if (isTiming)
        {
            timer += Time.deltaTime;
            if (timer > 5f)
            {
                transform.GetComponent<Animator>().SetBool("Break", true);
                isTiming = false;
            }
        }
    }

    public void EndAnim()
    {
        /*transform.GetComponent<Animator>().SetBool("Break", false);*/
        Destroy(transform.GetComponent<Animator>());
        Debug.Log("Ending anim");
    }
}
