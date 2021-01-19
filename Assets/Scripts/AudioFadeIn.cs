using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioFadeIn : MonoBehaviour
{
    public AnimationCurve volumeCurve;
    public float volumeMultiplier = 1f;
    [Tooltip("Lower speed will take longer (1 / speed = timetaken)")]
    public float speed = 0.5f; 

    AudioSource source;
    // Start is called before the first frame update
    void Start()
    {
        source = gameObject.GetComponent<AudioSource>();
    }

    float timer = 0f;

    // Update is called once per frame
    void Update()
    {
        if (timer < 1)
        {
            timer += Time.deltaTime * speed;
            source.volume = volumeCurve.Evaluate(timer);
        }
        else
        {
            source.volume = volumeMultiplier;
        }
    }
}
