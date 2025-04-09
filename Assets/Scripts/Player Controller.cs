using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement Settings")]
    [SerializeField] private float walkSpeed = 1;
    [Space(5)]



    [Header("Vertical Movement Settings")]
    [SerializeField] private float jumpForce = 45f;
    private float jumpBufferCounter = 0;
    [SerializeField] private float jumpBufferFrames;
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime;
    private int airJumpCounter = 0;
    public int maxAirJumps;
    [Space(5)]



    [Header("Landing Effect Settings")]
    [SerializeField] private GameObject landingEffectPrefab;
    [SerializeField] private Transform landingEffectSpawnPoint;
    [SerializeField] private bool followPlayer = false; // Должен ли эффект следовать за игроком?
    private bool wasGroundedLastFrame;
    private GameObject currentEffect;
    [SerializeField] private float landingEffectCooldown = 0.5f;
    private float lastLandingEffectTime = -Mathf.Infinity;
    [Space(5)]



    [Header("Wall Jump Settings")]
    [SerializeField] private float wallSlidingSpeed = 2f;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallJumpingDuration;
    [SerializeField] private Vector2 wallJumpingPower;
    float wallJumpingDirection;
    bool isWallSliding;
    bool isWallJumping;
    [Space(5)]



    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;
    [Space(5)]



    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;
    [SerializeField] GameObject dashEffect;
    private bool canDash = true, dashed;
    [Space(5)]



    [Header("Attack Settings")]
    [SerializeField] private Transform SideAttackTransform;
    [SerializeField] private Vector2 SideAttackArea;

    [SerializeField] private Transform UpAttackTransform;
    [SerializeField] private Vector2 UpAttackArea;

    [SerializeField] private Transform DownAttackTransform;
    [SerializeField] private Vector2 DownAttackArea;

    [SerializeField] private LayerMask attackableLayer;

    [SerializeField] private float timeBetweenAttack;
    private float timeSinceAttack;

    [SerializeField] private float damage;

    [SerializeField] private GameObject slashEffect;

    bool restoreTime;
    float restoreTimeSpeed;
    [Space(5)]



    [Header("Recoil Settings")]
    [SerializeField] private int recoilXSteps = 5;
    [SerializeField] private int recoilYSteps = 5;

    [SerializeField] private float recoilXSpeed = 100;
    [SerializeField] private float recoilYSpeed = 100;

    private int stepsXRecoiled, stepsYRecoiled;
    [Space(5)]

    [Header("Health Settings")]
    public int health;
    public int maxHealth;
    [SerializeField] GameObject bloodSpurt;
    [SerializeField] float hitFlashSpeed;
    public delegate void OnHealthChangedDelegate();
    [HideInInspector] public OnHealthChangedDelegate onHealthChangedCallback;

    float healTimer;
    [SerializeField] float timeToHeal;
    [Space(5)]

    [Header("Other Settings")]
    private Vector3 lastSafePosition;
    [SerializeField] private string safeTag = "Safe";
    [SerializeField] private string spikeTag = "Spike";
    [SerializeField] private float respawnDelay = 0.1f;
    private Collider2D lastSafePlatformCollider;

    [HideInInspector] public PlayerStateList pState;
    private Animator anim;
    private Rigidbody2D rb;
    private float gravity;
    private SpriteRenderer sr;
    private bool initiatedWallSlide = false;

    [SerializeField] private Vector2 respawnOffset = new Vector2(0, 0.5f);

    private float xAxis, yAxis;
    private bool attack = false;
    private bool canFlash = true;
    private bool canMove = true;

    public static PlayerController Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        Health = maxHealth;
    }

    void Start()
    {
        pState = GetComponent<PlayerStateList>();

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        anim = GetComponent<Animator>();

        gravity = rb.gravityScale;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    void Update()
    {
        GetInputs();
        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (Grounded() && !wasGroundedLastFrame)
        {
            CreateLandingEffect();
        }
        wasGroundedLastFrame = Grounded();

        UpdateJumpVariables();
        RestoreTimeScale();

        if (pState.dashing) return;
        FlashWhileInvincible();
        Heal();

        if (!isWallJumping)
        {
            Move();
            Flip();
            Jump();
        }

        if (pState.healing) return;
        WallSlide();
        WallJump();
        StartDash();
        Attack();

        if (Grounded())
        {
            RaycastHit2D hit = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY + 0.1f, whatIsGround);
            if (hit.collider != null && hit.collider.CompareTag(safeTag))
            {
                lastSafePlatformCollider = hit.collider;
                lastSafePosition = transform.position; // резервный вариант
            }
        }
    }

    private void FixedUpdate()
    {
        if (pState.dashing || pState.healing) return;
        Recoil();
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");
        attack = Input.GetButtonDown("Attack");
    }

    void Flip()
    {
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
            pState.lookingRight = false;
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
            pState.lookingRight = true;
        }
    }

    private void Move()
    {
        if (pState.healing) rb.velocity = new Vector2(0, 0);
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
        anim.SetBool("Walking", rb.velocity.x != 0 && Grounded());
    }

    void StartDash()
    {
        if (Input.GetButtonDown("Dash") && canDash && !dashed)
        {
            StartCoroutine(Dash());
            dashed = true;
        }

        if (Grounded())
        {
            dashed = false;
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        pState.dashing = true;
        anim.SetTrigger("Dashing");
        rb.gravityScale = 0;
        rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0);
        if (Grounded()) Instantiate(dashEffect, transform);
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = gravity;
        pState.dashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if (attack && timeSinceAttack >= timeBetweenAttack)
        {
            timeSinceAttack = 0;
            anim.SetTrigger("Attacking");

            if (yAxis == 0 || yAxis < 0 && Grounded())
            {
                int RecoilLeftOrRight = pState.lookingRight ? 1 : -1;

                Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, Vector2.right * RecoilLeftOrRight, recoilXSpeed);
                Instantiate(slashEffect, SideAttackTransform);
            }
            else if (yAxis > 0)
            {
                Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, Vector2.up, recoilYSpeed);
                SlashEffectAtAngle(slashEffect, 80, UpAttackTransform);
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, Vector2.down, recoilYSpeed);
                SlashEffectAtAngle(slashEffect, -90, DownAttackTransform);
            }
        }


    }
    void Hit(Transform attackTransform, Vector2 attackArea, ref bool recoilBool, Vector2 recoilDir, float recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(attackTransform.position, attackArea, 0, attackableLayer);
        List<Enemy> hitEnemies = new List<Enemy>();

        if (objectsToHit.Length > 0)
        {
            recoilBool = true;
        }

        for (int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<Enemy>() != null)
            {
                objectsToHit[i].GetComponent<Enemy>().EnemyHit(damage, recoilDir, recoilStrength);
            }
        }
    }

    void SlashEffectAtAngle(GameObject slashEffect, int effectAngle, Transform attackTransform)
    {
        slashEffect = Instantiate(slashEffect, attackTransform);
        slashEffect.transform.eulerAngles = new Vector3(0, 0, effectAngle);
        slashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
    }
    void Recoil()
    {
        if (pState.recoilingX)
        {
            if (pState.lookingRight)
            {
                rb.velocity = new Vector2(-recoilXSpeed, 0);
            }
            else
            {
                rb.velocity = new Vector2(recoilXSpeed, 0);
            }
        }

        if (pState.recoilingY)
        {
            rb.gravityScale = 0;
            if (yAxis < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, -recoilYSpeed);
            }
            airJumpCounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }

        if (pState.recoilingX && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }
        if (pState.recoilingY && stepsYRecoiled < recoilYSteps)
        {
            stepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }

        if (Grounded())
        {
            StopRecoilY();
        }
    }
    void StopRecoilX()
    {
        stepsXRecoiled = 0;
        pState.recoilingX = false;
    }
    void StopRecoilY()
    {
        stepsYRecoiled = 0;
        pState.recoilingY = false;
    }
    public void TakeDamage(float damage)
    {
        Health -= Mathf.RoundToInt(damage);
        StartCoroutine(StopTakingDamage());
    }
    IEnumerator StopTakingDamage()
    {
        pState.invincible = true;
        GameObject bloodSpurtParticles = Instantiate(bloodSpurt, transform.position, Quaternion.identity);
        Destroy(bloodSpurtParticles, 1.5f);
        anim.SetTrigger("TakeDamage");
        yield return new WaitForSeconds(1f);
        pState.invincible = false;
    }

    IEnumerator Flash()
    {
        sr.enabled = !sr.enabled;
        canFlash = false;
        yield return new WaitForSeconds(0.1f);
        canFlash = true;
    }

    void FlashWhileInvincible()
    {
        if (pState.invincible)
        {
            if (Time.timeScale > 0.2 && canFlash)
            {
                StartCoroutine(Flash());
            }
        }
        else
        {
            sr.enabled = true;
        }
    }
    void RestoreTimeScale()
    {
        if (restoreTime)
        {
            if (Time.timeScale < 1)
            {
                Time.timeScale += Time.unscaledDeltaTime * restoreTimeSpeed;
            }
            else
            {
                Time.timeScale = 1;
                restoreTime = false;
            }
        }
    }

    public void HitTimeStop(float newTimeScale, int restoreSpeed, float delay)
    {
        restoreTimeSpeed = restoreSpeed;
        if (delay > 0)
        {
            StopCoroutine(StartTimeAgain(delay));
            StartCoroutine(StartTimeAgain(delay));
        }
        else
        {
            restoreTime = true;
        }
        Time.timeScale = newTimeScale;
    }
    IEnumerator StartTimeAgain(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        restoreTime = true;
    }

    public int Health
    {
        get { return health; }
        set
        {
            if (health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);

                if (onHealthChangedCallback != null)
                {
                    onHealthChangedCallback.Invoke();
                }
            }
        }
    }
    void Heal()
    {
        if (Input.GetButton("Healing") && Health < maxHealth && Grounded() && !pState.dashing)
        {
            pState.healing = true;
            anim.SetBool("Healing", true);

            healTimer += Time.deltaTime;
            if (healTimer >= timeToHeal)
            {
                Health++;
                healTimer = 0;
            }
        }
        else
        {
            pState.healing = false;
            anim.SetBool("Healing", false);
            healTimer = 0;
        }
    }

    public bool Grounded()
    {
        if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void Jump()
    {
        if (!pState.jumping)
        {
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce);
                pState.jumping = true;
            }
            else if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
            {
                pState.jumping = true;
                airJumpCounter++;
                rb.velocity = new Vector3(rb.velocity.x, jumpForce);
            }
        }

        // Если быстро отпустили кнопку, уменьшаем силу прыжка
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.35f); // Раньше было 0.5f, теперь еще меньше
            pState.jumping = false;
        }

        anim.SetBool("Jumping", !Grounded());
    }

    private bool Walled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }
    void WallSlide()
    {
        if (Grounded())
        {
            isWallSliding = false;
            initiatedWallSlide = false;
            return;
        }

        if (Walled() && !Grounded() && xAxis != 0)
        {
            // Стартуем скольжение, если игрок нажал на клавишу вбок около стены
            initiatedWallSlide = true;
        }

        if (Walled() && !Grounded() && initiatedWallSlide)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }
    void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = !pState.lookingRight ? 1 : -1;

            CancelInvoke(nameof(StopWallJumping));
        }
        if (Input.GetButtonDown("Jump") && isWallSliding)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);

            dashed = false;
            airJumpCounter = 0;

            if ((pState.lookingRight && transform.eulerAngles.y == 0) || (!pState.lookingRight && DownAttackTransform.eulerAngles.y != 0))
            {
                pState.lookingRight = !pState.lookingRight;
                int yRotation = pState.lookingRight ? 0 : 180;

                DownAttackTransform.eulerAngles = new Vector2(DownAttackTransform.eulerAngles.x, yRotation);
            }
            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }
    void StopWallJumping()
    {
        isWallJumping = false;
    }

    void UpdateJumpVariables()
    {
        if (Grounded())
        {
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter = jumpBufferCounter - Time.deltaTime * 10;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(spikeTag))
        {
            StartCoroutine(RespawnPlayer());
        }
    }

    IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(respawnDelay);

        TakeDamage(1f);

        // Телепорт к последней безопасной позиции
        rb.velocity = Vector2.zero;
        Vector3 respawnPoint;

        if (lastSafePlatformCollider != null)
        {
            Bounds bounds = lastSafePlatformCollider.bounds;

            // Центр платформы по X и верхняя часть по Y (чтобы игрок не застревал внутри)
            respawnPoint = new Vector3(
                bounds.center.x,
                bounds.max.y + respawnOffset.y,
                transform.position.z
            );
        }
        else
        {
            // Если платформы нет — fallback
            respawnPoint = lastSafePosition + (Vector3)respawnOffset;
        }

        // Устанавливаем позицию
        transform.position = respawnPoint;

        // Отключаем управление на 1 секунду
        canMove = false;

        anim.SetBool("Walking", false);
        anim.SetBool("Jumping", false);
        anim.Play("Idle");

        yield return new WaitForSeconds(0.5f);
        canMove = true;

        // Game Over логика
        if (Health <= 0)
        {
            Debug.Log("Game Over!");
            // Здесь можно вставить перезапуск уровня или переход на экран поражения
        }
    }

    void CreateLandingEffect()
    {
        if (landingEffectPrefab == null || landingEffectSpawnPoint == null) return;

        // Не создавать эффект, если кулдаун не прошел
        if (Time.time < lastLandingEffectTime + landingEffectCooldown) return;

        lastLandingEffectTime = Time.time;

        if (currentEffect != null)
        {
            Destroy(currentEffect);
        }

        currentEffect = Instantiate(landingEffectPrefab, landingEffectSpawnPoint.position, Quaternion.identity);

        if (followPlayer)
        {
            currentEffect.transform.SetParent(transform);
        }
    }
}