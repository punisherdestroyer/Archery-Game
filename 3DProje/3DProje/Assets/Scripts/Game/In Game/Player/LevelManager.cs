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
        if (expFill != null) expFill.fillAmount = curExp / reqExp;
        if (InGameUIController.Instance != null) InGameUIController.Instance.UpdateExpBar(curExp, reqExp);
    }

    void LevelUp()
    {
        curExp = 0;
        reqExp *= 1.1f;
        currentLevel++;
        player.FullyHeal();
        Time.timeScale = 0;
        
        if (InGameUIController.Instance != null)
        { 
            InGameUIController.Instance._mainView.GetController("ManagerI").selectedIndex = 3;
        }
        
        if (panel != null) panel.SetActive(false);
        SetupButtons();
    }

    void SetupButtons()
    {
        List<UpgradeOption> sPool = new List<UpgradeOption>(statPool);
        List<UpgradeOption> aPool = new List<UpgradeOption>(abilityPool);
        List<UpgradeOption> selected = new List<UpgradeOption>();

        if (sPool.Count > 0)
        {
            int r = Random.Range(0, sPool.Count);
            selected.Add(sPool[r]);
            sPool.RemoveAt(r);
        }

        List<UpgradeOption> combined = new List<UpgradeOption>();
        combined.AddRange(sPool);
        combined.AddRange(aPool);

        if (combined.Count > 0)
        {
            int r = Random.Range(0, combined.Count);
            selected.Add(combined[r]);
            if (aPool.Contains(combined[r])) aPool.Remove(combined[r]);
        }

        if (aPool.Count > 0)
        {
            int r = Random.Range(0, aPool.Count);
            selected.Add(aPool[r]);
        }

        if (InGameUIController.Instance != null)
        {
            InGameUIController.Instance.ShowUpgradeScreen(selected, (id) => {
                ExecuteUpgradeByID(id);
                InGameUIController.Instance.ShowScreen(0);
                Time.timeScale = 1;
            });
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
            player.atkDamage += 75f;
        }
        else if (id == "ASPD") 
        {
            player.atkSpeedLevel++;
            player.attackSpeedMultiplier += 0.2f;
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
    }
}