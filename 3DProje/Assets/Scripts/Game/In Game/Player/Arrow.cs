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

    // OverlapSphereNonAlloc için GC oluşmasını engelleyen sabit boyutlu dizi havuzu.
    private static readonly Collider[] NeighborBuffer = new Collider[32];

    public void Setup(Vector3 direction, bool burn, float burnDur, float burnDmg, int bounceCount, float dmg, float bounceLoss)
    {
        // Okun yönü, hasarı, yakma efektleri ve sekme özellikleri ilk değerlerine atanır.
        dir = direction;
        canBurn = burn;
        bDur = burnDur;
        bDmg = burnDmg;
        rCount = bounceCount;
        damage = dmg;
        rLoss = bounceLoss;
        
        // Okun haritada sonsuza kadar gitmemesi için 5 saniye sonra otomatik imha süresi başlatılır.
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        // Ok, belirlenen yön ve hız doğrultusunda her kare ileriye doğru hareket ettirilir.
        transform.position += dir * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Temas edilen nesne bir düşmansa ve bu ok o düşmana daha önce çarpmadıysa hasar aşamasına geçilir.
        if (other.CompareTag("Enemy") && !hits.Contains(other.transform))
        {
            if (other.TryGetComponent(out Enemy e))
            {
                // Düşmana doğrudan hasar verilir ve eğer yetenek aktifse yakma etkisi başlatılır.
                e.TakeDamage(damage);
                if (canBurn) e.StartBurn(bDur, bDmg);
            }
            
            // Aynı düşmana tekrar çarpmamak adına düşman transformu listeye kaydedilir.
            hits.Add(other.transform);

            // Eğer okun hala sekme hakkı varsa sekme sayısı ve hasarı azaltılarak yeni hedef aranır.
            if (rCount > 0)
            {
                rCount--;
                damage *= (1f - rLoss);
                FindNextTarget(other.transform);
            }
            else
            {
                // Sekme hakkı kalmadıysa ok yok edilir.
                Destroy(gameObject);
            }
        }
    }

    private void FindNextTarget(Transform currentHit)
    {
        // Çevre taraması yapılarak 15 birim yarıçapındaki potansiyel düşmanlar listelenir.
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, 15f, NeighborBuffer);
        float dist = Mathf.Infinity;
        Transform nextTarget = null;

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = NeighborBuffer[i];
            
            // Nesnenin düşman olup olmadığı, mevcut çarpılan nesne olmadığı ve eski hedefler arasında bulunmadığı kontrol edilir.
            if (col.CompareTag("Enemy") && col.transform != currentHit && !hits.Contains(col.transform))
            {
                // Karekök hesabı içeren Distance yerine sqrMagnitude kullanılarak en yakın düşman tespit edilir.
                float d = (transform.position - col.transform.position).sqrMagnitude;
                if (d < dist)
                {
                    dist = d;
                    nextTarget = col.transform;
                }
            }
        }

        // Eğer geçerli bir sonraki hedef bulunduysa okun yönü ve rotasyonu o düşmana doğru çevrilir.
        if (nextTarget != null)
        {
            dir = (nextTarget.position - transform.position).normalized;
            dir.y = 0;
            transform.rotation = Quaternion.LookRotation(dir);
        }
        else
        {
            // Sekilecek başka hiçbir düşman kalmadıysa ok imha edilir.
            Destroy(gameObject);
        }
    }
}