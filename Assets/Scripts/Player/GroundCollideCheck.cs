using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCollideCheck : MonoBehaviour
{
    public bool isGrounded;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.layer == 8)
        {
            isGrounded = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.layer == 8)
        {
            isGrounded = false;
        }
    }
}
