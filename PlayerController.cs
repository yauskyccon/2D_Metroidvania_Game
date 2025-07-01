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
    private int jumpBufferCounter = 0; //stores the jump button input
    [SerializeField] private int jumpBufferFrames; //sets the max amount of frames the jump buffer input is stored
    private float coyoteTimeCounter = 0; //stores the Grounded() bool
    [SerializeField] private float coyoteTime; //sets the max amount of frames the Grounded() bool is stored
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJumps;
    private float gravity;
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
    [SerializeField] private Transform SideAttackTransform; //the middle of the side attack area
    [SerializeField] private Vector2 SideAttackArea; //how large the area of side attack is

    [SerializeField] private Transform UpAttackTransform; //the middle of the up attack area
    [SerializeField] private Vector2 UpAttackArea; //how large the area of side attack is

    [SerializeField] private Transform DownAttackTransform; //the middle of the down attack area
    [SerializeField] private Vector2 DownAttackArea; //how large the area of down attack is

    [SerializeField] LayerMask attackableLayer;
    [SerializeField] private float timeBetweenAttack;
    private float timeSinceAttack;
    [SerializeField] private float damage;
    [SerializeField] private GameObject hitEffect;

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
    public int maxTotalHealth = 10;
    public int heartShards;
    [SerializeField] GameObject bloodSpurt;
    [SerializeField] float hitFlashSpeed;
    public delegate void OnHealthChangedDelegate();
    [HideInInspector] public OnHealthChangedDelegate onHealthChangedCallback;
    float healTimer;
    [SerializeField] float timeToHeal;
    [Space(5)]

    [Header("Mana Settings")]
    [SerializeField] UnityEngine.UI.Image manaStorage;
    [SerializeField] float mana;
    [SerializeField] float manaDrainSpeed;
    [SerializeField] float manaGain;
    public bool halfMana;

    public ManaOrbsHandler manaOrbsHandler;
    public int orbShard;
    public int manaOrbs;
    [Space(5)]

    [Header("Spell Settings")]
    [SerializeField] float manaSpellCost = 0.3f;
    [SerializeField] float timeBetweenCast = 0.5f;
    [SerializeField] float spellDamage;
    [SerializeField] float downSpellForce;
    [SerializeField] GameObject sideSpellFireball;
    [SerializeField] GameObject upSpellExplosion;
    [SerializeField] GameObject downSpellFireball;
    float timeSinceCast;
    float castOrHealtimer;
    [Space(5)]

    [Header("Camera Stuff")]
    [SerializeField] private float playerFallSpeedThreshold = -10;
    [Space(5)]

    [Header("Audio")]
    [SerializeField] AudioClip landingSound;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip dashAndAttackSound;
    [SerializeField] AudioClip spellCastSound;
    [SerializeField] AudioClip hurtSound;
    private bool landingSoundPlayed;

    [HideInInspector] public PlayerStateList pState;
    public Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    AudioSource audioSource;

    //Input Variables
    private float xAxis, yAxis;
    private bool attack = false;
    private bool canFlash = true;
    bool openMap;
    bool openInventory;

    //creates a singleton of the PlayerController
    public static PlayerController Instance;

    //unlocking 
    public bool unlockedWallJump;
    public bool unlockedDash;
    public bool unlockedVarJump;

    public bool unlockedSideCast;
    public bool unlockedUpCast;
    public bool unlockedDownCast;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        DontDestroyOnLoad(gameObject);

    }

    // Start is called before the first frame update
    void Start()
    {
        pState = GetComponent<PlayerStateList>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        manaOrbsHandler = FindObjectOfType<ManaOrbsHandler>();
        audioSource = GetComponent<AudioSource>();
        gravity = rb.gravityScale;
        Mana = mana;
        manaStorage.fillAmount = Mana;

        SaveData.Instance.LoadPlayerData();
        if (manaOrbs > 3)
        {
            manaOrbs = 3;
        }

        if (halfMana)
        {
            UIManager.Instance.SwitchMana(UIManager.ManaState.HalfMana);
        }
        else
        {
            UIManager.Instance.SwitchMana(UIManager.ManaState.FullMana);
        }

        if (onHealthChangedCallback != null) onHealthChangedCallback.Invoke();

        if (Health == 0)
        {
            pState.alive = false;
            GameManager.Instance.RespwanPlayer();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    // Update is called once per frame
    void Update()
    {
        if (pState.cutscene || GameManager.Instance.isPaused) return;

        if (pState.alive)
        {
            GetInputs();
            ToggleMap();
            ToggleInventory();
        }
        UpdateJumpVariables();
        UpdateCameraYDampForPlayerFall();

        if (pState.dashing) return;
        FlashWhileInvincible();
        if (!pState.alive) return;
        if (!isWallJumping)
        {
            Move();
        }
        
        Heal();
        CastSpell();
        if (pState.healing) return;
        if (!isWallJumping)
        {
            Flip();
            Jump();
        }

        if (unlockedWallJump)
        {
            WallSlide();
            WallJump();
        }

        if (unlockedDash)
        {
            StartDash();
        }

        Attack();
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if(_other.GetComponent<Enemy>() != null && pState.casting)
        {
            _other.GetComponent<Enemy>().EnemyHit(spellDamage, (_other.transform.position - transform.position).normalized, -recoilYSpeed);
        }
    }

    private void FixedUpdate()
    {
        if (pState.cutscene) return;

        if (pState.dashing || pState.healing) return;
        Recoil();
    }

    void GetInputs()
    {
        if (GameManager.Instance.isPaused || GameManager.isStopped) return;

        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");
        attack = Input.GetButtonDown("Attack");
        openMap = Input.GetButton("Map");
        openInventory = Input.GetButton("Inventory");

        if (Input.GetButton("Cast/Heal"))
        {
            castOrHealtimer += Time.deltaTime;
        }
    }

    void ToggleMap()
    {
        if (openMap)
        {
            UIManager.Instance.mapHandler.SetActive(true);
        }
        else
        {
            UIManager.Instance.mapHandler.SetActive(false);
        }
    }

    void ToggleInventory()
    {
        if (openInventory)
        {
            UIManager.Instance.inventory.SetActive(true);
        }
        else
        {
            UIManager.Instance.inventory.SetActive(false);
        }
    }

    void Flip()
    {
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
            pState.lookingRight = false;
        }
        if (xAxis > 0)
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

    void UpdateCameraYDampForPlayerFall()
    {
        //if falling past a certain speed threshold
        if (rb.velocity.y < playerFallSpeedThreshold && !CameraManager.Instance.isLerpingYDamping && !CameraManager.Instance.hasLerpedYDamping) StartCoroutine(CameraManager.Instance.LerpYDamping(true));
        //if standing still or moving up
        if (rb.velocity.y >= 0 && !CameraManager.Instance.isLerpingYDamping && CameraManager.Instance.hasLerpedYDamping)
        {
            //reset camera function
            CameraManager.Instance.hasLerpedYDamping = false;
            StartCoroutine(CameraManager.Instance.LerpYDamping(false));
        }
    }

    void StartDash()
    {
        if(Input.GetButtonDown("Dash") && canDash && !dashed)
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
        audioSource.PlayOneShot(dashAndAttackSound);
        rb.gravityScale = 0;
        int _dir = pState.lookingRight ? 1 : -1;
        rb.velocity = new Vector2(_dir * dashSpeed, 0);
        if (Grounded()) Instantiate(dashEffect, transform);
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = gravity;
        pState.dashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public IEnumerator WalkIntoNewScene(Vector2 _exitDir, float _delay)
    {
        pState.invincible = true;

        //If exit direction is upwards
        if(_exitDir.y > 0) rb.velocity = jumpForce * _exitDir;

        //If exit direction requires horizontal movement
        if (_exitDir.x != 0)
        {
            xAxis = _exitDir.x > 0 ? 1 : -1;

            Move();
        }

        Flip();
        yield return new WaitForSeconds(_delay);
        pState.invincible = false;
        pState.cutscene = false;
    }

    void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if(attack && timeSinceAttack >= timeBetweenAttack)
        {
            int _recoilLeftOrRight = pState.lookingRight ? 1 : -1;

            timeSinceAttack = 0;
            anim.SetTrigger("Attacking");
            audioSource.PlayOneShot(dashAndAttackSound);

            if (yAxis == 0 || yAxis < 0 && Grounded())
            {
                Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, Vector2.right * _recoilLeftOrRight, recoilXSpeed);
                Instantiate(hitEffect, SideAttackTransform);
            }
            else if(yAxis > 0)
            {
                Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, Vector2.up, recoilYSpeed);
                HitEffectAngle(hitEffect, 80, UpAttackTransform);
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, Vector2.down, recoilYSpeed);
                HitEffectAngle(hitEffect, -90, DownAttackTransform);
            }
        }
    }

    void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilBool, Vector2 _recoilDir, float _recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);
        /*List<Enemy> hitEnemies = new List<Enemy>();

        bool hitEnemy = false;

        foreach (Collider2D collider in objectsToHit)
        {
            Enemy e = collider.GetComponent<Enemy>();
            if (e && !hitEnemies.Contains(e))
            {
                e.EnemyHit(damage, (transform.position - collider.transform.position).normalized, _recoilStrength);
                hitEnemies.Add(e);
                hitEnemy = true; // Set hitEnemy flag to true if an enemy is hit

            }
        }

        // Apply recoil only if the player hit an enemy
        if (hitEnemy)
        {
            _recoilDir = true;
        }*/

        if (objectsToHit.Length > 0)
        {
            _recoilBool = true;
        }

        for(int i =0; i< objectsToHit.Length; i++)
        {
            if(objectsToHit[i].GetComponent<Enemy>() != null)
            {
                objectsToHit[i].GetComponent<Enemy>().EnemyHit(damage, _recoilDir, _recoilStrength);

                if (objectsToHit[i].CompareTag("Enemy"))
                {
                    if (!halfMana && Mana < 1 || (halfMana && Mana < 0.5))
                    {
                        Mana += manaGain;
                        manaStorage.fillAmount = Mana;
                    }
                    else
                    {
                        mana = 0.5f;
                        manaOrbsHandler.UpdateMana(manaGain * 3);
                    }
                    /*if (!halfMana)
                    {
                        mana += manaGain;
                        manaStorage.fillAmount = Mana;
                        if (mana > 1f) mana = 1f;
                    }
                    else
                    {
                        if (mana < 0.5f)
                        {
                            mana += manaGain;
                            manaStorage.fillAmount = Mana;
                        }
                        else mana = 0.5f;
                    }*/

                }
            }
        }
    }

    void HitEffectAngle(GameObject _hitEffect, int _effectAngle, Transform _attackTransform)
    {
        _hitEffect = Instantiate(_hitEffect, _attackTransform);
        _hitEffect.transform.eulerAngles = new Vector3(0, 0, _effectAngle);
        _hitEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
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

        //stop recoil
        if(pState.recoilingX && stepsXRecoiled < recoilXSteps)
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

    public void TakeDamage(float _damage)
    {
        if (pState.alive)
        {
            audioSource.PlayOneShot(hurtSound);

            Health -= Mathf.RoundToInt(_damage);
            if(Health <= 0)
            {
                Health = 0;
                StartCoroutine(Death());
            }
            else StartCoroutine(StopTakingDamage());
        }
    }

    IEnumerator StopTakingDamage()
    {
        pState.invincible = true;
        GameObject _bloodSpurtParticles = Instantiate(bloodSpurt, transform.position, Quaternion.identity);
        Destroy(_bloodSpurtParticles, 1.5f);
        anim.SetTrigger("takeDamage");
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
        if (pState.invincible && !pState.cutscene)
        {
            if (Time.timeScale > 0.2 && canFlash) StartCoroutine(Flash());
        }
        else sr.enabled = true;
    }


    IEnumerator Death()
    {
        pState.alive = false;
        Time.timeScale = 1f;
        GameObject _bloodSpurtParticles = Instantiate(bloodSpurt, transform.position, Quaternion.identity);
        Destroy(_bloodSpurtParticles, 1.5f);
        anim.SetTrigger("Death");
        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        GetComponent<BoxCollider2D>().enabled = false;

        yield return new WaitForSeconds(0.9f);
        StartCoroutine(UIManager.Instance.ActivateDeathScreen());

        yield return new WaitForSeconds(0.1f);
        Instantiate(GameManager.Instance.shade, transform.position, Quaternion.identity);

        SaveData.Instance.SavePlayerData();
    }

    public void Respawned()
    {
        if (!pState.alive)
        {
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            GetComponent<BoxCollider2D>().enabled = true;
            pState.alive = true;
            halfMana = true;
            UIManager.Instance.SwitchMana(UIManager.ManaState.HalfMana);
            Mana = 0;
            Health = maxHealth;
            anim.Play("player_Idle");
        }
    }

    public void RestoreMana()
    {
        halfMana = false;
        UIManager.Instance.SwitchMana(UIManager.ManaState.FullMana);
    }

    public int Health
    {
        get { return health; }
        set
        {
            if(health!= value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);

               if(onHealthChangedCallback != null)
                {
                    onHealthChangedCallback.Invoke();
                }
            }
        }
    }

    void Heal()
    {
        if(Input.GetButton("Cast/Heal") && castOrHealtimer > 0.05f && Health < maxHealth && Mana > 0 && Grounded() && !pState.dashing)
        {
            pState.healing = true;
            anim.SetBool("Healing", true);

            healTimer += Time.deltaTime;
            if(healTimer >= timeToHeal)
            {
                Health++;
                healTimer = 0;
            }

            manaOrbsHandler.usedMana = true;
            manaOrbsHandler.countDown = 3f;
            Mana -= Time.deltaTime * manaDrainSpeed;
        }
        else
        {
            pState.healing = false;
            anim.SetBool("Healing", false);
            healTimer = 0;
        }
    }

    public float Mana
    {
        get { return mana; }
        set
        {
            if (mana != value)
            {
                if (!halfMana)
                {
                    mana = Mathf.Clamp(value, 0, 1);
                }
                else if (halfMana)
                {
                    mana = Mathf.Clamp(value, 0, 0.5f);
                }
                manaStorage.fillAmount = Mana;
            }
        }
    }

    void CastSpell()
    {
        if(Input.GetButtonUp("Cast/Heal") && castOrHealtimer <= 0.5f && timeSinceCast >= timeBetweenCast && Mana >= manaSpellCost)
        {
            pState.casting = true;
            timeSinceCast = 0;
            StartCoroutine(CastCoroutine());
        }
        else
        {
            timeSinceCast += Time.deltaTime;
        }

        if (!Input.GetButton("Cast/Heal"))
        {
            castOrHealtimer = 0;
        }

        if (Grounded())
        {
            downSpellFireball.SetActive(false);
        }

        if (downSpellFireball.activeInHierarchy)
        {
            rb.velocity += downSpellForce * Vector2.down;
        }
    }

    IEnumerator CastCoroutine()
    {
        //side cast
        if ((yAxis == 0 || (yAxis < 0 && Grounded())) && unlockedSideCast)
        {
            audioSource.PlayOneShot(spellCastSound);

            anim.SetBool("Casting", true);
            yield return new WaitForSeconds(0.15f);
            GameObject _fireBall = Instantiate(sideSpellFireball, SideAttackTransform.position, Quaternion.identity);

            //flip fireball
            if (pState.lookingRight)
            {
                _fireBall.transform.eulerAngles = Vector3.zero; // if facing right, fireball continues as per normal
            }
            else
            {
                _fireBall.transform.eulerAngles = new Vector2(_fireBall.transform.eulerAngles.x, 180);
                //if not facing right, rotate the fireball 180 deg
            }
            pState.recoilingX = true;

            Mana -= manaSpellCost;
            manaOrbsHandler.usedMana = true;
            manaOrbsHandler.countDown = 3f;
            yield return new WaitForSeconds(0.35f);
        }

        //up cast
        else if (yAxis > 0 && unlockedUpCast)
        {
            audioSource.PlayOneShot(spellCastSound);

            anim.SetBool("Casting", true);
            yield return new WaitForSeconds(0.15f);

            Instantiate(upSpellExplosion, transform);
            rb.velocity = Vector2.zero;

            Mana -= manaSpellCost;
            manaOrbsHandler.usedMana = true;
            manaOrbsHandler.countDown = 3f;
            yield return new WaitForSeconds(0.35f);
        }

        //down cast
        else if ((yAxis < 0 && !Grounded()) && unlockedDownCast)
        {
            audioSource.PlayOneShot(spellCastSound);

            anim.SetBool("Casting", true);
            yield return new WaitForSeconds(0.15f);

            downSpellFireball.SetActive(true);

            Mana -= manaSpellCost;
            manaOrbsHandler.usedMana = true;
            manaOrbsHandler.countDown = 3f;
            yield return new WaitForSeconds(0.35f);
        }

        anim.SetBool("Casting", false);
        pState.casting = false;
    }

    public bool Grounded()
    {
        if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround)) return true;
        else return false;
    }

    void Jump()
    {
        if (!pState.jumping && jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            if (Input.GetButtonDown("Jump"))
            {
                audioSource.PlayOneShot(jumpSound);
            }

            rb.velocity = new Vector3(rb.velocity.x, jumpForce);

            pState.jumping = true;
        }
        if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump") && unlockedVarJump)
        {
            audioSource.PlayOneShot(jumpSound);

            pState.jumping = true;

            airJumpCounter++;

            rb.velocity = new Vector3(rb.velocity.x, jumpForce);
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 3)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);

            pState.jumping = false;
        }

        anim.SetBool("Jumping", !Grounded());
    }

    void UpdateJumpVariables()
    {
        if (Grounded())
        {
            if (!landingSoundPlayed)
            {
                landingSoundPlayed = true;
                audioSource.PlayOneShot(landingSound);
            }
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            landingSoundPlayed = false;

        }

        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferFrames;
        else jumpBufferCounter--; 
    }

    private bool Walled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    void WallSlide()
    {
        if (Walled() && !Grounded() && xAxis != 0)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else isWallSliding = false;
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
            audioSource.PlayOneShot(jumpSound);
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);

            dashed = false;
            airJumpCounter = 0;

            pState.lookingRight = !pState.lookingRight;

            float jumpDirection = pState.lookingRight ? 0 : 180;
            transform.eulerAngles = new Vector2(transform.eulerAngles.x, jumpDirection);

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }
    void StopWallJumping()
    {
        isWallJumping = false;
        transform.eulerAngles = new Vector2(transform.eulerAngles.x, 0);
    }
}
