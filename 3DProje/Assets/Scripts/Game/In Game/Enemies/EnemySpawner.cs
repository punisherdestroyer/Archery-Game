using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject elitePrefab;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform playerTransform;

    [Header("Spawn")]
    [SerializeField] private float minSpawnDistance = 15f;
    [SerializeField] private float maxSpawnDistance = 25f;
    [SerializeField] private float minSpawnInterval = 0.3f;
    [SerializeField] private float eliteSpawnChance = 5f;

    [Header("Borders")]
    [SerializeField] private float minX = -45f;
    [SerializeField] private float maxX = 45f;
    [SerializeField] private float minZ = -45f;
    [SerializeField] private float maxZ = 45f;

    private float currentSpawnInterval = 2f;
    private int maxEnemiesAllowed = 100;
    public int elapsedTime = 0;
    
    private float enemyHpBuff = 0f;
    private float enemyAtkBuff = 0f;
    private float enemySpdBuff = 0f;

    // Her döngüde yeni nesne üretilmesini engelleyerek hafıza yönetimini optimize eden önbelleklenmiş saniye nesnesi.
    private static readonly WaitForSeconds OneSecondWait = new WaitForSeconds(1f);

    void Start()
    {
        // Zamanlayıcı ve düşman oluşturma döngüleri bağımsız olarak başlatılır.
        StartCoroutine(TimerTick());
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator TimerTick()
    {
        while (!GameManager.Instance.IsGameOver)
        {
            yield return OneSecondWait;
            if (GameManager.Instance.IsPaused) continue;

            // Hayatta kalınan toplam süre saniye bazında artırılır.
            elapsedTime++;

            // Her 45 saniyede bir düşmanlara rastgele bir güçlendirme uygulanır.
            if (elapsedTime > 0 && elapsedTime % 45 == 0)
            {
                ApplyRandomEnemyBuff();
            }

            // Süre ilerledikçe oyun zorluğu, düşman limitleri ve doğma sıklığı dinamik olarak güncellenir.
            eliteSpawnChance = Mathf.Min(25f, 5f + (elapsedTime / 60f));
            maxEnemiesAllowed = 50 + (elapsedTime / 10);
            currentSpawnInterval = Mathf.Max(minSpawnInterval, 2f - (elapsedTime * 0.005f));
        }
    }

    private void ApplyRandomEnemyBuff()
    {
        // Can, hasar veya hız özelliklerinden biri rastgele seçilerek kalıcı olarak artırılır.
        int rand = Random.Range(0, 3);
        string msg = "";

        if (rand == 0) { enemyHpBuff += 0.25f; msg = "ENEMIES' HEALTH INCREASED!"; }
        else if (rand == 1) { enemyAtkBuff += 0.25f; msg = "ENEMIES' DAMAGE INCREASED!"; }
        else { enemySpdBuff += 0.25f; msg = "ENEMIES' SPEED INCREASED!"; }

        StartCoroutine(ShowBuffText(msg));
    }

    IEnumerator ShowBuffText(string message)
    {
        // Ekrana gelen güçlendirme bildirimi belirli bir süre gösterildikten sonra kaldırılır.
        InGameUIController.Instance?.ShowNotification(message);
        yield return new WaitForSeconds(4f);
    }

    IEnumerator SpawnRoutine()
    {
        while (!GameManager.Instance.IsGameOver)
        {
            // Oyun duraklatılmadıysa ve haritadaki düşman sayısı limiti aşmadıysa yeni düşman üretilir.
            if (!GameManager.Instance.IsPaused && Enemy.ActiveEnemyCount < maxEnemiesAllowed)
            {
                SpawnEnemyNearPlayer();
            }
            // Değişen doğma aralığı süresine göre dinamik olarak beklenir.
            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    void SpawnEnemyNearPlayer()
    {
        if (playerTransform == null) return;

        // Oyuncunun etrafında rastgele bir yönde ve belirlenen mesafe aralığında bir doğum pozisyonu hesaplanır.
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
        Vector3 spawnPos = playerTransform.position + new Vector3(randomDir.x, 0, randomDir.y) * distance;
        
        // Hesaplanan pozisyonun oyun haritası sınırlarının dışına çıkması engellenir.
        spawnPos.x = Mathf.Clamp(spawnPos.x, minX, maxX);
        spawnPos.z = Mathf.Clamp(spawnPos.z, minZ, maxZ);
        spawnPos.y = 0.5f;

        GameObject prefabToSpawn = enemyPrefab;
        float hpMult = 1f;
        float atkMult = 1f;
        int expMult = 1;

        // Her 5 dakikada bir Boss, aksi durumlarda ise şansa bağlı olarak Elite düşman seçilir.
        if (elapsedTime > 0 && elapsedTime % 300 == 0)
        {
            prefabToSpawn = bossPrefab;
            hpMult = 10f;
            atkMult = 3f;
            expMult = 50;
        }
        else if (Random.Range(0f, 100f) < eliteSpawnChance)
        {
            prefabToSpawn = elitePrefab;
            hpMult = 3f;
            atkMult = 1.5f;
            expMult = 5;
        }
        
        // Seçilen düşman prefabı belirlenen konumda dünyaya getirilir.
        GameObject spawned = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        int baseExp = 10 + Mathf.FloorToInt(LevelManager.Instance.currentLevel * 1.5f);
        
        // Dünyaya gelen düşmanın nitelikleri ve vereceği tecrübe puanı ilk değerlerine atanır.
        if (spawned.TryGetComponent(out Enemy enemyComponent))
        {
            enemyComponent.InitStats(
                enemyHpBuff + (hpMult - 1f), 
                enemyAtkBuff + (atkMult - 1f), 
                enemySpdBuff, 
                baseExp * expMult
            );
        }
    }
}