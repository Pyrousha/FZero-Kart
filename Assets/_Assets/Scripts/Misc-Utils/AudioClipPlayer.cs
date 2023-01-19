using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioClipPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip[] audioClips;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Plays an audio clip out of the local array of audio clips assigned to this object
    /// </summary>
    /// <param name="_index"></param>
    /// <param name="_playOnce"></param>
    public void PlayClipWithIndex(int _index, bool _playOnce = true)
    {
        audioSource.clip = audioClips[_index];
        audioSource.loop = !_playOnce;
        audioSource.Play();
    }
}


