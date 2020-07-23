using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatController : MonoBehaviour
{
    public GameObject seatedObject;
    public Vector3 outFromSeatPos;

    public AnimationClip playerAnimationClip;

    public void SeatPlayer(GameObject player)
    {
        player.GetComponent<CharacterController>().enabled = false;
        //thirdPersonMovement.GetComponent<Collider>().enabled = false;
        player.GetComponent<ThirdPersonMovement>().enabled = false;

        player.transform.parent = transform;
        player.transform.position = transform.position;
        player.transform.localEulerAngles = transform.localEulerAngles;

        seatedObject = player.gameObject;

        player.GetComponentInChildren<PlayerAnimationController>().PlayCustomAnimation(playerAnimationClip);
    }

    public void SeatOutPlayer(GameObject player)
    {
        player.transform.localPosition = outFromSeatPos;
        seatedObject = null;
        player.transform.parent = null;

        //GetComponent<Collider>().enabled = true;
        player.GetComponent<CharacterController>().enabled = true;
        player.GetComponent<ThirdPersonMovement>().enabled = true;

        player.GetComponentInChildren<PlayerAnimationController>().animator.SetBool("PlayCustomAnimation", false);
    }
}
