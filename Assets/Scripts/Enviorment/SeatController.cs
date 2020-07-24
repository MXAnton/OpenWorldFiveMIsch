using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatController : MonoBehaviour
{
    public GameObject seatedObject;
    public Vector3 outFromSeatPos;
    public float seatableDistance;

    public AnimationClip playerAnimationClip;

    public void SeatPlayer(GameObject player)
    {
        player.GetComponentInChildren<PlayerAnimationController>().PlayCustomAnimation(playerAnimationClip);

        player.GetComponent<ThirdPersonMovement>().magnitude = 0;
        player.GetComponent<ThirdPersonMovement>().enabled = false;
        player.GetComponent<CharacterController>().enabled = false;
        //thirdPersonMovement.GetComponent<Collider>().enabled = false;

        player.transform.parent = transform;
        player.transform.position = transform.position;
        player.transform.localEulerAngles = transform.localEulerAngles;

        seatedObject = player.gameObject;
    }

    public void SeatOutPlayer(GameObject player)
    {
        player.GetComponentInChildren<PlayerAnimationController>().animator.SetBool("PlayCustomAnimation", false);

        player.transform.localPosition = outFromSeatPos; //new Vector3(player.transform.localPosition.x, player.transform.localPosition.y, player.transform.localPosition.z + outFromSeatPos.z);
        seatedObject = null;
        player.transform.parent = null;

        //GetComponent<Collider>().enabled = true;
        player.GetComponent<CharacterController>().enabled = true;
        player.GetComponent<ThirdPersonMovement>().enabled = true;
        player.GetComponent<ThirdPersonMovement>().magnitude = 0;
    }
}
