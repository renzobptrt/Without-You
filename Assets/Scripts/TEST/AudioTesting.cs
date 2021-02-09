using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTesting : MonoBehaviour
{
    public float volume, pitch;
    public AudioClip[] sfx;
    public AudioClip[] music;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AudioManager.instance.PlaySfx(sfx[0], volume, pitch);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            AudioManager.instance.PlayMusic(music[Random.Range(0,music.Length)]);
        }
    }
}
