using NUnit.Framework.Constraints;
using System;
using UnityEditor.Experimental.GraphView;
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
    [SerializeField] private GameObject[] frontTireParents = new GameObject[2];

    [Header("Suspension Settings")]
    [SerializeField] private float damperStiffness; // Value used to represent damper fluid to prevent continuous bouncing
    [SerializeField] private float springStiffness; // The maximum force the spring can exert, occurring when fully compressed
    [SerializeField] private float restLength; // The standard length of our theoretical spring when at rest
    [SerializeField] private float springTravel; // The maximum distances the spring can either compress or extend from rest
    [SerializeField] private float wheelRadius;

    #region Movement Attributes
    // Movement
    [Header("Car Settings")]
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float steerStrength = 15f;
    [SerializeField] private AnimationCurve turningCurve;
    [SerializeField] private float dragCoefficient = 1f;
    #endregion

    #region Air Control Attributes
    [Header("Air Control Settings")]
    [SerializeField] private float rollRotationSpeed = 200f;
    [SerializeField] private float jumpForce = 31000f;
    private float jumpProtection = 0f;
    [SerializeField] private float yawpitchRotationSpeed = 200f;
    [SerializeField] private float rotationTorqueSpeed = 10f;
    [SerializeField] private float angularVelocity = 500f;
    [SerializeField] InputAction jump;
    [SerializeField] InputAction rotate;
    private bool justJumped = false;
    private float sinceJumped = 0f;
    private float jumpBuffer = 0.5f;
    #endregion

    #region Powerup Attributes
    public enum Powerup 
    { 
        NONE,
        JUMPBOOST,
        MAGNET,
        SPEEDBOOST
    }
    public Powerup[] collectedItems = new Powerup[2];

    public static event Action itemCollected;
    public static event Action itemUsed;
    public static event Action wallRideBegin;
    private bool magnetised;

    public float magnetDuration = 3f;
    private float magnetTimer = 0f;
    private int[] magnetisedWheels = new int[4];
    private bool wallAttraction = false;
    #endregion


    private Vector3 currentCarLocalVelocity = Vector3.zero;
    private float carVelocityRatio = 0;

    private int[] wheelsIsGrounded = new int[4];
    private bool isGrounded = false;

    [Header("Wall Riding")]
    [SerializeField] private LayerMask ridableWall;
    private int[] wheelsOnWall = new int[4];
    private bool isWallRiding = false;

    [Header("Input")]
    [SerializeField] private InputAction playerControls;
    Vector2 moveDirection = Vector2.zero;

    // Visuals
    [Header("Visuals")]
    [SerializeField] private float tireRotSpeed = 3000f;
    [SerializeField] private float maxSteeringAngle = 30f;

    #region Unity Functions
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        carRB = GetComponent<Rigidbody>();
        carRB.maxAngularVelocity = angularVelocity;
        jumpProtection = Time.time;
        magnetised = false;
        collectedItems[0] = Powerup.NONE;
        collectedItems[1] = Powerup.NONE;
    }

    private void OnEnable()
    {
        playerControls.Enable();
        jump.Enable();
        rotate.Enable();

        // Item Collection
        GachaItem.DoubleJumpCollected += ()=>CollectItem(Powerup.JUMPBOOST);
        GachaItem.MagnetCollected += () => CollectItem(Powerup.MAGNET);
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
        Visuals();
    }

    private void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        WallCheck();
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
            // Shoots "magnetism" raypoints, these are slightly longer and will cause the car to pull towards a wall
            else if (Physics.Raycast(rayPoint.position, -rayPoint.up, out hit, maxLength + wheelRadius, ridableWall) && collectedItems[0] == Powerup.MAGNET)
            {
                RaycastHit magnetiseHit;
                if(Physics.Raycast(rayPoint.position, -rayPoint.up, out magnetiseHit, 2*maxLength + wheelRadius, ridableWall))
                {
                    magnetisedWheels[i] = 1;
                    if (wallAttraction && !magnetised)
                    {
                        carRB.AddForceAtPosition(350 * -rayPoint.up, rayPoint.position);
                    }
                }
                // Detects when the player magnetises to a wall
                if (!magnetised)
                {
                    magnetised = true;
                    magnetTimer = Time.time;
                    wallRideBegin?.Invoke();
                }
                    wheelsOnWall[i] = 1;

                // Here the suspension code needs to be altered
                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = restLength - currentSpringLength / springTravel;

                float springVelocity = Vector3.Dot(carRB.GetPointVelocity(rayPoint.position), rayPoint.up);
                float dampForce = damperStiffness * springVelocity;

                float springForce = springStiffness * springCompression;

                float netForce = springForce - dampForce;

                carRB.AddForceAtPosition(netForce * rayPoint.up, rayPoint.position);
                if (isWallRiding) // If the player is wall riding, attach them to the wall and give them a small boost
                {
                    carRB.AddForceAtPosition(200 * -rayPoint.up, rayPoint.position);
                    carRB.AddForceAtPosition((acceleration * moveDirection.y * transform.forward), accelerationPoint.position, ForceMode.Impulse);
                }
                
            }
            else
            {
                // If the raycast does NOT hit the ground, the wheel is NOT grounded
                wheelsIsGrounded[i] = 0;
                wheelsOnWall[i] = 0;

                // Visuals
                SetTirePosition(tires[i], rayPoint.position - rayPoint.up * maxLength);

                Debug.DrawLine(rayPoint.position, rayPoint.position + (wheelRadius + maxLength) * -rayPoint.up, Color.green);
            }

            // Allows for wall riding for the specified duration, detaching the player when the powerup runs out
            if (Time.time - magnetTimer > magnetDuration && magnetTimer != 0f)
            {
                UseItem();
                magnetised = false;
                magnetTimer = 0f;
                // Pushes the car lightly off the wall if attached
                if (isWallRiding)
                {
                    float releaseStrength = 0.3f * jumpForce;
                    carRB.AddForce(carRB.transform.up * releaseStrength, ForceMode.Impulse);
                    carRB.AddForce(Vector3.up * releaseStrength, ForceMode.Impulse);
                }  
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

        if (tempGroundedWheels > 2)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    /// <summary>
    /// WallCheck() ensures that if we are riding on a wall
    /// </summary>
    private void WallCheck()
    {
        int tempWallRidingWheels = 0;
        int tempMagnetisedWheels = 0;

        for (int i = 0; i < wheelsOnWall.Length; i++)
        {
            tempWallRidingWheels += wheelsOnWall[i];
            tempMagnetisedWheels += magnetisedWheels[i];
        }

        if(tempWallRidingWheels > 2)
        {
            isWallRiding = true;
        }
        else
        {
            isWallRiding = false;
        }

        if (tempMagnetisedWheels > 2)
        {
            wallAttraction = true;
        }
        else
        {
            wallAttraction = false;
        }
        //Debug.Log(isGrounded);
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
        if (isGrounded || isWallRiding)
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
        if (collectedItems[0] == Powerup.JUMPBOOST)
        {
            HandleJump();
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
        // If rotation is +ve, turn right
        if (rotation > 0)
        {
            // Apply torque backwards (-frwd) at defined rotation speed
            Vector3 appliedTorque = -transform.forward * rotationTorqueSpeed;
            // ForceMode Acceleration ensures a more drift-car feel
            carRB.AddTorque(appliedTorque, ForceMode.Acceleration);

            //carRB.transform.Rotate(Vector3.back, rotation * rollRotationSpeed * Time.deltaTime); - LEGACY
        }
        // If rotation is -ve, turn left
        else if (rotation < 0)
        {
            // Apply torque backwards (-frwd) at defined rotation speed
            Vector3 appliedTorque = transform.forward * rotationTorqueSpeed;
            // ForceMode Acceleration ensures a more drift-car feel
            carRB.AddTorque(appliedTorque, ForceMode.Acceleration);

            //carRB.transform.Rotate(Vector3.forward, -rotation * rollRotationSpeed * Time.deltaTime); - LEGACY
        }
    }

    private void carPitchYaw()
    {
        float moveX = moveDirection.x;
        float moveY = moveDirection.y;

        if (moveY > 0)
        {
            Vector3 appliedTorque = transform.right * rotationTorqueSpeed;
            carRB.AddTorque(appliedTorque, ForceMode.Acceleration);
            //carRB.transform.Rotate(Vector3.right, yawpitchRotationSpeed * Time.deltaTime);
        }
        else if (moveY < 0)
        {
            Vector3 appliedTorque = -transform.right * rotationTorqueSpeed;
            carRB.AddTorque(appliedTorque, ForceMode.Acceleration);
            //carRB.transform.Rotate(Vector3.left, yawpitchRotationSpeed * Time.deltaTime);
        }

        if (moveX > 0)
        {
            Vector3 appliedTorque = transform.up * rotationTorqueSpeed;
            carRB.AddTorque(appliedTorque, ForceMode.Acceleration);
            //carRB.transform.Rotate(Vector3.up, yawpitchRotationSpeed * Time.deltaTime);
        }
        else if (moveX < 0)
        {
            Vector3 appliedTorque = -transform.up * rotationTorqueSpeed;
            carRB.AddTorque(appliedTorque, ForceMode.Acceleration);
            //carRB.transform.Rotate(Vector3.down, yawpitchRotationSpeed * Time.deltaTime);
        }
    }

    private void HandleJump()
    {
        if (jump.IsPressed())
        {
            

            if (sinceJumped == 0f)
            {
                if (collectedItems[0] == Powerup.JUMPBOOST && !isGrounded)
                {
                    // Powerup jump should be slightly stronger, as the player is already airborne and may want to alter their trajectory
                    float powerJumpForce = jumpForce * 0.7f;
                    // Using "transform.up" here instead of "Vector3.up" ensures that the player can alter their jump trajectory with the powerup
                    carRB.AddForce(transform.up * powerJumpForce, ForceMode.Impulse);
                    UseItem();
                }
                else if (isGrounded)
                {
                    carRB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                    justJumped = true;
                }
                else if (isWallRiding)
                {
                    float upForce = (float)0.3 * jumpForce;
                    carRB.AddForce(carRB.transform.up * jumpForce, ForceMode.Impulse);
                    carRB.AddForce(Vector3.up * upForce, ForceMode.Impulse);
                }
                sinceJumped = Time.time;
            }
            else if (Time.time - sinceJumped > jumpBuffer)
            {
                sinceJumped = 0f;
            }

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
        if (moveDirection.y > 0 || carVelocityRatio > 0)
        {
            carRB.AddTorque(steerStrength * moveDirection.x * turningCurve.Evaluate(carVelocityRatio) * Mathf.Sign(carVelocityRatio) * transform.up, ForceMode.Acceleration);
        }
        else if (moveDirection.y < 0 || carVelocityRatio < 0)
        {
            carRB.AddTorque(steerStrength * moveDirection.x * turningCurve.Evaluate(-carVelocityRatio) * Mathf.Sign(carVelocityRatio) * transform.up, ForceMode.Acceleration);
        }
        
    }

    private void SidewaysDrag()
    {
        float currentSidewaysSpeed = currentCarLocalVelocity.x;
        float dragMagnitude = -currentSidewaysSpeed * dragCoefficient;

        Vector3 dragForce = transform.right * dragMagnitude;

        carRB.AddForceAtPosition(dragForce, carRB.worldCenterOfMass, ForceMode.Acceleration);
    }
    #endregion

    #region Item Handling
    private void CollectItem(Powerup item)
    {
        if (collectedItems[0] == Powerup.NONE)
        {
            collectedItems[0] = item;
            itemCollected?.Invoke();
            return;
        }
        else if (collectedItems[1] == Powerup.NONE)
        {
            collectedItems[1] = item;
            itemCollected?.Invoke();
            return;
        }
    }

    private void UseItem()
    {
        // Save the item in the second box and void it
        Powerup tempItem = collectedItems[1];
        collectedItems[1] = 0;
        // Move the saved item into the first box
        collectedItems[0] = tempItem;
        itemUsed?.Invoke();
    }

    #endregion

    #region Visuals

    private void Visuals()
    {
        TireVisuals();
    }

    private void TireVisuals()
    {
        float steerAngle = maxSteeringAngle * moveDirection.x;

        for (int i = 0; i < tires.Length; i++)
        {
            if (i < 2)
            {
                tires[i].transform.Rotate(Vector3.right, tireRotSpeed * carVelocityRatio * Time.deltaTime, Space.Self);

                // Steering code
                frontTireParents[i].transform.localEulerAngles = new Vector3(0, steerAngle, 0);

                
            }
            else
            {
                tires[i].transform.Rotate(Vector3.right, tireRotSpeed * moveDirection.y * Time.deltaTime, Space.Self);
            }
        }
    }

    private void SetTirePosition(GameObject tire, Vector3 targetPosition)
    {
        tire.transform.position = targetPosition;
    }

    #endregion
}
