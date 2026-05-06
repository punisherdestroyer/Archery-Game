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
    [SerializeField] private HealthBarController healthBar;
    private Transform player;
    
    private bool isBurning;
    private float gravity = -9.81f;
    private float verticalVelocity;

    void Awake() => ActiveEnemyCount++;
    void OnDestroy() => ActiveEnemyCount--;

    public void InitStats(float hpBuff, float atkBuff, float spdBuff, int levelBaseExp)
    {
        maxHp = 50f * (1f + hpBuff);
        hp = maxHp;
        atk = 50f * (1f + atkBuff);
        spd = 3f * (1f + spdBuff);
        expYield = levelBaseExp;
        
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        
        healthBar.UpdateHealth(hp, maxHp);
    }

    void Update()
    {
        if (player == null || GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        if (controller.isGrounded) verticalVelocity = -0.5f;
        else verticalVelocity += gravity * Time.deltaTime;

        Vector3 move = direction * spd;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Player"))
        {
            hit.gameObject.GetComponent<Player>().TakeDamage(atk * Time.deltaTime);
        }
    }

    public void TakeDamage(float amount)
    {
        hp -= amount;
        healthBar.UpdateHealth(hp, maxHp);
        if (hp <= 0) Die();
    }

    public void StartBurn(float duration, float damagePerTick)
    {
        if (!isBurning) StartCoroutine(BurnRoutine(duration, damagePerTick));
    }

    private IEnumerator BurnRoutine(float duration, float damagePerTick)
    {
        isBurning = true;
        while (duration > 0)
        {
            TakeDamage(damagePerTick);
            duration -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        isBurning = false;
    }

    private void Die()
    {
        LevelManager.Instance.AddExp(expYield);
        Destroy(gameObject);
    }
}