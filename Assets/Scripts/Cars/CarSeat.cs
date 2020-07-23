using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSeat : MonoBehaviour
{
    public CarController carController;
    public GameObject seatedObject;

    public Vector3 outFromCarPos;

    public bool driversSeat = false;

    public AnimationClip playerAnimationClip;

    void Update()
    {
        if (driversSeat)
        {
            if (seatedObject == null)
            {
                carController.enabled = false;
            }
            else
            {
                carController.enabled = true;
            }
        }
    }

    public void SeatPlayer(GameObject player)
    {
        player.GetComponent<ThirdPersonMovement>().enabled = false;
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<Collider>().enabled = false;

        player.transform.parent = transform;
        player.transform.position = transform.position;
        player.transform.localEulerAngles = transform.localEulerAngles;
        seatedObject = player.gameObject;

        player.GetComponentInChildren<PlayerAnimationController>().PlayCustomAnimation(playerAnimationClip);
    }

    public void SeatOutPlayer(GameObject player)
    {
        player.transform.localPosition = outFromCarPos;
        seatedObject = null;
        player.transform.parent = null;

        player.GetComponent<Collider>().enabled = true;
        player.GetComponent<CharacterController>().enabled = true;
        player.GetComponent<ThirdPersonMovement>().enabled = true;

        player.GetComponentInChildren<PlayerAnimationController>().animator.SetBool("PlayCustomAnimation", false);
    }
}
