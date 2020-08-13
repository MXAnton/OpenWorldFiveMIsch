using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUse : MonoBehaviour
{
    ThirdPersonMovement thirdPersonMovement;

    //public float useRaycastLength = 5;
    public LayerMask layerMask;

    public float usableCheckPosHeight = 8.5f;
    public float usableCheckRadius;

    private void Start()
    {
        thirdPersonMovement = GetComponent<ThirdPersonMovement>();
    }

    void Update()
    {
        //RaycastHit hit;
        //Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, useRaycastLength);
        //if (hit.collider != null)
        //{
        //    GetComponent<CrosshairController>().SetCrosshair(hit.collider.gameObject);
        //}

        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            // Get hits
            Collider[] hitColliders = Physics.OverlapSphere(new Vector3(transform.position.x, transform.position.y + usableCheckPosHeight, transform.position.z), usableCheckRadius, layerMask);
            float nearestHitDistance = usableCheckRadius * 2;
            GameObject nearestHit = null;
            if (hitColliders.Length != 0)
            {
                foreach (Collider hitCollider in hitColliders)
                {
                    float distance = Vector3.Distance(new Vector3(transform.position.x, transform.position.y + usableCheckPosHeight, transform.position.z), hitCollider.transform.position);
                    if (distance <= nearestHitDistance)
                    {
                        nearestHit = hitCollider.gameObject;
                        nearestHitDistance = distance;
                    }
                }
            }

            // Check hits
            if (nearestHit != null)        //Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, useRaycastLength, layerMask))
            {
                if (nearestHit.GetComponent<CarSeat>())
                {
                    if (nearestHit.gameObject.GetComponent<CarSeat>().seatedObject == null && transform.parent == null)
                    {
                        //thirdPersonMovement.MoveTowardsTarget(nearestHit);
                        thirdPersonMovement.target = nearestHit;
                    }
                    else
                    {
                        nearestHit.gameObject.GetComponent<CarSeat>().SeatOutPlayer(gameObject);
                    }
                }
                else if (nearestHit.GetComponent<SeatController>())
                {
                    if (nearestHit.gameObject.GetComponent<SeatController>().seatedObject == null && transform.parent == null)
                    {
                        //thirdPersonMovement.MoveTowardsTarget(nearestHit);
                        thirdPersonMovement.target = nearestHit;
                    }
                    else
                    {
                        nearestHit.gameObject.GetComponent<SeatController>().SeatOutPlayer(gameObject);
                    }
                }
            }
            //else
            //{
            //    if (GetComponentInParent<CarSeat>() != null)
            //    {
            //        if (GetComponentInParent<CarSeat>().seatedObject == gameObject)
            //        {
            //            GetComponentInParent<CarSeat>().SeatOutPlayer(gameObject);
            //        }
            //    }
            //    else if (GetComponentInParent<SeatController>() != null)
            //    {
            //        if (GetComponentInParent<SeatController>().seatedObject == gameObject)
            //        {
            //            GetComponentInParent<SeatController>().SeatOutPlayer(gameObject);
            //        }
            //    }
            //    //else
            //    //{
            //    //    if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, useRaycastLength))
            //    //    {
            //    //        Debug.Log(hit.collider.name);
            //    //    }
            //    //    else
            //    //    {
            //    //        Debug.Log("No hit");
            //    //    }
            //    //}
            //}
        }
    }
}
