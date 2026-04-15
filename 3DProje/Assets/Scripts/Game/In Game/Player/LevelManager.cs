using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class UpgradeOption
{
    public GameObject prefab;
    public string upgradeID;
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    [SerializeField] private Player player;
    [SerializeField] private AbilityManager abM;
    [SerializeField] private GameObject panel;
    [SerializeField] private Image expFill;
    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;
    [SerializeField] private Transform spawnPoint3;
    
    [SerializeField] private List<UpgradeOption> statPool; 
    [SerializeField] private List<UpgradeOption> abilityPool;
    
    private float curExp, reqExp = 100;
    public int currentLevel = 1;

    void Awake() => Instance = this;

    public void AddExp(float amt)
    {
        curExp += amt;
        if (curExp >= reqExp) LevelUp();
        expFill.fillAmount = curExp / reqExp;
    }

    void LevelUp()
    {
        curExp = 0;
        reqExp *= 1.2f;
        currentLevel++;
        player.FullyHeal();
        Time.timeScale=0;
        panel.SetActive(true);
        SetupButtons();
    }

    void SetupButtons()
    {
        foreach (Transform child in spawnPoint1) Destroy(child.gameObject);
        foreach (Transform child in spawnPoint2) Destroy(child.gameObject);
        foreach (Transform child in spawnPoint3) Destroy(child.gameObject);

        List<UpgradeOption> currentStats = new List<UpgradeOption>(statPool);
        List<UpgradeOption> currentAbilities = new List<UpgradeOption>(abilityPool);
        List<UpgradeOption> selectedOptions = new List<UpgradeOption>();

        if (currentStats.Count > 0)
        {
            int r = Random.Range(0, currentStats.Count);
            selectedOptions.Add(currentStats[r]);
            currentStats.RemoveAt(r);
        }

        if (currentAbilities.Count > 0)
        {
            int r = Random.Range(0, currentAbilities.Count);
            selectedOptions.Add(currentAbilities[r]);
            currentAbilities.RemoveAt(r);
        }

        List<UpgradeOption> remainingPool = new List<UpgradeOption>();
        remainingPool.AddRange(currentStats);
        remainingPool.AddRange(currentAbilities);

        if (remainingPool.Count > 0)
        {
            int r = Random.Range(0, remainingPool.Count);
            selectedOptions.Add(remainingPool[r]);
        }

        Transform[] points = { spawnPoint1, spawnPoint2, spawnPoint3 };

        for (int i = 0; i < selectedOptions.Count; i++)
        {
            GameObject btnObj = Instantiate(selectedOptions[i].prefab, points[i]);
            Button btn = btnObj.GetComponent<Button>();
            string id = selectedOptions[i].upgradeID;
            btn.onClick.AddListener(() => { ExecuteUpgradeByID(id); panel.SetActive(false);});
        }
    }

    public void ExecuteUpgradeByID(string id)
    {
        if (id == "HP") 
        {
            player.hpLevel++;
            player.maxHealth += 25f;
        }
        else if (id == "ATK") 
        {
            player.atkLevel++;
            player.atkDamage += 5f;
        }
        else if (id == "ASPD") 
        {
            player.atkSpeedLevel++;
            player.attackSpeedMultiplier += 0.05f;
        }
        else if (id == "SPD") 
        {
            player.moveSpeedLevel++;
            player.moveSpeed += 2f;
        }
        else if (id == "B_CD") abM.burnCDLvl++;
        else if (id == "B_DUR") abM.burnDurLvl++;
        else if (id == "B_DMG") abM.burnDmgLvl++;
        else if (id == "S_CD") abM.speedCDLvl++;
        else if (id == "S_DUR") abM.speedDurLvl++;
        else if (id == "S_PWR") abM.speedMultLvl++;
        else if (id == "R_CD") abM.ricoCDLvl++;
        else if (id == "R_DUR") abM.ricoDurLvl++;
        else if (id == "R_PWR") abM.ricoCountLvl++;
        else if (id == "M_CD") abM.multiShotCDLvl++;
        else if (id == "M_DUR") abM.multiShotDurLvl++;
        else if (id == "M_PWR") abM.multiShotCountLvl++;
        else if (id == "RG_CD") abM.rageCDLvl++;
        else if (id == "RG_DUR") abM.rageDurLvl++;
        else if (id == "RG_PWR") abM.ragePowerLvl++;
        
        player.LevelCheck();
        player.FullyHeal();
        Time.timeScale=1;
    }
}