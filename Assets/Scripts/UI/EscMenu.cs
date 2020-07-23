using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscMenu : MonoBehaviour
{
    public bool menuOpen = false;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuOpen == true)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                menuOpen = false;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.None;


                menuOpen = true;
            }
        }
    }
}
