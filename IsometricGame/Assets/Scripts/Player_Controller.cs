using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    public Vector3 direction
    {
        get
        {
            if (m_inputVector.magnitude > Mathf.Epsilon)
                return m_inputVector;
            else
                return transform.forward;
        }
    }

    //Public variables
    [Header("Movement")]
    public float m_moveSpeed = 10.0f;       // The player's movement speed
    public float m_acceleration = 10.0f;    // The player's acceleration
    public float m_jumpSpeed = 5.0f;        // How fast the player jumps
    public float m_jumpTime = 1.0f;         // How long until the player can jump again
    public float m_jumpCharges;             // Holds how many times the player can jump before landing

    [Header("Gameplay")]
    public LayerMask m_groundLayer;         // Used for checking the correct layer when ground checking
    public float m_maxJumpCharges = 1;      // Decides how many jump charges the player can have at one time
    
    // Private member variables
    private Rigidbody m_rigidbody;          // Reference to the player's rigidbody
    private Vector3 m_inputVector;          // Vector composing the movement input
    private Vector3 m_currentInputVector;   // The vector actually used for moving the player. Made up of a lerp with acceleration and inputVector
    private Vector3 m_groundCheckVector;    // The position from which the ground check will be made 
    private float m_jumpTimer;              // Used to "delay" the ability to jump so that the player cannot spam the jump button
    private bool m_grounded;                // If true, the player is on the ground and can for example jump
    private bool m_shouldJump;              // Used in FixedUpdate to see if a jump has been issued
    private float m_maxCastDistance = 0.5f; // The distance the ground check is done


    // Use this for initialization
    void Start()
    {
        
        m_rigidbody = this.GetComponent<Rigidbody>();

        m_jumpTimer = 1;
    }

    // Update is called once per frame
    void Update()
    {

        // Update timers
        m_jumpTimer = Mathf.Clamp01(m_jumpTimer + Time.deltaTime / m_jumpTime); // The timer counts between 0 and 1 and the jump time defines how fast it will count.

        // Update conditions

        // If available jump charges are greater than max number of charges, set charges to max
        if (m_jumpCharges > m_maxJumpCharges)
            m_jumpCharges = m_maxJumpCharges;

        // Update input
        UpdateInput();
    }

    void FixedUpdate()
    {
        // Ground check
        bool wasGrounded = m_grounded;

        RaycastHit hit;
        m_groundCheckVector = transform.position;
        m_grounded = (Physics.SphereCast(m_groundCheckVector, 0.5f, -Vector3.up, out hit, m_maxCastDistance, m_groundLayer.value) && Mathf.Approximately(m_jumpTimer, 1));

        Debug.Log("Timer: " + m_jumpTimer + " Ray: " + Physics.SphereCast(m_groundCheckVector, 0.5f, -Vector3.up, out hit, m_maxCastDistance, m_groundLayer.value));
        Debug.Log("Mask " + m_groundLayer.value);
        // If the player is grounded, jump charges will be restored to max number
        if (m_grounded)
            m_jumpCharges = m_maxJumpCharges;

        // Update movement
        UpdateMovement();

        if (wasGrounded && !m_grounded)
        {
            m_jumpCharges--;
        }

    }

    void UpdateInput()
    {
        // Movement input, using directional vector
        m_inputVector = Vector3.zero;
        m_inputVector += Input.GetKey("up") ? transform.forward : Vector3.zero;
        m_inputVector += Input.GetKey("down") ? -transform.forward : Vector3.zero;
        m_inputVector += Input.GetKey("right") ? transform.right : Vector3.zero;
        m_inputVector += Input.GetKey("left") ?- transform.right : Vector3.zero;

        // Camera-relative input
        m_inputVector = Camera.main.transform.rotation * m_inputVector;
        m_inputVector.y = 0; // Prevent the camera rotation from giving us movement along the y-axis.


        // Normalize input vector so speed is the same in all directions
        m_inputVector.Normalize();

        // If the player inputs space and is grounded, we store the jump input so we can use it in FixedUpdate()
        //if (Input.GetKeyDown(KeyCode.Space) && m_grounded && Mathf.Approximately(m_jumpTimer, 1))

        // If the palyer inputs space, has at least one jump charge, we store the jump input so we can use it in FixedUpdate()
        if (Input.GetKeyDown(KeyCode.Space) && m_jumpCharges >= 1) // THe jump timer makes sure we cannot spam the jump
        {
            m_shouldJump = true;
            
        }
    }

    void UpdateMovement()
    {
        // By using lerp we make sure the movement is smooth
        m_currentInputVector = Vector3.Lerp(m_currentInputVector, m_inputVector, m_acceleration * Time.fixedDeltaTime);

        // We cannot modify the members of Rigidbody.velocity so we make a copy, modify it and then reassign it 
        Vector3 currentVelocity = m_rigidbody.velocity;
        Vector3 inputVelocity = m_currentInputVector * m_moveSpeed;

        // This overrides the existing velocity, giving us complete control
        currentVelocity.x = inputVelocity.x;
        currentVelocity.z = inputVelocity.z;

        m_rigidbody.velocity = currentVelocity;

        // Jumping is performed by applying velocity upwards
        if(m_shouldJump)
        {
            // If the jump was a double jump, then the jump charge must be decremented manually
            if (m_jumpCharges > 0 && !m_grounded)
            {
                m_jumpCharges--;
            }

            m_jumpTimer = 0;
            m_jumpTimer = Mathf.Clamp01(m_jumpTimer + Time.deltaTime / m_jumpTime);
            m_rigidbody.velocity += Vector3.up * m_jumpSpeed;
            m_shouldJump = false;
            m_grounded = false;

        }

    }
}
