using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    public GameObject crosshairHolder;
    public GameObject normalCrosshair, useCrosshair, chairCrosshair;
    const int useCrosshairLayer = 9;

    //public void SetCrosshair(GameObject hitGameObject)
    //{
    //    Destroy(crosshairHolder.transform.GetChild(0).gameObject);
    //    switch (hitGameObject.layer)
    //    {
    //        case useCrosshairLayer:
    //            if (hitGameObject.GetComponent<CarSeat>() || hitGameObject.GetComponent<SeatController>())
    //            {
    //                Instantiate(chairCrosshair, crosshairHolder.transform);
    //            }
    //            else
    //            {
    //                Instantiate(useCrosshair, crosshairHolder.transform);
    //            }
    //            break;
    //        default:
    //            Instantiate(normalCrosshair, crosshairHolder.transform);
    //            break;
    //    }
    //}
}
