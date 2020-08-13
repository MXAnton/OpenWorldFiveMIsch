using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceController : MonoBehaviour
{
    public AudioSource audioSource;
    public float maxVolume;
    public float currentVolume;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void SetMaxVolume(float newMaxVolume)
    {
        float newVolumeMultiplier = maxVolume - newMaxVolume + 1;
        audioSource.volume *= newVolumeMultiplier;

        maxVolume = newMaxVolume;
    }

    public void SetVolume(float newVolume)
    {
        audioSource.volume = newVolume * maxVolume;
    }

    public void SetPitch(float newPitch)
    {
        audioSource.pitch = newPitch;
    }
}
