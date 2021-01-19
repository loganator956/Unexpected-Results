using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReaderAccessibility : MonoBehaviour
{
    /* This script should be attached to an item with a Text component. 
     * The script allows the player to hold mouse1 (RMB usually) to swap fonts to a more easy to read font
     */

    [SerializeField]
    Font easyFont;
    Font fancyFont;

    Text text;
    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponent<Text>();
        fancyFont = text.font;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1)) { text.font = easyFont;  };
        if (Input.GetKeyUp(KeyCode.Mouse1)) { text.font = fancyFont; };
    }
}
