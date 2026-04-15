using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator anim;
    [SerializeField] private Renderer[] characterRenderers;
    [SerializeField] private Joystick joystick;
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float atkDamage = 20f;
    public float attackSpeedMultiplier = 1f;
    public float moveSpeed = 6f;
    [SerializeField] private HealthBarController healthBar;
    public float shootRotationOffset = 800f;

    [Header("Level")]
    public int hpLevel = 0;
    public int atkLevel = 0;
    public int atkSpeedLevel = 0;
    public int moveSpeedLevel = 0;

    [Header("Ability Levels")]
    public int multiShotCDLvl;
    public int multiShotDurLvl;
    public int multiShotCountLvl;
    public int burnCDLvl;
    public int burnDurLvl;
    public int burnDmgLvl;
    public int speedCDLvl;
    public int speedDurLvl;
    public int speedMultLvl;
    public int ricoCDLvl;
    public int ricoDurLvl;
    public int ricoCountLvl;
    public int rageCDLvl;
    public int rageDurLvl;
    public int ragePowerLvl;

    [Header("Ability")]
    public bool isMultiActive;
    public bool isBurnActive;
    public bool isSpeedActive;
    public bool isRicoActive;
    public bool isRageActive;
    private bool isRageShadowMulti;
    public bool isRageShadowBurn;
    public bool isRageShadowSpeed;
    public bool isRageShadowRico;
    
    private bool isDashing = false;
    private float dashCD = 0f;
    private float maxDashCD = 2f; 
    private float gravity = -19.62f;
    private float verticalVelocity;
    private Vector3 lastMoveDirection = Vector3.forward;

    [Header("Dash")]
    [SerializeField] private Image dashOverlay; 
    [SerializeField] private TMP_Text dashText;
    
    private Image actualDashOverlayImage; 

    [Header("Fight")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float detectionRange = 10f;
    private Transform currentTarget;
    private float nextFireTime;

    void Start()
    {
        LevelCheck();
        currentHealth = maxHealth;
        healthBar.UpdateHealth(currentHealth, maxHealth);

        if (dashOverlay != null)
        {
            Transform t = dashOverlay.transform.Find("Cooldown Overlay");
            if (t != null)
            {
                actualDashOverlayImage = t.GetComponent<Image>();
                if (actualDashOverlayImage != null) actualDashOverlayImage.fillAmount = 0;
            }
        }
        if (dashText != null) dashText.text = "";
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused) return;

        HandleInput();
        HandleMovement();
        HandleCombat();

        if (dashCD > 0)
        {
            dashCD -= Time.deltaTime;
        }
        else
        {
            dashCD = 0;
        }

        if (actualDashOverlayImage != null)
        {
            if (dashCD > 0)
            {
                actualDashOverlayImage.fillAmount = dashCD / maxDashCD;
                if (dashText != null) dashText.text = Mathf.CeilToInt(dashCD).ToString();
            }
            else
            {
                actualDashOverlayImage.fillAmount = 0;
                if (dashText != null) dashText.text = "";
            }
        }
    }

    public void LevelCheck()
    {
        maxHealth = 100f + (hpLevel * 25f);
        atkDamage = 25f + (atkLevel * 5f);
        attackSpeedMultiplier = 1f + (atkSpeedLevel * 0.25f);
        moveSpeed = 6f + (moveSpeedLevel * 2f);
        healthBar.UpdateHealth(currentHealth, maxHealth);
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) AbilityManager.Instance.TryActivateAbility(0); 
        if (Input.GetKeyDown(KeyCode.Alpha2)) AbilityManager.Instance.TryActivateAbility(1); 
        if (Input.GetKeyDown(KeyCode.Alpha3)) AbilityManager.Instance.TryActivateAbility(2); 
        if (Input.GetKeyDown(KeyCode.Alpha4)) AbilityManager.Instance.TryActivateAbility(3); 
        if (Input.GetKeyDown(KeyCode.Alpha5)) AbilityManager.Instance.TryActivateAbility(4); 
        
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && dashCD == 0) StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        dashCD = maxDashCD;
        
        float dashTime = 0.2f;
        float dashPower = moveSpeed * 3f;

        while (dashTime > 0)
        {
            controller.Move(lastMoveDirection * dashPower * Time.deltaTime);
            dashTime -= Time.deltaTime;
            yield return null; 
        }
        
        isDashing = false;
    }

    public void Dash()
    {
        if (!isDashing && dashCD <= 0)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private void HandleMovement()
    {
        if (isDashing) return;

        float hInput = Input.GetAxisRaw("Horizontal");
        float vInput = Input.GetAxisRaw("Vertical");

        if (joystick != null)
        {
            if (hInput != 0 || vInput != 0)
            {
                Vector2 inputVector = new Vector2(hInput, vInput).normalized;
                
                var background = joystick.GetComponent<RectTransform>();
                var handle = background.GetChild(0).GetComponent<RectTransform>();
                float radius = background.sizeDelta.x * 0.5f;
                
                handle.anchoredPosition = inputVector * radius;
            }
            else if (!Input.anyKey)
            {
                var background = joystick.GetComponent<RectTransform>();
                var handle = background.GetChild(0).GetComponent<RectTransform>();
                handle.anchoredPosition = Vector2.zero;
            }
        }

        float h = (hInput != 0) ? hInput : joystick.Horizontal;
        float v = (vInput != 0) ? vInput : joystick.Vertical;

        Vector3 move = new Vector3(h, 0, v);
        if (move.sqrMagnitude > 1f) move.Normalize();

        if (controller.isGrounded) verticalVelocity = -0.5f;
        else verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        if (move.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = move.normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lastMoveDirection), 15f * Time.deltaTime);
            anim.SetFloat("Speed", move.magnitude);
        }
        else
        {
            anim.SetFloat("Speed", 0f);
        }
    }

    private void HandleCombat()
    {
        FindTarget();
        if (currentTarget != null && !isMoving() && Time.time >= nextFireTime)
        {
            Vector3 targetPos = currentTarget.position;
            targetPos.y = transform.position.y; 
            
            Vector3 targetDir = (targetPos - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            transform.rotation = targetRotation * Quaternion.Euler(0, shootRotationOffset, 0);

            Shoot();
            nextFireTime = Time.time + (1f / attackSpeedMultiplier);
        }
    }

    private void Shoot()
    {
        anim.SetTrigger("Attack");
        
        int count = AbilityManager.Instance.GetArrowCount(); 
        float finalDmg = atkDamage;

        if (AbilityManager.Instance.IsRageActive()) finalDmg *= (1.25f + (ragePowerLvl * 0.01f));

        for (int i = 0; i < count; i++)
        {
            float angle = (i - (count - 1) * 0.5f) * 10f;
            Quaternion rot = Quaternion.Euler(0, angle, 0);
            
            Vector3 targetPos = currentTarget.position;
            targetPos.y = firePoint.position.y; 
            
            Vector3 dir = rot * (targetPos - firePoint.position).normalized;

            GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.LookRotation(dir));
            Arrow script = arrow.GetComponent<Arrow>();
            
            float bDmg = AbilityManager.Instance.GetBurnDamage();
            float bDur = AbilityManager.Instance.GetBurnDuration();
            int rCount = AbilityManager.Instance.GetBounceCount(); 
            float rLoss = AbilityManager.Instance.GetBounceLoss();

            script.Setup(dir, bDur > 0, bDur, bDmg, rCount, finalDmg, rLoss);
        }
    }

    private bool isMoving() => (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f || joystick.Direction.sqrMagnitude > 0.01f);

    private void FindTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float dist = Mathf.Infinity;
        currentTarget = null;
        foreach (var e in enemies)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < dist && d < detectionRange) { dist = d; currentTarget = e.transform; }
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        healthBar.UpdateHealth(currentHealth, maxHealth);
        if (currentHealth <= 0) Die();
    }

    private void Die() => GameManager.Instance.GameOver();

    public void FullyHeal() { currentHealth = maxHealth; healthBar.UpdateHealth(currentHealth, maxHealth); }
}