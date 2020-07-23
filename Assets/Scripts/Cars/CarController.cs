using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct Wheel
{
    public GameObject model;
    public WheelCollider collider;
}

public class CarController : MonoBehaviour
{
    public enum CarType
    {
        FrontWheelDrive,   // Motor torque just applies to the front wheels
        RearWheelDrive,    // Motor torque just applies to the rear wheels
        FourWheelDrive     // Motor torque applies to the all wheels
    }

    public Wheel[] wheels;

    [Header("Motor Vars")]
    public float maxMotorTorque;                        // Maximum torque the motor can apply to wheels
    public float maxSteeringAngle = 20;                 // Maximum steering angle the wheels can have    
    public float maxSpeed;                              // Car maximum speed
    public float brakePower;                            // Maximum brake power
    public CarType carType = CarType.FourWheelDrive;    // Set car type here

    float carSpeed;                                     // The car speed in meter per second 
    float carSpeedKmh;                            // The car speed in kilometer per hour
    float motorTorque;                                  // Current Motor torque
    float tireAngle;                                    // Current steer angle

    float vertical = 0;                                 // The vertical input
    float horizontal = 0;                               // The horizontal input   
    bool hBrake = false;                                // If handbrake button(Spacebar) pressed it becomes true

    public Transform CenterOfMass;                      // Center of mass of the car
    Rigidbody carRigidbody;                             // Rigidbody of the car

    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = CenterOfMass.localPosition;
    }

    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        tireAngle = maxSteeringAngle * horizontal;                  // Calculate the front tires angles
        foreach (Wheel wheel in wheels)
        {
            if (wheel.model.transform.localPosition.z > 0)
            {
                wheel.collider.steerAngle = tireAngle;           // Set front wheel colliders steer angles
            }
        }

        carSpeed = carRigidbody.velocity.magnitude;                 // Calculate the car speed in meter/second                 
        carSpeedKmh = Mathf.Round(carSpeed * 3.6f);             // Convert the car speed from meter/second to kilometer/hour
                                                                      // carSpeedRounded = Mathf.Round(carSpeed * 2.237f);         // Use this formula for mile/hour
        HandBrake();

        // Set the motorTorques based on the carType
        if (carType == CarType.FrontWheelDrive)
        {
            foreach (Wheel wheel in wheels)
            {
                if (wheel.model.transform.localPosition.z > 0)
                {
                    wheel.collider.motorTorque = motorTorque; 
                }
            }
        }
        else if (carType == CarType.RearWheelDrive)
        {
            foreach (Wheel wheel in wheels)
            {
                if (wheel.model.transform.localPosition.z < 0)
                {
                    wheel.collider.motorTorque = motorTorque;  
                }
            }
        }
        else if (carType == CarType.FourWheelDrive)
        {
            foreach (Wheel wheel in wheels)
            {
                wheel.collider.motorTorque = motorTorque;
            }
        }


        ApplyTransformToWheels();
    }


        // Set the wheel meshes to the correct position and rotation based on their wheel collider position and rotation
    void ApplyTransformToWheels()
    {
        Vector3 position;
        Quaternion rotation;

        foreach (Wheel wheel in wheels)
        {
            wheel.collider.GetWorldPose(out position, out rotation);
            wheel.model.transform.position = position;
            wheel.model.transform.rotation = rotation;
        }
    }

    void HandBrake()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            hBrake = true;
        }
        else
        {
            hBrake = false;
        }
        if (hBrake)
        {
            motorTorque = 0;

            foreach (Wheel wheel in wheels)
            {
                if (wheel.model.transform.localPosition.z < 0)
                {
                    wheel.collider.brakeTorque = brakePower;
                }
            }
        }
        else
        {
            foreach (Wheel wheel in wheels)
            {
                wheel.collider.brakeTorque = 0;
            }

            // Check if car speed has exceeded from maxSpeed
            if (carSpeedKmh < maxSpeed)
            {
                motorTorque = maxMotorTorque * vertical;
            }
            else
            {
                motorTorque = 0;
            }
        }
    }
}
