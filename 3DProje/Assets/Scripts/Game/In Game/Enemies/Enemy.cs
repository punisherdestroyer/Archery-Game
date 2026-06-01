using UnityEngine;
using System.Collections;
using Unity.UI;

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

    void Awake() => ActiveEnemyCount++;
    
    void OnDestroy()
    {
        if (!isDead)
        {
            ActiveEnemyCount = Mathf.Max(0, ActiveEnemyCount - 1);
        }
        _healthBar?.Destroy();
    }

    public void InitStats(float hpBuff, float atkBuff, float spdBuff, int levelBaseExp)
    {
        maxHp = 50f * (1f + hpBuff);
        hp = maxHp;
        atk = 50f * (1f + atkBuff);
        spd = 3f * (1f + spdBuff);
        expYield = levelBaseExp;
        
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        
        if (InGameUIController.Instance != null && InGameUIController.Instance.GetMainView() != null)
        {
            _healthBar = new HealthBarUI("packageInGame", "componentHPBar", transform, InGameUIController.Instance.GetMainView(), 1.2f);
        }
        _healthBar?.UpdateValue(hp, maxHp);
    }

    void Update()
    {
        if (player == null || GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused || isDead) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        Collider[] neighbors = Physics.OverlapSphere(transform.position, 1.2f);
        Vector3 separation = Vector3.zero;
        foreach (var col in neighbors)
        {
            if (col.gameObject != gameObject && col.CompareTag("Enemy"))
            {
                Vector3 away = transform.position - col.transform.position;
                away.y = 0;
                if (away.sqrMagnitude > 0f)
                    separation += away.normalized / away.magnitude;
            }
        }

        Vector3 moveDir = (direction + separation * 0.3f).normalized;

        if (controller.isGrounded) verticalVelocity = -0.5f;
        else verticalVelocity += gravity * Time.deltaTime;

        Vector3 move = moveDir * spd;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);

        if (moveDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), 10f * Time.deltaTime);
        }
    }

    void LateUpdate()
    {
        if (player != null && !GameManager.Instance.IsGameOver && !GameManager.Instance.IsPaused && !isDead)
        {
            _healthBar?.UpdatePosition();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isDead) return;

        if (hit.gameObject.CompareTag("Player"))
        {
            hit.gameObject.GetComponent<Player>().TakeDamage(atk * Time.deltaTime);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        hp -= amount;
        _healthBar?.UpdateValue(hp, maxHp);
        if (hp <= 0) Die();
    }

    public void StartBurn(float duration, float damagePerTick)
    {
        if (!isBurning && !isDead) StartCoroutine(BurnRoutine(duration, damagePerTick));
    }

    private IEnumerator BurnRoutine(float duration, float damagePerTick)
    {
        isBurning = true;
        while (duration > 0 && !isDead)
        {
            TakeDamage(damagePerTick);
            duration -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        isBurning = false;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

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