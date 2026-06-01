using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public static int ActiveEnemyCount = 0;

    [SerializeField] private float hp;
    [SerializeField] private float maxHp;
    [SerializeField] private float atk;
    [SerializeField] private float spd;
    [SerializeField] private int expYield;
    
    [SerializeField] private CharacterController controller;
    private HealthBarUI _healthBar;
    private Transform player;
    
    private bool isBurning;
    private float gravity = -9.81f;
    private float verticalVelocity;
    private bool isDead = false;

    // Sürekli yeni nesne üretilmesini engellemek için kullanılan performans önbellekleri.
    private static readonly WaitForSeconds BurnTickWait = new WaitForSeconds(0.1f);
    private static readonly Collider[] NeighborBuffer = new Collider[16];

    void Awake()
    {
        // Sahnedeki aktif düşman sayısını bir artırır.
        ActiveEnemyCount++;
    }
    
    void OnDestroy()
    {
        // Eğer düşman ölmeden sahneden silindiyse sayaç güvenli bir şekilde azaltılır.
        if (!isDead)
        {
            ActiveEnemyCount = Mathf.Max(0, ActiveEnemyCount - 1);
        }
        _healthBar?.Destroy();
    }

    public void InitStats(float hpBuff, float atkBuff, float spdBuff, int levelBaseExp)
    {
        // Gelen çarpanlara göre düşmanın temel nitelikleri hesaplanır.
        maxHp = 50f * (1f + hpBuff);
        hp = maxHp;
        atk = 50f * (1f + atkBuff);
        spd = 3f * (1f + spdBuff);
        expYield = levelBaseExp;
        
        // Oyuncu nesnesi etiket ile bulunup transform referansı saklanır.
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        
        // Arayüz yöneticisi aktifse düşmana özel bir can barı oluşturulur.
        if (InGameUIController.Instance != null && InGameUIController.Instance.GetMainView() != null)
        {
            _healthBar = new HealthBarUI("packageInGame", "componentHPBar", transform, InGameUIController.Instance.GetMainView(), 1.2f);
        }
        _healthBar?.UpdateValue(hp, maxHp);
    }

    void Update()
    {
        // Oyun bittiyse, duraklatıldıysa veya düşman öldüyse hiçbir işlem yapılmaz.
        if (player == null || GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused || isDead) return;

        // Oyuncuya doğru giden yatay düzlemdeki yön vektörü hesaplanır.
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        // Yakındaki diğer düşmanları tespit etmek için çevre taraması yapılır.
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, 1.2f, NeighborBuffer);
        Vector3 separation = Vector3.zero;

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = NeighborBuffer[i];
            // Temas edilen nesne kendisi değilse ve bir düşmansa aradaki mesafe hesaplanır.
            if (col.gameObject != gameObject && col.CompareTag("Enemy"))
            {
                Vector3 away = transform.position - col.transform.position;
                away.y = 0;
                
                float sqrMag = away.sqrMagnitude;
                if (sqrMag > 0f)
                {
                    // Yakındaki düşmanlardan uzaklaşmayı sağlayan itme kuvveti vektöre eklenir.
                    separation += away.normalized / Mathf.Sqrt(sqrMag);
                }
            }
        }

        // Oyuncuya gidiş yönü ile diğer düşmanlardan kaçış yönü harmanlanır.
        Vector3 moveDir = (direction + separation * 0.3f).normalized;

        // Düşmanın yere basma durumuna göre yerçekimi ivmesi uygulanır.
        if (controller.isGrounded) verticalVelocity = -0.5f;
        else verticalVelocity += gravity * Time.deltaTime;

        // Hesaplanan hız ve yön verileri nihai hareket vektörüne dönüştürülür.
        Vector3 move = moveDir * spd;
        move.y = verticalVelocity;

        // Karakter motoru kullanılarak hareket gerçekleştirilir.
        controller.Move(move * Time.deltaTime);

        // Düşman hareket ettiği yöne doğru yumuşak bir açıyla döndürülür.
        if (moveDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), 10f * Time.deltaTime);
        }
    }

    void LateUpdate()
    {
        // Fizik ve hareket işlemleri bittikten sonra can barının pozisyonu güncellenir.
        if (player != null && !GameManager.Instance.IsGameOver && !GameManager.Instance.IsPaused && !isDead)
        {
            _healthBar?.UpdatePosition();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isDead) return;

        // Eğer hareket esnasında oyuncuya temas edilirse oyuncuya zamana bağlı hasar verilir.
        if (hit.gameObject.CompareTag("Player"))
        {
            if (hit.gameObject.TryGetComponent(out Player playerComponent))
            {
                playerComponent.TakeDamage(atk * Time.deltaTime);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        // Alınan hasar mevcut candan düşülür, arayüz güncellenir ve sıfıra ulaştıysa ölüm tetiklenir.
        hp -= amount;
        _healthBar?.UpdateValue(hp, maxHp);
        if (hp <= 0) Die();
    }

    public void StartBurn(float duration, float damagePerTick)
    {
        // Düşman zaten yanmıyorsa ve hayattaysa zamanla hasar veren fonksiyon başlatılır.
        if (!isBurning && !isDead) StartCoroutine(BurnRoutine(duration, damagePerTick));
    }

    private IEnumerator BurnRoutine(float duration, float damagePerTick)
    {
        isBurning = true;
        // Belirlenen süre boyunca, her periyotta düşmana hasar verilmeye devam edilir.
        while (duration > 0 && !isDead)
        {
            TakeDamage(damagePerTick);
            duration -= 0.1f;
            yield return BurnTickWait;
        }
        isBurning = false;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Oyuncunun tecrübe puanı artırılır, can barı temizlenir ve düşman yok edilir.
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.AddExp(expYield);
        }

        if (_healthBar != null)
        {
            _healthBar.Destroy();
            _healthBar = null;
        }

        ActiveEnemyCount = Mathf.Max(0, ActiveEnemyCount - 1);

        Destroy(gameObject);
    }
}