using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

[Serializable]
public struct Wheel
{
    public GameObject model;
    public WheelCollider collider;
    public float wheelRadius;
}
[Serializable]
public struct GearInfo
{
    public float maxSpeed;   // Gear speed
    public float maxTorque;    // Gear torque
    public float motorBrakeTorque;    // Gear motorBrakeTorque
}

public class CarController : MonoBehaviour
{
    public enum CarType
    {
        FrontWheelDrive,   // Motor torque just applies to the front wheels
        RearWheelDrive,    // Motor torque just applies to the rear wheels
        FourWheelDrive     // Motor torque applies to the all wheels
    }


    public Rigidbody carRigidbody;                             // Rigidbody of the car
    public Transform CenterOfMass;                      // Center of mass of the car

    AudioSource audio;

    public bool engineOn = false;
    public bool isStarting = false;
    public bool driven = false;

    public Wheel[] wheels;
    public Wheel currentMaxRadiusDrivingWheel;

    [Header("Motor Vars")]
    public float currentMaxMotorTorque;                        // Maximum torque the motor can apply to wheels
    public float currentMotorTorque;                                  // Current Motor torque
    public CarType carType = CarType.FourWheelDrive;    // Set car type here
    public float maxRPM = 6500;
    public float idleRPM = 1500;
    public float rpm;

    [Header("Gear Vars")]
    //public int amountOfGears; // Without reverse
    //public float[] gearsSpeed, gearsTorque;
    public GearInfo[] gears;
    public int currentGear; //  -1 reverse | 0 natural | 1 gear one...

    public bool clutched = false;

    [Header("Brake Vars")]
    public float minMotorBrakePower;
    public float currentMotorBrakePower;
    public float brakePower;                            // Maximum brake power
    public float handBrakePower;


    [Header("Speed Vars")]
    public float currentMaxCarSpeedKmh;                              // Car maximum speed
    float carSpeed;                                     // The car speed in meter per second 
    public float carSpeedKmh;                            // The car speed in kilometer per hour

    [Header("Steering Vars")]
    public float maxSteeringAngle = 40;                 // Maximum steering angle the wheels can have    
    float tireAngle;                                    // Current steer angle


    [Header("Inputs")]
    public float throttleInput;
    public float brakeInput;
    //public float vertical = 0;                                 // The vertical input
    public float horizontalInput = 0;                               // The horizontal input   
    bool hBrake = false;                                // If handbrake button(Spacebar) pressed it becomes true


    [Header("Sound Vars")]
    public float engineIdlePitch = 0.3f;
    float enginePitch;
    float gearMinValue;
    float gearMaxValue;

    public AudioClip startSound;
    public AudioClip onSound;

    [Header("Exhaust Smoke Vars")]
    public ParticleSystem exhaustSmokeParticleSystem;
    public float exhaustRate;

    [Header("Wheel Smokea Vars")]
    public float wheelSmokeRate;

    [Header("UI")]
    public TMP_Text speedText;
    public TMP_Text gearText;
    public Image handBrakeText;

    public RectTransform rpmNeedle;
    public float rpmNeedleMinAngle = 160;
    public float rpmNeedleMaxAngle = -130;

    public RectTransform speedometerNeedle;
    public float speedometerNeedleMinAngle = 160;
    public float speedometerNeedleMaxAngle = -130;
    public float speedometerMaxSpeed = 200;


    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = CenterOfMass.localPosition;

        audio = GetComponent<AudioSource>();


        foreach (Wheel wheel in wheels)
        {
            wheel.collider.radius = wheel.wheelRadius;
        }

        if (carType == CarType.FrontWheelDrive)
        {
            foreach (Wheel wheel in wheels)
            {
                if (wheel.model.transform.localPosition.z > 0)
                {
                    if (wheel.wheelRadius > currentMaxRadiusDrivingWheel.wheelRadius)
                    {
                        currentMaxRadiusDrivingWheel.wheelRadius = wheel.wheelRadius;
                        currentMaxRadiusDrivingWheel.collider = wheel.collider;
                    }
                }
            }
        }
        else if (carType == CarType.RearWheelDrive)
        {
            foreach (Wheel wheel in wheels)
            {
                if (wheel.model.transform.localPosition.z < 0)
                {
                    if (wheel.wheelRadius > currentMaxRadiusDrivingWheel.wheelRadius)
                    {
                        currentMaxRadiusDrivingWheel.wheelRadius = wheel.wheelRadius;
                        currentMaxRadiusDrivingWheel.collider = wheel.collider;
                    }
                }
            }
        }
        else if (carType == CarType.FourWheelDrive)
        {
            foreach (Wheel wheel in wheels)
            {
                if (wheel.wheelRadius > currentMaxRadiusDrivingWheel.wheelRadius)
                {
                    currentMaxRadiusDrivingWheel.wheelRadius = wheel.wheelRadius;
                    currentMaxRadiusDrivingWheel.collider = wheel.collider;
                }
            }
        }
    }

    void Update()
    {
        SetUI();

        // Calculate the car speed in meter/second            
        Vector3 localVelocity = transform.InverseTransformDirection(carRigidbody.velocity);
        float newCarSpeed = localVelocity.z;
        if (newCarSpeed < 0)
        {
            newCarSpeed = -newCarSpeed;
        }
        carSpeed = newCarSpeed;
        carSpeedKmh = Mathf.Round(carSpeed * 3.6f);             // Convert the car speed from meter/second to kilometer/hour
                                                                // carSpeedRounded = Mathf.Round(carSpeed * 2.237f);         // Use this formula for mile/hour

        if (driven)
        {
            if (engineOn)
            {
                throttleInput = Input.GetAxis("Throttle");
                if (throttleInput < 0)
                {
                    throttleInput = 0;
                }
            }

            float newBrakeInput = Input.GetAxis("Brake");
            if (Input.GetKey(KeyCode.JoystickButton4))
            {
                newBrakeInput = 1;
            }
            brakeInput = newBrakeInput;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                hBrake = !hBrake;
            }

            horizontalInput = Input.GetAxis("Horizontal");


            if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.JoystickButton7))
            {
                if (engineOn == false && isStarting == false)
                {
                    audio.Play();
                    isStarting = true;

                    currentGear = 1;
                    GearDown();
                }
                else
                {
                    audio.Stop();
                    audio.loop = false;
                    isStarting = false;
                    engineOn = false;
                }
            }
            if (isStarting == true && audio.isPlaying == false)
            {
                rpm = idleRPM;

                engineOn = true;
                isStarting = false;
                audio.clip = onSound;
                audio.loop = true;
                audio.Play();
            }


            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.JoystickButton5))
            {
                clutched = true;
            }
            else
            {
                clutched = false;
            }


            if (Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                if (currentGear < gears.Length)
                {
                    GearUp();
                }
            }
            else if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                if (currentGear > -1)
                {
                    GearDown();
                }
            }
        }
        else
        {
            horizontalInput = 0;
            brakeInput = 0;
            throttleInput = 0;
            clutched = false;
        }


        rpm = GetRPM();


        EngineSound();
        ExhaustSmoke();


        ApplyBrakeTorqueAndSetMotorTorque();


        // Set the motorTorques based on the carType
        if (carType == CarType.FrontWheelDrive)
        {
            foreach (Wheel wheel in wheels)
            {
                if (wheel.model.transform.localPosition.z > 0)
                {
                    wheel.collider.motorTorque = currentMotorTorque * rpm / maxRPM;

                    //if (wheel.wheelRadius > currentMaxRadiusDrivingWheel.wheelRadius)
                    //{
                    //    currentMaxRadiusDrivingWheel.wheelRadius = wheel.wheelRadius;
                    //    currentMaxRadiusDrivingWheel.collider = wheel.collider;
                    //}
                }
            }
        }
        else if (carType == CarType.RearWheelDrive)
        {
            foreach (Wheel wheel in wheels)
            {
                if (wheel.model.transform.localPosition.z < 0)
                {
                    wheel.collider.motorTorque = currentMotorTorque * rpm / maxRPM;

                    //if (wheel.wheelRadius > currentMaxRadiusDrivingWheel.wheelRadius)
                    //{
                    //    currentMaxRadiusDrivingWheel.wheelRadius = wheel.wheelRadius;
                    //    currentMaxRadiusDrivingWheel.collider = wheel.collider;
                    //}
                }
            }
        }
        else if (carType == CarType.FourWheelDrive)
        {
            foreach (Wheel wheel in wheels)
            {
                wheel.collider.motorTorque = currentMotorTorque * rpm / maxRPM;

                //if (wheel.wheelRadius > currentMaxRadiusDrivingWheel.wheelRadius)
                //{
                //    currentMaxRadiusDrivingWheel.wheelRadius = wheel.wheelRadius;
                //    currentMaxRadiusDrivingWheel.collider = wheel.collider;
                //}
            }
        }


        Steer();
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


    void GearDown()
    {
        currentGear -= 1;

        if (currentGear == 0)
        {
            currentMaxCarSpeedKmh = 0;
            currentMaxMotorTorque = 0;
            currentMotorBrakePower = minMotorBrakePower;
        }
        else if (currentGear == -1)
        {
            currentMaxCarSpeedKmh = gears[0].maxSpeed; // If reverse, then same max speed as 1gear
            currentMaxMotorTorque = -gears[0].maxTorque; // If reverse, then same max torque as 1gear
            currentMotorBrakePower = gears[0].motorBrakeTorque;
        }
        else
        {
            currentMaxCarSpeedKmh = gears[currentGear - 1].maxSpeed;
            currentMaxMotorTorque = gears[currentGear - 1].maxTorque;
            currentMotorBrakePower = gears[currentGear - 1].motorBrakeTorque;
        }
    }
    void GearUp()
    {
        currentGear += 1;

        if (currentGear == 0)
        {
            currentMaxCarSpeedKmh = 0;
            currentMaxMotorTorque = 0;
            currentMotorBrakePower = minMotorBrakePower;
        }
        else
        {
            currentMaxCarSpeedKmh = gears[currentGear - 1].maxSpeed;
            currentMaxMotorTorque = gears[currentGear - 1].maxTorque;
            currentMotorBrakePower = gears[currentGear - 1].motorBrakeTorque;
        }
    }


    float GetRPM()
    {
        //float newRPM = carSpeedKmh / currentMaxCarSpeedKmh * maxRPM;
        //float circumFerence = 2 * currentMaxRadiusDrivingWheel.wheelRadius * 3.14f;
        //float newRPMSpeed = circumFerence * currentMaxRadiusDrivingWheel.collider.rpm * 60 / 1000;
        //float newRPM = newRPMSpeed / currentMaxCarSpeedKmh * maxRPM;

        //float newTargetRPM = newRPMSpeed / currentMaxCarSpeedKmh * maxRPM;
        //float newRPM;

        float newTargetRPM = 0;
        float newRPM = 0;

        float allWheelsRPM = 0;
        foreach (Wheel wheel in wheels)
        {
            float circumFerence = 2 * wheel.wheelRadius * 3.14f;
            float newRPMSpeed = circumFerence * currentMaxRadiusDrivingWheel.collider.rpm * 60 / 1000;
            float newWheelRPM = newRPMSpeed / currentMaxCarSpeedKmh * maxRPM;

            if (newWheelRPM < 0)
            {
                newWheelRPM = -newWheelRPM;
            }

            allWheelsRPM += newWheelRPM;
            //Debug.Log(wheel.collider.rpm);
            //Debug.Log("new: " + newWheelRPM);
        }
        newRPM = allWheelsRPM / wheels.Length;


        newTargetRPM = newRPM;
        if (currentMaxCarSpeedKmh == 0)
        {
            newTargetRPM = rpm;
        }
        //float newTargetRPM = carSpeedKmh / currentMaxCarSpeedKmh * maxRPM;
        //float newRPM = rpm;
        //newRPM = Mathf.Lerp(newRPM, newTargetRPM, 10 * Time.deltaTime);
        //Debug.Log("target: " + newTargetRPM);
        //Debug.Log("new: " + newRPM);

        if (!engineOn)
        {
            newTargetRPM -= 10000 * Time.deltaTime;
            if (newTargetRPM < 0)
            {
                newTargetRPM = 0;
            }

            return newTargetRPM;
        }
        else
        {
            if (currentGear == 0 || clutched)
            {
                newTargetRPM = throttleInput * maxRPM;
                newRPM = Mathf.Lerp(rpm, newTargetRPM, 2.5f * Time.deltaTime);
                //Debug.Log("target: " + newTargetRPM);
                //Debug.Log("new: " + newRPM);
                //newRPM = torque * maxRPM;
            }
            else
            {
                float minRPMLerpSpeed = 1f;
                float rpmLerpSpeed = 2f * (newRPM / maxRPM / 10);
                if (rpmLerpSpeed < minRPMLerpSpeed)
                {
                    rpmLerpSpeed = minRPMLerpSpeed;
                }

                newRPM = Mathf.Lerp(rpm, newTargetRPM, rpmLerpSpeed * Time.deltaTime);
            }

            if (newRPM < idleRPM)
            {
                float idleThrottleing = 5000 * (newRPM / idleRPM);
                newRPM += idleThrottleing * Time.deltaTime;

                if (newRPM < 100)
                {
                    engineOn = false;
                }
            }
            else if (newRPM > maxRPM - 10) // Make it varvstoppa
            {
                newRPM = maxRPM - 200;
            }
        }


        //if (!engineOn)
        //{
        //    newTargetRPM -= 10000 * Time.deltaTime;
        //    if (newTargetRPM < 0)
        //    {
        //        newTargetRPM = 0;
        //    }

        //    return newTargetRPM;
        //}
        //else
        //{
        //    if (currentGear == 0 || clutched)
        //    {
        //        newTargetRPM = torque * maxRPM;
        //        newRPM = rpm;
        //        newRPM = Mathf.Lerp(newRPM, newTargetRPM, 1f * rpm / 1000 * Time.deltaTime);
        //        //Debug.Log("target: " + newTargetRPM);
        //        //Debug.Log("new: " + newRPM);
        //        //newRPM = torque * maxRPM;
        //    }

        //    if (newTargetRPM < idleRPM)
        //    {
        //        newTargetRPM = idleRPM;
        //    }
        //    else if (newTargetRPM > maxRPM - 10) // Make it varvstoppa
        //    {
        //        newTargetRPM = maxRPM - 200;
        //    }

        //    Debug.Log(newTargetRPM);
        //    newRPM = rpm;
        //    newRPM = Mathf.Lerp(newRPM, newTargetRPM, 5f * Time.deltaTime);
        //}

        return newRPM;
    }

    void ApplyBrakeTorqueAndSetMotorTorque()
    {
        if (hBrake)
        {
            currentMotorTorque = 0;

            foreach (Wheel wheel in wheels)
            {
                if (wheel.model.transform.localPosition.z < 0)
                {
                    wheel.collider.brakeTorque = handBrakePower;
                }
            }
        }
        else
        {
            foreach (Wheel wheel in wheels)
            {
                float newBrakeTorque = 0;

                
                if (brakeInput > 0)
                {
                    if (wheel.model.transform.localPosition.z < 0)
                    {
                        if (throttleInput > 0)
                        {
                            newBrakeTorque = 0;
                        }
                        else
                        {
                            newBrakeTorque = brakePower * 0.4f;
                        }
                    }
                    else
                    {
                        newBrakeTorque = brakePower;
                    }
                }
                else if (carSpeedKmh > currentMaxCarSpeedKmh) // Test
                {
                    newBrakeTorque = currentMotorBrakePower;
                }
                else if (throttleInput == 0)
                {
                    newBrakeTorque = currentMotorBrakePower;
                }
                else if (throttleInput > 0)
                {
                    newBrakeTorque = 0;
                }

                

                wheel.collider.brakeTorque = newBrakeTorque;
            }

            // Check if car speed has NOT exceeded from maxSpeed
            if (clutched || carSpeedKmh < -currentMaxCarSpeedKmh || carSpeedKmh > currentMaxCarSpeedKmh)
            {
                currentMotorTorque = 0;
            }
            else
            {
                currentMotorTorque = currentMaxMotorTorque * throttleInput;
            }
            //if (carSpeedKmh > -currentMaxCarSpeedKmh && carSpeedKmh < currentMaxCarSpeedKmh)
            //{
            //    currentMotorTorque = currentMaxMotorTorque * torque;
            //}
        }
    }

    void Steer()
    {
        tireAngle = maxSteeringAngle * horizontalInput;                  // Calculate the front tires angles
        foreach (Wheel wheel in wheels)
        {
            if (wheel.model.transform.localPosition.z > 0)
            {
                wheel.collider.steerAngle = tireAngle;           // Set front wheel colliders steer angles
            }
        }
    }


    void EngineSound()
    {
        if (engineOn == true)
        {
            //if (currentGear == -1)
            //{
            //    gearMinValue = gears[0].maxSpeed - gears[0].maxSpeed / 2;
            //    gearMaxValue = gears[1].maxSpeed;

            //    enginePitch = (carSpeedKmh - gearMinValue) / (gearMaxValue - gearMinValue) + 0.4f;
            //}
            //else if (currentGear != 0)
            //{
            //    if (currentGear == 1)
            //    {
            //        gearMinValue = gears[currentGear - 1].maxSpeed - gears[currentGear - 1].maxSpeed / 2;
            //    }
            //    if (currentGear == gears.Length)
            //    {
            //        gearMaxValue = gears[currentGear - 1].maxSpeed + gears[currentGear - 1].maxSpeed / 2;
            //    }
            //    else
            //    {
            //        gearMaxValue = gears[currentGear].maxSpeed;
            //    }

            //    enginePitch = (carSpeedKmh - gearMinValue) / (gearMaxValue - gearMinValue) + 0.4f;
            //}


            //if (currentGear == 0 || enginePitch < engineIdlePitch)
            //{
            //    enginePitch = engineIdlePitch;
            //}

            enginePitch = rpm / maxRPM;

            if (enginePitch > 1.5f)
            {
                enginePitch = 1.5f;
            }
            else if (enginePitch < 0.1f)
            {
                enginePitch = 0.1f;
            }

            audio.pitch = enginePitch;
        }
        else
        {
            audio.clip = startSound;
            audio.pitch = 1;
        }
    }

    void ExhaustSmoke()
    {
        exhaustSmokeParticleSystem.emissionRate = rpm * exhaustRate;


        float newStartAlpha = -0.2f;
        newStartAlpha += rpm / maxRPM;
        if (newStartAlpha < 0.2f)
        {
            newStartAlpha = 0.2f;
        }

        exhaustSmokeParticleSystem.startColor = new Color(130, 130, 130, newStartAlpha);
    }


    void SetUI()
    {
        speedText.text = "" + carSpeedKmh;
        if (currentGear == -1)
        {
            gearText.text = "R";
        }
        else if (currentGear == 0)
        {
            gearText.text = "N";
        }
        else
        {
            gearText.text = "" + currentGear;
        }

        if (hBrake)
        {
            handBrakeText.enabled = true;
        }
        else
        {
            handBrakeText.enabled = false;
        }

        //float rpmNeedleAngle = rpmNeedleMinAngle;
        //rpmNeedleAngle -= carSpeedKmh / currentMaxCarSpeedKmh * (rpmNeedleMinAngle - rpmNeedleMaxAngle);
        float rpmNeedleAngle = rpmNeedleMinAngle - rpm / maxRPM * (rpmNeedleMinAngle - rpmNeedleMaxAngle);
        //if (carSpeedKmh == 0 && currentMaxCarSpeedKmh == 0)
        //{
        //    rpmNeedleAngle = rpmNeedleMinAngle;
        //}
        if (rpmNeedleAngle < rpmNeedleMaxAngle)
        {
            rpmNeedleAngle = rpmNeedleMaxAngle;
        }
        rpmNeedle.eulerAngles = new Vector3(0, 0, rpmNeedleAngle);


        float speedometerAngle = speedometerNeedleMinAngle;
        speedometerAngle -= carSpeedKmh / speedometerMaxSpeed * (speedometerNeedleMinAngle - speedometerNeedleMaxAngle);
        if (speedometerAngle < speedometerNeedleMaxAngle)
        {
            speedometerAngle = speedometerNeedleMaxAngle;
        }
        speedometerNeedle.eulerAngles = new Vector3(0, 0, speedometerAngle);
    }
}
