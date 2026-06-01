using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class UpgradeOption
{
    public string upgradeID;
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    [SerializeField] private Player player;
    [SerializeField] private AbilityManager abM;

    private static readonly string[] StatIDs    = { "HP", "ATK", "ASPD", "SPD" };
    private static readonly string[] AbilityIDs = { "B_DMG", "B_CD", "B_DUR", "S_PWR", "S_CD", "S_DUR", "R_PWR", "R_CD", "R_DUR", "M_PWR", "M_CD", "M_DUR", "RG_PWR", "RG_CD", "RG_DUR" };

    private float curExp;
    private float reqExp = 100f;
    public int currentLevel = 1;

    void Awake() => Instance = this;

    public void AddExp(float amt)
    {
        // Kazanılan tecrübe puanı mevcut puana eklenir ve arayüzdeki bar güncellenir.
        curExp += amt;
        InGameUIController.Instance?.UpdateExpBar(curExp, reqExp);
        
        // Eğer mevcut tecrübe puanı gereken miktara ulaştıysa seviye atlama süreci tetiklenir.
        if (curExp >= reqExp) LevelUp();
    }

    private void LevelUp()
    {
        // Seviye atlandığında mevcut tecrübe sıfırlanır, gereken hedef puan artırılır ve oyuncu tamamen iyileştirilir.
        curExp = 0f;
        reqExp *= 1.1f;
        currentLevel++;
        player.FullyHeal();
        
        // Rastgele seçilen geliştirme seçenekleri arayüz ekranına gönderilerek oyuncuya sunulur.
        InGameUIController.Instance?.ShowUpgradeScreen(GetRandomUpgrades());
    }

    private List<string> GetRandomUpgrades()
    {
        List<string> selected = new List<string>();
        HashSet<string> used = new HashSet<string>();

        // İlk aşamada temel istatistiklerden (StatIDs) kesinlikle bir adet rastgele seçim yapılır.
        List<string> stats = new List<string>(StatIDs);
        int si = Random.Range(0, stats.Count);
        selected.Add(stats[si]);
        used.Add(stats[si]);

        // Geriye kalan tüm istatistik ve yetenek ID'leri, tekrar seçimi önlemek adına tek bir havuzda birleştirilir.
        List<string> combined = new List<string>();
        foreach (string id in StatIDs)    if (!used.Contains(id)) combined.Add(id);
        foreach (string id in AbilityIDs) if (!used.Contains(id)) combined.Add(id);

        // Havuzdan rastgele 2 adet benzersiz geliştirme seçeneği daha çekilerek toplam liste 3'e tamamlanır.
        for (int i = 0; i < 2 && combined.Count > 0; i++)
        {
            int r = Random.Range(0, combined.Count);
            selected.Add(combined[r]);
            used.Add(combined[r]);
            combined.RemoveAt(r);
        }

        return selected;
    }

    public void ExecuteUpgradeByID(string id)
    {
        // Seçilen geliştirme ID'sine göre oyuncunun ilgili nitelik seviyesi veya yetenek çarpanları kalıcı olarak artırılır.
        switch (id)
        {
            case "HP": player.hpLevel++; player.maxHealth += 25f; break;
            case "ATK": player.atkLevel++; player.atkDamage += 75f; break;
            case "ASPD": player.atkSpeedLevel++; player.attackSpeedMultiplier += 0.2f; break;
            case "SPD": player.moveSpeedLevel++; player.moveSpeed += 2f; break;
            case "B_CD": abM.burnCDLvl++; break;
            case "B_DUR": abM.burnDurLvl++; break;
            case "B_PWR": abM.burnDmgLvl++; break;
            case "S_CD": abM.speedCDLvl++; break;
            case "S_DUR": abM.speedDurLvl++; break;
            case "S_PWR": abM.speedMultLvl++; break;
            case "R_CD": abM.ricoCDLvl++; break;
            case "R_DUR": abM.ricoDurLvl++; break;
            case "R_PWR": abM.ricoCountLvl++; break;
            case "M_CD": abM.multiShotCDLvl++; break;
            case "M_DUR": abM.multiShotDurLvl++; break;
            case "M_PWR": abM.multiShotCountLvl++; break;
            case "RG_CD":  abM.rageCDLvl++; break;
            case "RG_DUR": abM.rageDurLvl++; break;
            case "RG_PWR": abM.ragePowerLvl++; break;
        }
        
        // Geliştirme sonrasında oyuncunun genel durum kontrolleri yenilenir ve sağlığı doldurulur.
        player.LevelCheck();
        player.FullyHeal();
    }
}