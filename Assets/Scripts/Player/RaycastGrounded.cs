using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastGrounded : MonoBehaviour
{
    public ThirdPersonMovement thirdPersonMovement;
    public PlayerAnimationController playerAnimationController;

    public bool grounded;

    LayerMask layerMask;

    void Start()
    {
        layerMask = thirdPersonMovement.layerMask;
    }

    void Update()
    {
        grounded = Physics.Raycast(transform.position, -Vector3.up, playerAnimationController.groundRaycastDistance, layerMask);
    }
}
