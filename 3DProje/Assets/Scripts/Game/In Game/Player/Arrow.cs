using UnityEngine;
using System.Collections.Generic;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage;
    [SerializeField] private Vector3 dir;
    [SerializeField] private bool canBurn;
    [SerializeField] private float bDur = 2f;
    [SerializeField] private float bDmg = 1.5f;
    
    [SerializeField] private float rLoss;
    [SerializeField] private int rCount;
    [SerializeField] private List<Transform> hits = new List<Transform>();

    public void Setup(Vector3 direction, bool burn, float burnDur, float burnDmg, int bounceCount, float dmg, float bounceLoss)
    {
        dir = direction;
        canBurn = burn;
        bDur = burnDur;
        bDmg = burnDmg;
        rCount = bounceCount;
        damage = dmg;
        rLoss = bounceLoss;
        
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.position += dir * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && !hits.Contains(other.transform))
        {
            Enemy e = other.GetComponent<Enemy>();
            if (e != null)
            {
                e.TakeDamage(damage);
                if (canBurn) e.StartBurn(bDur, bDmg);
            }
            
            hits.Add(other.transform);

            if (rCount > 0)
            {
                rCount--;
                damage *= (1f - rLoss);
                FindNextTarget(other.transform);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void FindNextTarget(Transform currentHit)
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, 15f);
        float dist = Mathf.Infinity;
        Transform nextTarget = null;

        foreach (var col in cols)
        {
            if (col.CompareTag("Enemy") && col.transform != currentHit && !hits.Contains(col.transform))
            {
                float d = Vector3.Distance(transform.position, col.transform.position);
                if (d < dist)
                {
                    dist = d;
                    nextTarget = col.transform;
                }
            }
        }

        if (nextTarget != null)
        {
            dir = (nextTarget.position - transform.position).normalized;
            dir.y = 0;
            transform.rotation = Quaternion.LookRotation(dir);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}