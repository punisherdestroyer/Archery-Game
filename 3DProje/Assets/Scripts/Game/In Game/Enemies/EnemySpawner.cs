using UnityEngine;
using TMPro;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject elitePrefab;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text buffNotificationText;

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

    void Start()
    {
        buffNotificationText.text = "";
        StartCoroutine(TimerTick());
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator TimerTick()
    {
        while (!GameManager.Instance.IsGameOver)
        {
            yield return new WaitForSeconds(1f);
            if (GameManager.Instance.IsPaused) continue;

            elapsedTime++;
            timerText.text = FormatTime(elapsedTime);

            if (elapsedTime > 0 && elapsedTime % 60 == 0)
            {
                ApplyRandomEnemyBuff();
            }

            eliteSpawnChance = Mathf.Min(25f, 5f + (elapsedTime / 60f));
            maxEnemiesAllowed = 50 + (elapsedTime / 10);
            currentSpawnInterval = Mathf.Max(minSpawnInterval, 2f - (elapsedTime * 0.005f));
        }
    }

    private string FormatTime(int seconds)
    {
        int m = seconds / 60;
        int s = seconds % 60;
        return string.Format("{0:00}:{1:00}", m, s);
    }

    private void ApplyRandomEnemyBuff()
    {
        int rand = Random.Range(0, 3);
        string msg = "";

        if (rand == 0) { enemyHpBuff += 0.25f; msg = "Enemies' Health Increased!"; }
        else if (rand == 1) { enemyAtkBuff += 0.25f; msg = "Enemies' Damage Increased!"; }
        else { enemySpdBuff += 0.25f; msg = "Enemies' Speed Increased!"; }

        StartCoroutine(ShowBuffText(msg));
    }

    IEnumerator ShowBuffText(string message)
    {
        buffNotificationText.text = message;
        yield return new WaitForSeconds(4f);
        buffNotificationText.text = "";
    }

    IEnumerator SpawnRoutine()
    {
        while (!GameManager.Instance.IsGameOver)
        {
            if (!GameManager.Instance.IsPaused && Enemy.ActiveEnemyCount < maxEnemiesAllowed)
            {
                SpawnEnemyNearPlayer();
            }
            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    void SpawnEnemyNearPlayer()
    {
        if (playerTransform == null) return;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
        Vector3 spawnPos = playerTransform.position + new Vector3(randomDir.x, 0, randomDir.y) * distance;
        
        spawnPos.x = Mathf.Clamp(spawnPos.x, minX, maxX);
        spawnPos.z = Mathf.Clamp(spawnPos.z, minZ, maxZ);
        spawnPos.y = 0.5f;

        GameObject prefabToSpawn = enemyPrefab;
        float hpMult = 1f;
        float atkMult = 1f;
        int expMult = 1;

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

        GameObject spawned = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        int baseExp = 10 + Mathf.FloorToInt(LevelManager.Instance.currentLevel * 1.5f);
        
        spawned.GetComponent<Enemy>().InitStats(
            enemyHpBuff + (hpMult - 1f), 
            enemyAtkBuff + (atkMult - 1f), 
            enemySpdBuff, 
            baseExp * expMult
        );
    }
}