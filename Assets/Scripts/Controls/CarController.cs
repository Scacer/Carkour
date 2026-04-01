using NUnit.Framework.Constraints;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    // Suspension
    [Header("References")]
    [SerializeField] private Rigidbody carRB;
    [SerializeField] private Transform[] rayPoints;
    [SerializeField] private LayerMask drivable;
    [SerializeField] private Transform accelerationPoint;
    [SerializeField] private GameObject[] tires = new GameObject[4];

    [Header("Suspension Settings")]
    [SerializeField] private float damperStiffness; // Value used to represent damper fluid to prevent continuous bouncing
    [SerializeField] private float springStiffness; // The maximum force the spring can exert, occurring when fully compressed
    [SerializeField] private float restLength; // The standard length of our theoretical spring when at rest
    [SerializeField] private float springTravel; // The maximum distances the spring can either compress or extend from rest
    [SerializeField] private float wheelRadius;

    // Movement
    [Header("Car Settings")]
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float steerStrength = 15f;
    [SerializeField] private AnimationCurve turningCurve;
    [SerializeField] private float dragCoefficient = 1f;

    [Header("Air Control Settings")]
    [SerializeField] private float rollRotationSpeed = 200f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float yawpitchRotationSpeed = 200f;
    [SerializeField] InputAction jump;
    [SerializeField] InputAction rotate;
    private bool jumpIsPossible = false;

    private Vector3 currentCarLocalVelocity = Vector3.zero;
    private float carVelocityRatio = 0;

    private int[] wheelsIsGrounded = new int[4];
    private bool isGrounded = false;

    [Header("Input")]
    private float moveInput = 0;
    private float steerInput = 0;
    [SerializeField] private InputAction playerControls;
    Vector2 moveDirection = Vector2.zero;

    // Visuals
    [Header("Visuals")]
    [SerializeField] private float tireRotSpeed = 3000f;

    #region Unity Functions
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        carRB = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        jump.Enable();
        rotate.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
        jump.Disable();
        rotate.Disable();
    }

    private void Update()
    {
        GetPlayerInput();
    }

    private void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        CalculateCarVelocity();
        Movement();
        Visuals();
    }
    #endregion

    #region Suspension Functions
    /// <summary>
    /// Suspension() involves a collection of formulas run to calculate and implement car suspension.
    /// </summary>
    private void Suspension()
    {
        // foreach will be 1 to n where n is the number of raypoints, and thus wheels.
        for (int i =0; i < rayPoints.Length; i++)
        {
            // Previously a "foreach" loop, rayPoint must now be initialized.
            Transform rayPoint = rayPoints[i];

            RaycastHit hit;
            float maxLength = restLength + springTravel;

            // Shoots a raycast downwards from the rayPoint's position  
            if (Physics.Raycast(rayPoint.position, -rayPoint.up, out hit, maxLength + wheelRadius, drivable))
            {
                // If the raycast hits the ground, the wheel is grounded
                wheelsIsGrounded[i] = 1;

                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = restLength - currentSpringLength / springTravel;

                float springVelocity = Vector3.Dot(carRB.GetPointVelocity(rayPoint.position), rayPoint.up);
                float dampForce = damperStiffness * springVelocity;

                float springForce = springStiffness * springCompression;

                float netForce = springForce - dampForce;

                carRB.AddForceAtPosition(netForce * rayPoint.up, rayPoint.position);

                // Visuals

                SetTirePosition(tires[i], hit.point + rayPoint.up * wheelRadius);

                Debug.DrawLine(rayPoint.position, hit.point, Color.red);
            }
            else
            {
                // If the raycast does NOT hit the ground, the wheel is NOT grounded
                wheelsIsGrounded[i] = 0;

                // Visuals
                SetTirePosition(tires[i], rayPoint.position - rayPoint.up * maxLength);

                Debug.DrawLine(rayPoint.position, rayPoint.position + (wheelRadius + maxLength) * -rayPoint.up, Color.green);
            }
        }
    }
    #endregion

    #region Car Status Check

    /// <summary>
    /// GroundCheck() ensures that the car is only classified as "grounded" if 2 or more wheels are on the ground.
    /// </summary>
    private void GroundCheck()
    {
        int tempGroundedWheels = 0;

        for (int i = 0; i < wheelsIsGrounded.Length; i++)
        {
            tempGroundedWheels += wheelsIsGrounded[i];
        }

        if (tempGroundedWheels > 1)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void CalculateCarVelocity()
    {
        currentCarLocalVelocity = transform.InverseTransformDirection(carRB.linearVelocity);
        carVelocityRatio = currentCarLocalVelocity.z / maxSpeed;
    }

    #endregion

    #region Input Handling

    private void Movement()
    {
        if (isGrounded)
        {
            Acceleration();
            Deceleration();
            Turn();
            SidewaysDrag();
            HandleJump();
        }
        else
        {
            AirborneControls();
        }

    }

    private void AirborneControls()
    {
        carRotation();
        carPitchYaw();
    }

    private void carRotation()
    {
        float rotation = rotate.ReadValue<float>();
        if (rotation < 0)
        {
            carRB.transform.Rotate(Vector3.back, rotation * rollRotationSpeed * Time.deltaTime);
        }
        else if (rotation > 0)
        {
            carRB.transform.Rotate(Vector3.forward, -rotation * rollRotationSpeed * Time.deltaTime);
        }
    }

    private void carPitchYaw()
    {
        float moveX = moveDirection.x;
        float moveY = moveDirection.y;

        if (moveY > 0)
        {
            carRB.transform.Rotate(Vector3.right, yawpitchRotationSpeed * Time.deltaTime);
        }
        else if (moveY < 0)
        {
            carRB.transform.Rotate(Vector3.left, yawpitchRotationSpeed * Time.deltaTime);
        }

        if (moveX > 0)
        {
            carRB.transform.Rotate(Vector3.up, yawpitchRotationSpeed * Time.deltaTime);
        }
        else if (moveX < 0)
        {
            carRB.transform.Rotate(Vector3.down, yawpitchRotationSpeed * Time.deltaTime);
        }
    }

    private void HandleJump()
    {
        if (jump.IsPressed())
        {
            carRB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void GetPlayerInput()
    {
        // We need "Vertical" and "Horizontal" control under the new Unity Input System
        // moveInput = Input.GetAxis("Vertical");
        //steerInput = Input.GetAxis("Horizontal");
        moveDirection = playerControls.ReadValue<Vector2>();
    }
    #endregion

    #region Movement

    private void Acceleration()
    {
        carRB.AddForceAtPosition(acceleration * moveDirection.y * transform.forward, accelerationPoint.position, ForceMode.Acceleration);
    }


    private void Deceleration()
    {
        carRB.AddForceAtPosition(deceleration * moveDirection.y * -transform.forward, accelerationPoint.position, ForceMode.Acceleration);
    }

    private void Turn()
    {
        // steerStrength determines how sharply the car turns, steerInput is between -1 and 1 (-ve turns left, +ve turns right)
        // turningCurve evaluates the turn strength on a curve, Mathf.Sign checks if the car is moving backwards or forwards
        // transform.up ensures the car is rotated vertically, and ForceMode is acceleration ensuring torque is applied independent of car mass.

        carRB.AddTorque(steerStrength * moveDirection.x * turningCurve.Evaluate(carVelocityRatio) * Mathf.Sign(carVelocityRatio) * transform.up, ForceMode.Acceleration);
    }

    private void SidewaysDrag()
    {
        float currentSidewaysSpeed = currentCarLocalVelocity.x;
        float dragMagnitude = -currentSidewaysSpeed * dragCoefficient;

        Vector3 dragForce = transform.right * dragMagnitude;

        carRB.AddForceAtPosition(dragForce, carRB.worldCenterOfMass, ForceMode.Acceleration);
    }
    #endregion

    #region Visuals

    private void Visuals()
    {
        TireVisuals();
    }

    private void TireVisuals()
    {
        for (int i = 0; i < tires.Length; i++)
        {
            if (i < 2)
            {
                tires[i].transform.Rotate(Vector3.right, tireRotSpeed * carVelocityRatio * Time.deltaTime, Space.Self);
            }
            else
            {
                tires[i].transform.Rotate(Vector3.right, tireRotSpeed * moveInput * Time.deltaTime, Space.Self);
            }
        }
    }

    private void SetTirePosition(GameObject tire, Vector3 targetPosition)
    {
        tire.transform.position = targetPosition;
    }

    #endregion
}
