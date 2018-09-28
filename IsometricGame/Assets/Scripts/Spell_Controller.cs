using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spell_Controller : MonoBehaviour {

    public bool isDashing
    {
        get
        {
            return !Mathf.Approximately(m_dashTimer, 1);
        }
    }

    public bool isSpeedBoosting
    {
        get
        {
            return !Mathf.Approximately(m_speedTimer, 1);
        }
    }

    public bool isControllable
    {
        get
        {
            return !isDashing;
        }
    }
    
    public bool isDoubleJumping
    {
        get
        {
            return !Mathf.Approximately(m_jumpTimer, 1);
        }
    }


    // **** Cooldown checks **** //
    public bool speedReady
    {
        get
        {
            return Mathf.Approximately(m_speedCooldownTimer, 1);
        }
    }
    public bool dashReady
    {
        get
        {
            return Mathf.Approximately(m_dashCooldownTimer, 1);
        }
    }
    public bool jumpReady
    {
        get
        {
            return Mathf.Approximately(m_jumpCooldownTimer, 1);
        }
    }
    public bool refreshReady
    {
        get
        {
            return Mathf.Approximately(m_refreshCooldownTimer, 1);
        }
    }


    // **** Public variables ****

    public GameObject m_player;

    [Header("Text elements")]
    public Text qCooldown;
    public Text wCooldown;
    public Text eCooldown;
    public Text rCooldown;

    [Header("Speed Boost")]
    public float m_boostSpeed;
    public float m_speedTime = 0.9f;
    public float m_speedCooldown = 10.0f;

    [Header("Dash")]
    public float m_dashDistance = 6.0f;
    public float m_dashTime = 0.2f;
    public float m_dashCooldown = 3.0f;

    [Header("Double Jump")]
    public float m_jumpTime = 3.0f;
    public float m_jumpCooldown = 10.0f;

    [Header("Refresh")]
    public float m_refreshCooldown = 15.0f;

    // Private variables
    private Player_Controller m_playerScript;
    private Rigidbody m_playerBody;

    private float m_originalSpeed;


    private Vector3 m_dashStart, m_dashEnd;
    private float m_dashTimer, m_speedTimer, m_jumpTimer;
    private float m_speedCooldownTimer, m_jumpCooldownTimer, m_refreshCooldownTimer, m_dashCooldownTimer;

    // Use this for initialization
    void Start() {

        // Get the needed components
        m_playerBody = m_player.GetComponent<Rigidbody>();
        m_playerScript = m_player.GetComponent<Player_Controller>();

        m_originalSpeed = m_playerScript.m_moveSpeed;

        // Setup timers
        m_dashTimer = 1;
        m_speedTimer = 1;
        m_jumpTimer = 1;

        m_speedCooldownTimer = 1;
        m_jumpCooldownTimer = 1;
        m_refreshCooldownTimer = 1;
        m_dashCooldownTimer = 1;

    }
	
	// Update is called once per frame
	void Update () {

        // ***** Update timers ***** //
        m_dashTimer = Mathf.Clamp01(m_dashTimer + Time.deltaTime / m_dashTime);
        m_speedTimer = Mathf.Clamp01(m_speedTimer + Time.deltaTime / m_speedTime);
        m_jumpTimer = Mathf.Clamp01(m_jumpTimer + Time.deltaTime / m_jumpTime);
        // Clamping the timers bewteen 0 and 1 and instead dividing with the cooldown time give us easy control over the speed of the timers
        m_speedCooldownTimer = Mathf.Clamp01(m_speedCooldownTimer + Time.deltaTime / m_speedCooldown);        
        m_dashCooldownTimer = Mathf.Clamp01(m_dashCooldownTimer + Time.deltaTime / m_dashCooldown);
        m_jumpCooldownTimer = Mathf.Clamp01(m_jumpCooldownTimer + Time.deltaTime / m_jumpCooldown);
        m_refreshCooldownTimer = Mathf.Clamp01(m_refreshCooldownTimer + Time.deltaTime / m_refreshCooldown);


        // ***** Update input ***** //
        if (isControllable)
        {
            if (Input.GetKeyDown("q"))
                CastSpell("Speed");
            else if (Input.GetKeyDown("w"))
                CastSpell("Dash");
            else if (Input.GetKeyDown("e"))
                CastSpell("Jump");
            else if (Input.GetKeyDown("r"))
                CastSpell("Refresh");
        }

        // ***** Update UI ***** //
        UpdateUI();
    }

    private void FixedUpdate()
    {
        // ***** Update movement ***** //
        if (isDashing)
        {
            var p = Vector3.Lerp(m_dashStart, m_dashEnd, m_dashTimer * m_dashTimer);
            m_playerBody.MovePosition(p);
        }

        if (isSpeedBoosting)
            m_playerScript.m_moveSpeed = m_boostSpeed;
        else
            m_playerScript.m_moveSpeed = m_originalSpeed;

        // If the player is double jumping then the max number of charges will increase to 2
        if (isDoubleJumping)
            m_playerScript.m_maxJumpCharges = 2;
        // If not, then the max charges will be set to its default value 1
        else
            m_playerScript.m_maxJumpCharges = 1;

            
    }

    // Manages UI such as cooldowns, health etc
    private void UpdateUI()
    {
        if (speedReady)
        {
            qCooldown.text = "Speed boost: Ready";
            qCooldown.color = Color.green;
        }
            
        else
        {
            qCooldown.text = "Speed boost: Not ready";
            qCooldown.color = Color.gray;
        }
            
        if (dashReady)
        {
            wCooldown.text = "Dash: Ready";
            wCooldown.color = Color.blue;
        }
            
        else
        {
            wCooldown.text = "Dash: Not ready";
            wCooldown.color = Color.gray;
        }
            

        if (jumpReady)
        {
            eCooldown.text = "Double jump: Ready";
            eCooldown.color = Color.yellow;
        }
            
        else
        {
            eCooldown.text = "Double jump: Not ready";
            eCooldown.color = Color.gray;
        }
            

        if (refreshReady)
        {
            rCooldown.text = "Refresh: Ready";
            rCooldown.color = Color.magenta;
        }
            
        else
        {
            rCooldown.text = "Refresh: Not ready";
            rCooldown.color = Color.gray;
        }
            
    }

    private void CastSpell(string ability)
    {
        // An ability can only be cast if the corresponding cooldown time has elapsed
        if (ability == "Speed" && speedReady)
            SpeedBoost();
        else if (ability == "Dash" && dashReady)
            Dash();
        else if (ability == "Jump" && jumpReady)
            DoubleJump();
        else if (ability == "Refresh" && refreshReady)
            RefreshCooldown();
    }

    // Increases the player's movement speed during a limited time
    private void SpeedBoost()
    {
        m_speedTimer = 0;
        m_speedCooldownTimer = 0;
        m_originalSpeed = m_playerScript.m_moveSpeed;
    }

    // Moves the player forward in current direction at high speed
    private void Dash()
    {
        m_dashTimer = 0;
        m_dashCooldownTimer = 0;
        m_dashStart = m_playerBody.position;
        m_dashEnd = m_dashStart + m_playerScript.direction * m_dashDistance;
    }

    // Allows the player to jump one extra time during a limitet time
    private void DoubleJump()
    {
        m_jumpTimer = 0;
        m_jumpCooldownTimer = 0;
    }

    // Resets all the current cooldowns
    private void RefreshCooldown()
    {
        m_refreshCooldownTimer = 0;

        m_speedCooldownTimer = 1.0f;
        m_dashCooldownTimer = 1.0f;
        m_jumpCooldownTimer = 1.0f;
    }

}
