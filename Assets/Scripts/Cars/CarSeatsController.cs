using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct Seat
{
    public GameObject seatGameObject;
    public GameObject seatedObject;
}

public class CarSeatsController : MonoBehaviour
{
    public Seat[] seats;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
