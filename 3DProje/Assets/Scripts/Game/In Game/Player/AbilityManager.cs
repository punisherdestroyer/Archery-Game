using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class AbilityManager : MonoBehaviour
{
    Player player;
    public int burnCDLvl, burnDurLvl, burnDmgLvl;
    public int speedCDLvl, speedDurLvl, speedMultLvl;
    public int ricoCDLvl, ricoDurLvl, ricoCountLvl;
    public int multiShotCDLvl, multiShotDurLvl, multiShotCountLvl;
    public int rageCDLvl, rageDurLvl, ragePowerLvl;
    
    private float[] lastFullCD = new float[5];
    private bool isBurnActive, isSpeedActive, isRicoActive, isMultiActive, isRageActive;
    private float[] nextReadyTimes = new float[5];
    
    [SerializeField] private Image[] cooldownOverlays;
    [SerializeField] private TMP_Text[] cooldownTexts;

    private Image[] actualOverlayImages = new Image[5];
    public static AbilityManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        for (int i = 0; i < cooldownOverlays.Length; i++)
        {
            if (cooldownOverlays[i] != null)
            {
                Transform t = cooldownOverlays[i].transform.Find("Cooldown Overlay");
                if (t != null)
                {
                    actualOverlayImages[i] = t.GetComponent<Image>();
                    if (actualOverlayImages[i] != null)
                    {
                        actualOverlayImages[i].type = Image.Type.Filled;
                        actualOverlayImages[i].fillMethod = Image.FillMethod.Radial360;
                        actualOverlayImages[i].fillAmount = 0;
                    }
                }
            }
            if (i < cooldownTexts.Length && cooldownTexts[i] != null)
            {
                cooldownTexts[i].text = "";
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < 5; i++)
        {
            float remain = nextReadyTimes[i] - Time.time;

            if (remain > 0 && lastFullCD[i] > 0)
            {
                if (actualOverlayImages[i] != null) 
                    actualOverlayImages[i].fillAmount = remain / lastFullCD[i];
                
                if (i < cooldownTexts.Length && cooldownTexts[i] != null) 
                    cooldownTexts[i].text = Mathf.CeilToInt(remain).ToString();
            }
            else
            {
                if (actualOverlayImages[i] != null) actualOverlayImages[i].fillAmount = 0;
                if (i < cooldownTexts.Length && cooldownTexts[i] != null) 
                    cooldownTexts[i].text = "";
            }
        }
    }

    public void TryActivateAbility(int index)
    {
        if (Time.time < nextReadyTimes[index]) return;

        float cd = GetBaseCD(index) * Mathf.Pow(0.95f, GetCDLvl(index));
        lastFullCD[index] = cd;
        nextReadyTimes[index] = Time.time + cd;

        float dur = GetBaseDur(index) * (1f + (GetDurLvl(index) * 0.2f));
        StartCoroutine(AbilityTimer(index, dur));
    }

    private IEnumerator AbilityTimer(int index, float duration)
    {
        SetFlag(index, true);
        yield return new WaitForSeconds(duration);
        SetFlag(index, false);
    }

    private void SetFlag(int index, bool state)
    {
        if (index == 0) isBurnActive = state;
        if (index == 1) isSpeedActive = state;
        if (index == 2) isRicoActive = state;
        if (index == 3) isMultiActive = state;
        if (index == 4) isRageActive = state;
    }

    public float GetAttackSpeedMultiplier()
    {
        float baseMult = player.attackSpeedMultiplier + 0.5f + (speedMultLvl * 0.15f);
        return CalculateRageEffect(isSpeedActive, baseMult, 1.0f);
    }

    public float GetBurnDamage() => CalculateRageEffect(isBurnActive, 1f + (burnDmgLvl * 0.25f), 0f);
    public float GetBurnDuration() => (isBurnActive || isRageActive) ? 2f : 0f;
    public int GetBounceCount() 
    {
        if (!isRicoActive && !isRageActive) return 0; 
        return Mathf.FloorToInt(CalculateRageEffect(isRicoActive, 1f + ricoCountLvl, 0f));
    }
    
    public float GetBounceLoss() => Mathf.Max(0f, 0.25f - (ricoCountLvl * 0.01f));

    public int GetArrowCount() 
    {
        if (!isMultiActive && !isRageActive) return 1;
        return Mathf.FloorToInt(CalculateRageEffect(isMultiActive, 2f + multiShotCountLvl, 1f));
    }

    public bool IsRageActive() => isRageActive;
    public float GetRagePowerBuff() => 0.5f + (ragePowerLvl * 0.15f);

    private float CalculateRageEffect(bool active, float activeVal, float inactiveVal)
    {
        if (active) return isRageActive ? activeVal + GetRagePowerBuff() : activeVal;
        if (isRageActive) return activeVal * (0.75f + (ragePowerLvl * 0.01f));
        return inactiveVal;
    }

    private float GetBaseCD(int i) => i==0?30f:i==1?35f:i==2?40f:i==3?45f:60f;
    private float GetBaseDur(int i) => i==4?5f:i==0?2f:3f;
    private int GetCDLvl(int i) => i==0?burnCDLvl:i==1?speedCDLvl:i==2?ricoCDLvl:i==3?multiShotCDLvl:rageCDLvl;
    private int GetDurLvl(int i) => i==0?burnDurLvl:i==1?speedDurLvl:i==2?ricoDurLvl:i==3?multiShotDurLvl:rageDurLvl;
}