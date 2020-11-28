using UnityEngine;
using System.Collections;
using Prime31;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;


public class PlayerMovement : MonoBehaviour
{
    // movement config
    public float gravity = -25f;
    public float runSpeed = 8f;
    public float groundDamping = 20f;
    public float inAirDamping = 5f;
    public float wallSlideFallSpeed = -2f;
    public float jumpHeight = 3f;
    public float wallSlideExitTime = 0.15f;

    public GameObject pauseCanvas;

    [HideInInspector]
    private float normalizedHorizontalSpeed = 0;

    private CharacterController2D _controller;
    private Animator _animator;
    private RaycastHit2D _lastControllerColliderHit;
    private Vector3 _velocity;
    private SpriteRenderer spriteRenderer;
    private Transform sprite;
    private AudioManager audioManager;
    private Player player;

    [HideInInspector]
    public bool attacking = false;
    [HideInInspector]
    public bool specialAttacking = false;
    [HideInInspector]
    public int wallSliding = 0;

    bool wasGroundedLastFrame = false;

    private bool wallSlideTimeout = false;
    private bool exitingWallSlide = false;
    [HideInInspector]
    public bool stuned = false;
    private bool paused = false;

    private float playerControlX = 1f;
    private Coroutine exitWall;
    [HideInInspector]
    public Coroutine stunTimer;

    Vector2 i_movement;

    Controls.PlayerActions playerActions;


    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _controller = GetComponent<CharacterController2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        sprite = transform.GetChild(0);
        audioManager = FindObjectOfType<AudioManager>();
        player = GetComponent<Player>();

        // listen to some events for illustration purposes
        _controller.onControllerCollidedEvent += onControllerCollider;
        _controller.onTriggerEnterEvent += onTriggerEnterEvent;
        _controller.onTriggerExitEvent += onTriggerExitEvent;
    }

    private void Start()
    {
        // StartCoroutine("StartSequence");
    }

    public void BindControls(Controls.PlayerActions iam)
    {
        playerActions = iam;
        // Bind discrete events using C# delegates
        iam.Jump.performed += OnJump;
        iam.Attack.performed += OnAttack;
        iam.SpecialAttack.performed += OnSpecialAttack;
    }

    #region Event Listeners

    void onControllerCollider(RaycastHit2D hit)
    {
        // bail out on plain old ground hits cause they arent very interesting
        if (hit.normal.y == 1f)
        {
            return;
        }

        // logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
        //Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
    }


    void onTriggerEnterEvent(Collider2D col)
    {
        // Debug.Log( "onTriggerEnterEvent: " + col.gameObject.name );
    }


    void onTriggerExitEvent(Collider2D col)
    {
        // Debug.Log( "onTriggerExitEvent: " + col.gameObject.name );
    }

    private void OnMove(InputAction.CallbackContext cc)
    {
        i_movement = cc.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext cc)
    {

        if (_controller.isGrounded && !stuned)
        {
            _velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
            _controller.move(Vector3.up * _velocity.y * Time.deltaTime);
        }
        else if (wallSliding != 0 && !stuned)
        {
            _velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
            _velocity.x = wallSliding * jumpHeight * 3f;
            _controller.move(_velocity * Time.deltaTime);
            StartCoroutine(WallSlideJumpUnlock(0.1f));
            StartCoroutine(RegainXControl(0.3f));
        }
    }

    private void OnAttack(InputAction.CallbackContext cc)
    {
        if (!stuned)
        {
            player.Attack();
        }
    }

    private void OnSpecialAttack(InputAction.CallbackContext cc)
    {
        if (!stuned)
        {
            player.SpecialAttack();
        }
    }

    private void OnDestroy()
    {
        playerActions.Jump.performed -= OnJump;
        playerActions.Attack.performed -= OnAttack;
        playerActions.SpecialAttack.performed -= OnSpecialAttack;
    }

    #endregion


    // the Update loop contains a very simple example of moving the character around and controlling the animation
    void Update()
    {
        // var keyboard = Keyboard.current;
        // var gamepad = Gamepad.current;
        // if (gamepad == null)
        // 	print("No gamepad connected.");
        //     // return; // No gamepad connected.

        // if (keyboard.pKey.wasPressedThisFrame || gamepad != null && gamepad.startButton.wasPressedThisFrame)
        // 	Pause();

        i_movement = playerActions.Move.ReadValue<Vector2>();

        // if( Input.GetKey( KeyCode.RightArrow ) )
        if (i_movement.x > 0.1f && !stuned)
        {
            normalizedHorizontalSpeed = 1;
            if (transform.localScale.x < 0f)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        // else if( Input.GetKey( KeyCode.LeftArrow ) )
        else if (i_movement.x < -0.1f && !stuned)
        {
            normalizedHorizontalSpeed = -1;
            if (transform.localScale.x > 0f)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            normalizedHorizontalSpeed = 0;
        }

        if (!_controller.isGrounded)
        {
            CheckForWall();
            wasGroundedLastFrame = false;

            // if (wallSliding == 0) {
            // } else {
            // 	// Wall Jump
            // 	if( keyboard.upArrowKey.wasPressedThisFrame || gamepad != null && gamepad.crossButton.wasPressedThisFrame )
            // 	{
            // 		_velocity.y = Mathf.Sqrt( 2f * jumpHeight * -gravity );
            // 		_velocity.x = wallSliding * jumpHeight * 3f;
            // 		StartCoroutine(WallSlideJumpUnlock(0.1f));
            // 		StartCoroutine(RegainXControl(0.3f));
            // 	}
            // }

        }


        if (_controller.isGrounded)
        {
            wallSliding = 0;

            if (!wasGroundedLastFrame)
            {
                audioManager.Play("Landing");
                wasGroundedLastFrame = true;
            }
        }


        if (wallSliding != 0 && !stuned)
        {
            // Fall slowly if Wall Sliding
            if (_velocity.y < wallSlideFallSpeed)
            {
                _velocity.y = wallSlideFallSpeed;
            }
            else
            {
                _velocity.y += gravity * Time.deltaTime;
            }
        }
        else
        {
            // Apply Gravity
            _velocity.y += gravity * Time.deltaTime;
        }

        if (wallSliding == 0 || stuned)
        {
            if (stuned) normalizedHorizontalSpeed = 0;

            // apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
            var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping;
            float speed = runSpeed;
            _velocity.x = Mathf.Lerp(_velocity.x, normalizedHorizontalSpeed * speed, Time.deltaTime * smoothedMovementFactor * playerControlX);
        }
        else
        {
            // Wall Sliding lock X movement logic
            _velocity.x = -wallSliding * 5f;
            if (normalizedHorizontalSpeed == wallSliding && !exitingWallSlide)
            {
                exitWall = StartCoroutine(WallSlideExitUnlock(wallSlideExitTime));
            }
            else if (normalizedHorizontalSpeed != wallSliding && exitingWallSlide)
            {
                if (exitWall != null)
                    StopCoroutine(exitWall);
                exitingWallSlide = false;
            }
        }

        // if holding down bump up our movement amount and turn off one way platform detection for a frame.
        // this lets us jump down through one way platforms
        if (i_movement.y < -0.8f)
        {
            // _velocity.y *= 3f;
            _controller.ignoreOneWayPlatformsThisFrame = true;
        }

        // UpdateSpriteOrientation();
        UpdateAnimation();

        _controller.move(_velocity * Time.deltaTime);

        // grab our current _velocity to use as a base for all calculations
        _velocity = _controller.velocity;
    }


    void UpdateAnimation()
    {
        if (stuned)
        {
            _animator.Play("Stun");
        }
        else if (wallSliding != 0)
        {
            _animator.Play("Wall-Slide");
        }
        else if (specialAttacking)
        {
            _animator.Play("SpecialAttack");
        }
        else if (attacking)
        {
            _animator.Play("Attack");
        }
        else if (_velocity.y > 0 && !_controller.isGrounded)
        {
            _animator.Play("Jump-Up");
        }
        else if (_velocity.y < 0 && !_controller.isGrounded)
        {
            _animator.Play("Jump-Down");
        }
        else if (i_movement.x != 0 && _controller.isGrounded)
        {
            _animator.Play("Running");
        }
        else
        {
            _animator.Play("Idle");
        }
    }

    void CheckForWall()
    {
        if (!wallSlideTimeout)
        {
            RaycastHit2D raycastL = Physics2D.Raycast(transform.position, Vector2.left, 0.6f, 1 << 9);
            RaycastHit2D raycastR = Physics2D.Raycast(transform.position, Vector2.right, 0.6f, 1 << 9);
            Debug.DrawRay(transform.position, Vector2.left * 0.6f);
            Debug.DrawRay(transform.position, Vector2.right * 0.6f);

            if (raycastL || raycastR)
            {
                // if (wallSliding == 0)
                // 	audioManager.Play("Land");
                if (raycastL && raycastL.normal.x > 0.9f)
                {
                    wallSliding = 1;
                }
                if (raycastR && raycastR.normal.x < -0.9f)
                {
                    wallSliding = -1;
                }
                transform.localScale = new Vector3(wallSliding, 1, 1);
            }
            else
            {
                wallSliding = 0;
            }
        }
    }

    IEnumerator WallSlideJumpUnlock(float waitTime)
    {
        wallSlideTimeout = true;
        wallSliding = 0;
        if (exitWall != null)
            StopCoroutine(exitWall);
        yield return new WaitForSeconds(waitTime);
        wallSlideTimeout = false;
    }

    IEnumerator WallSlideExitUnlock(float waitTime)
    {
        exitingWallSlide = true;
        yield return new WaitForSeconds(waitTime);
        wallSlideTimeout = true;
        wallSliding = 0;
        yield return new WaitForSeconds(waitTime);
        wallSlideTimeout = false;
        exitingWallSlide = false;
    }

    IEnumerator RegainXControl(float seconds)
    {
        playerControlX = 0f;
        float controlRate = 0.01f / seconds;
        while (playerControlX < 1f)
        {
            yield return new WaitForSeconds(0.01f);
            playerControlX += controlRate;
        }
    }

    IEnumerator KnockbackTimer(float seconds)
    {
        StartCoroutine(RegainXControl(seconds / 2));
        yield return new WaitForSeconds(seconds);
    }

    public IEnumerator StunTimer(float seconds)
    {
        stuned = true;
        yield return new WaitForSeconds(seconds);
        stuned = false;
    }

    public void DamageKnokback(Vector3 source, float hitForce)
    {
        float Xdirection = transform.position.x - source.x;
        Xdirection = Mathf.Sign(Xdirection);

        // float hitForce = 10f;

        _velocity.y = Mathf.Sqrt(0.5f * hitForce * -gravity);
        _velocity.x = Xdirection * hitForce * 2f;
        // StartCoroutine(WallSlideJumpUnlock(0.1f));
        GetComponent<CinemachineImpulseSource>().GenerateImpulse();
        StartCoroutine(KnockbackTimer(0.5f));
    }

    public void DamageKnokbackFromHazard()
    {
        float hitForce = 20f;
        _velocity.y = Mathf.Sqrt(0.5f * hitForce * -gravity);
        // StartCoroutine(WallSlideJumpUnlock(0.1f));
        GetComponent<CinemachineImpulseSource>().GenerateImpulse();
        StartCoroutine(KnockbackTimer(0.2f));
    }

    // IEnumerator StartSequence()
    // {
    // 	stuned = true;
    // 	sprite.gameObject.SetActive(false);
    // 	yield return new WaitForSeconds(1f);
    // 	sprite.gameObject.SetActive(true);
    // 	stuned = false;
    // 	_velocity.y = Mathf.Sqrt( 2f * emergeHeight * -gravity );
    // 	underground = false;
    // 	audioManager.Play("Dig");
    // 	Instantiate(emergePS, transform.position - Vector3.up, transform.rotation);
    // 	GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    // 	_controller.move( _velocity * Time.deltaTime );
    // }

    // public void Pause() {
    // 	if (pauseCanvas != null) {
    // 		if (paused) {
    // 			audioManager.ResumeMusic();
    // 			Time.timeScale = 1f;
    // 			paused = false;
    // 			pauseCanvas.SetActive(false);
    // 			EventSystem.current.SetSelectedGameObject(null);
    // 		}
    // 		else {
    // 			audioManager.PauseMusic();
    // 			Time.timeScale = 0f;
    // 			paused = true;
    // 			pauseCanvas.SetActive(true);
    // 			EventSystem.current.SetSelectedGameObject(pauseCanvas.transform.GetChild(0).GetChild(0).GetChild(0).gameObject);
    // 		}
    // 	}
    // }
}

