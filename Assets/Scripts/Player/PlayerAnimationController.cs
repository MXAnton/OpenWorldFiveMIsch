using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    ThirdPersonMovement thirdPersonMovement;
    public Transform cam;

    public Animator animator;
    AnimatorOverrideController animatorOverrideController;

    [Header("Point Vars")]
    public bool isPointing = false;
    [Space(2)]
    public GameObject pointerArmY;
    public GameObject pointerArmX;
    public GameObject pointerArmElbow;
    [Space(2)]
    public Vector3 pointerArmElbowAngle;
    public float maxYAngle = 70;
    [Space(2)]
    public float pointSmoothTime = 5;
    float pointSmoothVelocity;

    float previusTargetAngleY;
    float previusTargetAngleX;

    [Header("Ground Check Vars")]
    public GameObject[] groundRaycasts;
    public float groundRaycastDistance = 7;

    [Space]
    public float maxVerticalTiltAmount = 10;
    public float maxHorizontalTiltAmount = 10;
    public float verticalTiltMultiplier = 10;
    public float horizontalTiltMultiplier = 10;
    public float tiltX;
    public float tiltZ;

    public float previousAngle;
    public float previousAngleVelocity;

    void Start()
    {
        animator = GetComponent<Animator>();
        thirdPersonMovement = GetComponentInParent<ThirdPersonMovement>();

        animatorOverrideController = new AnimatorOverrideController();
        animatorOverrideController.runtimeAnimatorController = animator.runtimeAnimatorController;
    }

    void Update()
    {
        if (IsGrounded())
        {
            animator.SetBool("IsGrounded", true);
        }
        else
        {
            animator.SetBool("IsGrounded", false);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            isPointing = !isPointing;
        }
    }

    void LateUpdate()
    {
        TiltAfterVelocity();

        if (isPointing)
        {
            animator.SetBool("IsPointing", true);
            Point();
        }
        else
        {
            animator.SetBool("IsPointing", false);
        }
    }


    void TiltAfterVelocity()
    {
        float angleVelocity = previousAngle - thirdPersonMovement.GetComponent<Transform>().localEulerAngles.y;
        angleVelocity = previousAngleVelocity + angleVelocity;
        angleVelocity /= 2;

        if (angleVelocity > -0.5 && angleVelocity < 0.5f)
        {
            if (tiltZ < 0)
            {
                tiltZ += horizontalTiltMultiplier / 2 * Time.deltaTime;
            }
            else if (tiltZ > 0)
            {
                tiltZ -= horizontalTiltMultiplier / 2 * Time.deltaTime;
            }
        }
        else if (angleVelocity < 0)
        {
            tiltZ -= horizontalTiltMultiplier * Time.deltaTime;
        }
        else if (angleVelocity > 0)
        {
            tiltZ += horizontalTiltMultiplier * Time.deltaTime;
        }

        if (angleVelocity > -0.1f && angleVelocity < 0.1f)
        {
            if (tiltZ > -0.2f && tiltZ < 0.2f)
            {
                tiltZ = 0;
            }
        }


        if (tiltZ < -maxHorizontalTiltAmount)
        {
            tiltZ = -maxHorizontalTiltAmount;
        }
        if (tiltZ > maxHorizontalTiltAmount)
        {
            tiltZ = maxHorizontalTiltAmount;
        }


        Vector3 newTilt = new Vector3(
            tiltX,
            0,
            tiltZ);

        transform.localEulerAngles = newTilt;

        previousAngleVelocity = angleVelocity;
        previousAngle = thirdPersonMovement.GetComponent<Transform>().localEulerAngles.y;
    }


    void Point()
    {
        float targetAngleX = cam.localEulerAngles.x;
        float targetAngleY = cam.localEulerAngles.y;

        float angleY = Mathf.SmoothDampAngle(previusTargetAngleY, targetAngleY, ref pointSmoothVelocity, pointSmoothTime);
        float angleX = Mathf.SmoothDampAngle(previusTargetAngleX, targetAngleX, ref pointSmoothVelocity, pointSmoothTime);

        float nonPointAbleWidth = 360 - maxYAngle * 2;
        if (angleY < 360 - maxYAngle && angleY > maxYAngle + nonPointAbleWidth / 2)
        {
            angleY = 360 - maxYAngle;
        }
        if (angleY > maxYAngle && angleY < maxYAngle + nonPointAbleWidth / 2)
        {
            angleY = maxYAngle;
        }

        pointerArmX.transform.localRotation = Quaternion.Euler(0, angleX - 90, -90);
        pointerArmY.transform.localRotation = Quaternion.Euler(0, angleY - 5, -90);

        previusTargetAngleX = targetAngleX;
        previusTargetAngleY = targetAngleY;

        pointerArmElbow.transform.localRotation = Quaternion.Euler(pointerArmElbowAngle.x, pointerArmElbowAngle.y, pointerArmElbowAngle.z);
    }

    bool IsGrounded()
    {
        bool grounded = false;

        int raycastHitsAmount = 0;
        foreach (GameObject groundRaycast in groundRaycasts)
        {
            if (groundRaycast.GetComponent<RaycastGrounded>().grounded)
            {
                raycastHitsAmount++;
            }
        }

        if (raycastHitsAmount >= 1)
        {
            grounded = true;
        }

        return grounded;
    }


    public void PlayCustomAnimation(AnimationClip customAnimationClip)
    {
        animatorOverrideController["CustomAnimation"] = customAnimationClip;
        animator.runtimeAnimatorController = animatorOverrideController;
        animator.SetBool("PlayCustomAnimation", true);
    }
}
