using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    CharacterController controller;
    public Transform body;
    public Animator animator;
    public Transform cam;

    [Space]
    public float mass = 80;

    public float distanceToGround = 0.2f;
    public float groundCheckerRadius = 1;

    public LayerMask layerMask;

    [Header("Horizontal Movement")]
    public float speed = 20;
    public float magnitude;
    public float runMovementSpeedMultiplier = 2;
    public float crouchMovementSpeedMultiplier = 0.75f;
    float movemenSpeedtMultiplier;
    public float movementSmoother = 2;

    public bool isCrouching = false;

    [Header("Vertical Movement")]
    public float jumpSpeed = 80;
    public float jumpDelayTime = 0.3f;
    float jumpDelay;
    float vSpeed;

    float horizontal;
    float vertical;

    public GameObject target;

    [Header("Turn Vars")]
    public float turnSmoothTime = 0.2f;
    float turnSmoothVelocity;

    [Space]
    public float targetAngle;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {
        Move();
        MoveY();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
            animator.SetBool("IsCrouching", isCrouching);
        }
    }


    void Move()
    {
        if (target != null)
        {
            MoveTowardsTarget();
        }

        if (target == null || target != null && Input.GetAxisRaw("Horizontal") != 0 || target != null && Input.GetAxisRaw("Vertical") != 0)
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");

            target = null;
        }
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            movemenSpeedtMultiplier = runMovementSpeedMultiplier;
        }
        else
        {
            movemenSpeedtMultiplier = 1;
        }
        if (isCrouching)
        {
            movemenSpeedtMultiplier *= crouchMovementSpeedMultiplier;
        }

        if (magnitude < direction.magnitude * movemenSpeedtMultiplier)
        {
            magnitude += movementSmoother * Time.deltaTime; // no maximum
            if (magnitude > 1 * movemenSpeedtMultiplier)
            {
                magnitude = 1 * movemenSpeedtMultiplier;
            }
        }
        else if (magnitude > direction.magnitude * movemenSpeedtMultiplier)
        {
            magnitude -= movementSmoother * Time.deltaTime; // no maximum
            if (magnitude < 0)
            {
                magnitude = 0;
            }
        }

        if (magnitude >= 0.1f)
        {
            if (target == null)
            {
                targetAngle = transform.eulerAngles.y;
                if (direction.x > 0.1f || direction.x < -0.1f || direction.z > 0.1f || direction.z < -0.1f)
                {
                    targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                }
            }

            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            if (direction.x > 0.1f || direction.x < -0.1f || direction.z > 0.1f || direction.z < -0.1f)
            {
                transform.rotation = Quaternion.Euler(0, angle, 0);
            }

            Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            if (controller.enabled == true)
            {
                controller.Move(moveDirection.normalized * speed * movemenSpeedtMultiplier * Time.deltaTime);
            }
        }

        float currentMovementSpeed = Map(magnitude, 0f, 2f, 0f, 1f);
        animator.SetFloat("MovementSpeed", currentMovementSpeed);
    }

    void MoveY()
    {
        float jumpInput = Input.GetAxisRaw("Jump");


        if (IsGrounded())
        {
            if (jumpDelay <= 0)
            {
                // If jump
                if (jumpInput >= 0.5f)
                {
                    animator.SetTrigger("Jump");
                    jumpDelay = jumpDelayTime;
                }

                vSpeed = jumpSpeed * jumpInput;
            }
        }
        else
        {
            vSpeed -= mass * -Physics.gravity.y * Time.deltaTime;
        }

        Vector3 vel = new Vector3(0, vSpeed, 0); // include vertical speed in vel
                                                 // convert vel to displacement and Move the character
        if (controller.enabled == true)
        {
            controller.Move(vel * Time.deltaTime);
        }

        jumpDelay -= Time.deltaTime;
    }

    bool IsGrounded()
    {
        bool grounded = false;

        //grounded = Physics.Raycast(transform.position, -Vector3.up, distanceToGround);
        Vector3 checkPosition = new Vector3(transform.position.x, transform.position.y + distanceToGround, transform.position.z);

        grounded = Physics.CheckSphere(checkPosition, groundCheckerRadius, layerMask);
        //Vector3 halfBoxSize = new Vector3(groundCheckerRadius, groundCheckerRadius, groundCheckerRadius);
        //grounded = Physics.CheckBox(checkPosition, halfBoxSize, Quaternion.identity, layerMask);

        return grounded;
    }


    public float Map(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        float fromAbs = value - fromMin;
        float fromMaxAbs = fromMax - fromMin;

        float normal = fromAbs / fromMaxAbs;

        float toMaxAbs = toMax - toMin;
        float toAbs = toMaxAbs * normal;

        float to = toAbs + toMin;

        return to;
    }


    public void MoveTowardsTarget()
    {
        vertical = 1;

        Vector3 targetDir = target.transform.position - transform.position;
        float singleStep = 100 * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDir, singleStep, 0.0f);
        targetAngle = Mathf.Atan2(newDirection.x, newDirection.z) * Mathf.Rad2Deg;

        float distance = Vector3.Distance(new Vector3(transform.position.x, transform.position.y + 0.85f, transform.position.z), target.transform.position);
        if (target.GetComponent<CarSeat>())
        {
            if (distance < target.GetComponent<CarSeat>().seatableDistance)
            {
                vertical = 0;

                target.gameObject.GetComponent<CarSeat>().SeatPlayer(gameObject);

                target = null;
            }
        }
        else if (target.GetComponent<SeatController>())
        {
            if (distance < target.GetComponent<SeatController>().seatableDistance)
            {
                vertical = 0;

                target.gameObject.GetComponent<SeatController>().SeatPlayer(gameObject);

                target = null;
            }
        }
    }
}
