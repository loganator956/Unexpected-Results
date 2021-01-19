using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TestingInfo : MonoBehaviour
{
    [Header("GUI References")]
    public Canvas canvas;
    public Text fpsText;
    float refreshRate = 0.5f;


    float timer = 0f;
    // Start is called before the first frame update
    void Start()
    {
#if DEBUG
        canvas.gameObject.SetActive(true);
#endif

    }
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > refreshRate)
        {
            // refresh things
            fpsText.text = $"FPS: {1 / Time.deltaTime}\r\nFrame Time: {Time.deltaTime}ms";
            

            
            timer = 0;
        }
    }

    private void OnDrawGizmos()
    {
        /*Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(Vector3.zero, 1);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(2, 0), 1);*/
    }
}
