using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextTypingEffect : MonoBehaviour
{
    public string text;
    public float interval;

    public bool reverse = false;

    char[] characters;
    int index = 0;
    float timer = 0f;

    string progressString = "";

    // Start is called before the first frame update
    void Start()
    {
        characters = text.ToCharArray();
    }

    // Update is called once per frame
    void Update()
    {
        if (!reverse)
        {
            try
            {
                timer += Time.deltaTime;
                if (timer > interval)
                {
                    progressString += characters[index];
                    index++;
                    timer = 0;
                }
            }
            catch(Exception except)
            {
                Debug.Log(except.Message);
            }
        }
        else
        {
            try
            {
                timer += Time.deltaTime;
                if (timer > interval)
                {
                    string newString = "";
                    for (int i = 0; i < progressString.Length-1; i++)
                    {
                        newString += progressString.ToCharArray()[i];
                    }
                    progressString = newString;
                    timer = 0;
                }
            }
            catch(Exception except)
            {
                Debug.Log(except.Message);  
            }
        }
        GetComponent<Text>().text = progressString;
    }
}
