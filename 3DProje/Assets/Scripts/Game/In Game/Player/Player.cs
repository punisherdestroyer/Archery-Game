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
    private Vector2 moveInput;
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float atkDamage = 20f;
    public float attackSpeedMultiplier = 1f;
    public float moveSpeed = 6f;
    private HealthBarUI _healthBar;
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
    private float dashSpeed = 12f;
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
    [SerializeField] private float detectionRange = 14f;
    private Transform currentTarget;
    private float nextFireTime;

    // Hedef arama sırasında her kare GC oluşmasını engelleyen sabit boyutlu dizi havuzu.
    private static readonly Collider[] EnemyOverlapBuffer = new Collider[64];
    
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    void Start()
    {
        // Oyuncunun mevcut canı başlangıçta maksimum canına eşitlenir.
        currentHealth = maxHealth;
    
        // Arayüz yöneticisi aktifse oyuncu için dünya üzerinde konumlanacak bir can barı üretilir.
        if (InGameUIController.Instance != null && InGameUIController.Instance.GetMainView() != null)
        {
            _healthBar = new HealthBarUI("packageInGame", "componentHPBar", transform, InGameUIController.Instance.GetMainView(), 2.5f);
            _healthBar?.UpdateValue(currentHealth, maxHealth);
        }
    
        // Oyuncunun seviye bilgileri ve nitelik değerleri kontrol edilip senkronize edilir.
        LevelCheck();

        // Dash butonunun altındaki Cooldown Overlay katmanı hiyerarşik olarak bulunup hazırlanır.
        if (dashOverlay != null)
        {
            Transform t = dashOverlay.transform.Find("Cooldown Overlay");
            if (t != null)
            {
                actualDashOverlayImage = t.GetComponent<Image>();
                if (actualDashOverlayImage != null) actualDashOverlayImage.fillAmount = 0;
            }
        }
        if (dashText != null) dashText.text = string.Empty;
    }

    void Update()
    {
        // Oyun bittiyse veya duraklatıldıysa oyuncu mantıksal döngüleri çalıştırılmaz.
        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused) return;

        // Klavye girdileri, hareket motoru ve savaş mekanikleri her kare tetiklenir.
        HandleInput();
        HandleMovement();
        HandleCombat();

        // Dash yeteneğinin bekleme süresi zaman akışına göre azaltılır.
        if (dashCD > 0)
        {
            dashCD -= Time.deltaTime;
        }
        else
        {
            dashCD = 0;
        }

        // Dash butonunun arayüz üzerindeki doluluk oranı ve kalan saniye metni güncellenir.
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
                if (dashText != null) dashText.text = string.Empty;
            }
        }
    }

    void LateUpdate()
    {
        // Oyuncunun fiziki hareketi bittikten sonra can barının dünyadaki konumu güncellenir.
        if (!GameManager.Instance.IsGameOver && !GameManager.Instance.IsPaused)
        {
            _healthBar?.UpdatePosition();
        }
    }

    public float GetDashCD() => dashCD;
    public float GetMaxDashCD() => maxDashCD;

    public void LevelCheck()
    {
        // Geliştirme seviyelerine göre oyuncunun can, hasar, saldırı hızı ve hareket hızı hesaplanır.
        maxHealth = 100f + (hpLevel * 25f);
        atkDamage = 25f + (atkLevel * 5f);
        attackSpeedMultiplier = 1f + (atkSpeedLevel * 0.2f);
        moveSpeed = 6f + (moveSpeedLevel * 2f);
        _healthBar?.UpdateValue(currentHealth, maxHealth);
    }

    private void HandleInput()
    {
        // Sayı tuşlarına (1-5) basıldığında ilgili yeteneğin aktifleştirilmesi AbilityManager üzerinden denenir.
        if (Input.GetKeyDown(KeyCode.Alpha1)) AbilityManager.Instance.TryActivateAbility(0); 
        if (Input.GetKeyDown(KeyCode.Alpha2)) AbilityManager.Instance.TryActivateAbility(1); 
        if (Input.GetKeyDown(KeyCode.Alpha3)) AbilityManager.Instance.TryActivateAbility(2); 
        if (Input.GetKeyDown(KeyCode.Alpha4)) AbilityManager.Instance.TryActivateAbility(3); 
        if (Input.GetKeyDown(KeyCode.Alpha5)) AbilityManager.Instance.TryActivateAbility(4); 
        
        // Space tuşuna basıldığında atılma koşulları uygunsa asenkron Dash süreci başlatılır.
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && dashCD == 0) StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        dashCD = maxDashCD;
        
        float dashTime = 0.2f;

        // 0.2 saniye boyunca oyuncu son hareket ettiği yöne doğru yüksek hızda kaydırılır.
        while (dashTime > 0)
        {
            controller.Move(lastMoveDirection * dashSpeed * Time.deltaTime);
            dashTime -= Time.deltaTime;
            yield return null; 
        }
        
        isDashing = false;
    }

    public void Dash()
    {
        // Harici arayüz butonlarından çağrılabilecek güvenli Dash tetikleme fonksiyonu.
        if (!isDashing && dashCD <= 0)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private void HandleMovement()
    {
        if (isDashing) return;

        // Joystick veya klavye üzerinden gelen hareket girdi yönleri normale çevrilerek okunur.
        if (InGameUIController.Instance != null)
        {
            moveInput = InGameUIController.Instance.GetJoystickAxis();
        }
        else
        {
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        float h = moveInput.x;
        float v = moveInput.y;

        Vector3 move = new Vector3(h, 0, v);
        if (move.sqrMagnitude > 1f) move.Normalize();

        if (controller != null)
        {
            // Karakterin havada kalmaması için zemin kontrolüyle beraber yerçekimi ivmesi uygulanır.
            if (controller.isGrounded) verticalVelocity = -0.5f;
            else verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = move * moveSpeed;
            velocity.y = verticalVelocity;

            controller.Move(velocity * Time.deltaTime);
        }

        // Eğer bir hareket girdisi mevcutsa oyuncunun yüzü o yöne çevrilir ve yürüme animasyonu oynatılır.
        if (move.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = move.normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lastMoveDirection), 15f * Time.deltaTime);
            if (anim != null) anim.SetFloat(SpeedHash, move.magnitude);
        }
        else
        {
            if (anim != null) anim.SetFloat(SpeedHash, 0f);
        }
    }

    private void HandleCombat()
    {
        // En yakın düşman taranır, eğer oyuncu duruyorsa ve saldırı hızı süresi dolduysa atış gerçekleştirilir.
        FindTarget();
        if (currentTarget != null && !isMoving() && Time.time >= nextFireTime)
        {
            Vector3 targetPos = currentTarget.position;
            targetPos.y = transform.position.y; 
            
            // Oyuncunun yüzü ateş etmeden hemen önce hedefe doğru döndürülür.
            Vector3 targetDir = (targetPos - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            transform.rotation = targetRotation * Quaternion.Euler(0, shootRotationOffset, 0);

            Shoot();
            nextFireTime = Time.time + (1f / attackSpeedMultiplier);
        }
    }

    private void Shoot()
    {
        // Saldırı animasyonu tetiklenir.
        anim.SetTrigger(AttackHash);
        
        // Aktif yeteneklere göre fırlatılacak ok sayısı ve rage durumundaki hasar çarpanları hesaplanır.
        int count = AbilityManager.Instance.GetArrowCount(); 
        float finalDmg = atkDamage;

        if (AbilityManager.Instance.IsRageActive()) finalDmg *= (1.25f + (ragePowerLvl * 0.01f));

        // Hesaplanan ok sayısı kadar, yelpaze şeklinde açıyla ok nesneleri dünyaya getirilir.
        for (int i = 0; i < count; i++)
        {
            float angle = (i - (count - 1) * 0.5f) * 10f;
            Quaternion rot = Quaternion.Euler(0, angle, 0);
            
            Vector3 targetPos = currentTarget.position;
            targetPos.y = firePoint.position.y; 
            
            Vector3 dir = rot * (targetPos - firePoint.position).normalized;

            GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.LookRotation(dir));
            
            if (arrow.TryGetComponent(out Arrow script))
            {
                // Okun taşıyacağı yakma hasarı, sekme sayısı ve azalış oranları verilerek ok kurulumu tamamlanır.
                float bDmg = AbilityManager.Instance.GetBurnDamage();
                float bDur = AbilityManager.Instance.GetBurnDuration();
                int rCount = AbilityManager.Instance.GetBounceCount(); 
                float rLoss = AbilityManager.Instance.GetBounceLoss();

                script.Setup(dir, bDur > 0, bDur, bDmg, rCount, finalDmg, rLoss);
            }
        }
    }

    private bool isMoving()
    {
        // Oyuncunun o an hareket edip etmediğini girdi eksenlerinin karekök uzunluğuna bakarak döndürür.
        if (InGameUIController.Instance != null)
        {
            return InGameUIController.Instance.GetJoystickAxis().sqrMagnitude > 0.01f;
        }
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).sqrMagnitude > 0.01f;
    }

    private void FindTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, EnemyOverlapBuffer);
        float dist = Mathf.Infinity;
        currentTarget = null;

        for (int i = 0; i < count; i++)
        {
            Collider col = EnemyOverlapBuffer[i];
            if (col.CompareTag("Enemy"))
            {
                // Mesafe hesabı için ağır olan Vector3.Distance yerine sqrMagnitude kullanılarak en yakın düşman bulunur.
                float d = (transform.position - col.transform.position).sqrMagnitude;
                if (d < dist)
                {
                    dist = d;
                    currentTarget = col.transform;
                }
            }
        }
    }

    public void TakeDamage(float amount)
    {
        // Alınan hasar miktarı mevcut candan düşülür, arayüz güncellenir ve can tükendiyse ölüm tetiklenir.
        currentHealth -= amount;
        _healthBar?.UpdateValue(currentHealth, maxHealth);
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        // Oyuncu öldüğünde üzerindeki can barı imha edilir ve GameManager üzerinden oyun bitiş ekranı açılır.
        if (_healthBar != null)
        {
            _healthBar.Destroy();
            _healthBar = null;
        }
        GameManager.Instance.GameOver();
    }

    public void FullyHeal() 
    { 
        // Oyuncunun canını tamamen doldurarak arayüzü günceller.
        currentHealth = maxHealth; 
        _healthBar?.UpdateValue(currentHealth, maxHealth); 
    }

    void OnDestroy()
    {
        // Nesne sahneden silinirken can barı belleğinin sızmaması için temizlik yapılır.
        if (_healthBar != null)
        {
            _healthBar.Destroy();
            _healthBar = null;
        }
    }
}