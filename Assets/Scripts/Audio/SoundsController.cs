using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsController : MonoBehaviour
{
    public float mainVolume = 0.5f;

    void Start()
    {
        UpdateAudiosourceVolumes();
    }

    void Update()
    {
        UpdateAudiosourceVolumes();
    }

    public void UpdateAudiosourceVolumes()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("AudioController");
        for (int i = 0; i < gameObjects.Length; i++)
        {
            gameObjects[i].GetComponent<AudioSourceController>().SetMaxVolume(mainVolume);
        }
    }
}
