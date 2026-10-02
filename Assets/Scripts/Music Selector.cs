using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MusicSelector : MonoBehaviour
{
    public List<AudioClip> Musics = new List<AudioClip>();
    AudioSource Source;
    private AudioClip Clip;
    void Start()
    {
        Source = GetComponent<AudioSource>();
        Clip = Musics[Random.Range(0,Musics.Count)];
        Source.clip = Clip;
        Source.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if(Source.isPlaying == false)
        {
            Clip = Musics[Random.Range(0,Musics.Count)];
            Source.clip = Clip;
            Source.Play();
        }
    }
}
