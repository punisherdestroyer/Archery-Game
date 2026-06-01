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
        // Singleton yapısı kurulur.
        Instance = this;
    }

    void Start()
    {
        // Yetenek butonlarının altındaki cooldown görselleri aranır ve ilk ayarları yapılır.
        for (int i = 0; i < cooldownOverlays.Length; i++)
        {
            if (cooldownOverlays[i] != null)
            {
                // Hiyerarşideki isim baz alınarak ilgili alt nesne aranır.
                Transform t = cooldownOverlays[i].transform.Find("Cooldown Overlay");
                if (t != null)
                {
                    actualOverlayImages[i] = t.GetComponent<Image>();
                    if (actualOverlayImages[i] != null)
                    {
                        // Görsel tipinin dairesel dolum (Radial 360) olması garanti altına alınır.
                        actualOverlayImages[i].type = Image.Type.Filled;
                        actualOverlayImages[i].fillMethod = Image.FillMethod.Radial360;
                        actualOverlayImages[i].fillAmount = 0;
                    }
                }
            }
            // Bekleme süresi metinleri başlangıçta temizlenir.
            if (i < cooldownTexts.Length && cooldownTexts[i] != null) cooldownTexts[i].text = "";
        }
    }

    void Update()
    {
        // Toplam 5 yeteneğin bekleme süreleri her kare kontrol edilerek görsel ve metinsel arayüze yansıtılır.
        for (int i = 0; i < 5; i++)
        {
            float remain = nextReadyTimes[i] - Time.time;
            if (remain > 0 && lastFullCD[i] > 0)
            {
                // Yeteneğin kalan süresi devam ediyorsa dolum miktarı ve kalan saniye metni güncellenir.
                if (actualOverlayImages[i] != null) actualOverlayImages[i].fillAmount = remain / lastFullCD[i];
                if (i < cooldownTexts.Length && cooldownTexts[i] != null) cooldownTexts[i].text = Mathf.CeilToInt(remain).ToString();
            }
            else
            {
                // Bekleme süresi bittiyse arayüz elemanları sıfırlanır.
                if (actualOverlayImages[i] != null) actualOverlayImages[i].fillAmount = 0;
                if (i < cooldownTexts.Length && cooldownTexts[i] != null) cooldownTexts[i].text = "";
            }
        }
    }

    public float GetCooldownFill(int index)
    {
        // Dışarıdan sorgulanan yeteneğin anlık dolum oranını (0 ile 1 arasında) döndürür.
        float remain = nextReadyTimes[index] - Time.time;
        return (remain > 0 && lastFullCD[index] > 0) ? (remain / lastFullCD[index]) : 0;
    }

    public string GetCooldownText(int index)
    {
        // Dışarıdan sorgulanan yeteneğin kalan saniyesini yukarı yuvarlanmış metin formatında döndürür.
        float remain = nextReadyTimes[index] - Time.time;
        return remain > 0 ? Mathf.CeilToInt(remain).ToString() : "";
    }

    public void TryActivateAbility(int index)
    {
        /* Yeteneğin bekleme süresi dolmadıysa aktifleştirme işlemi engellenir.
        Ek güvenlik olarak InGameUIController.cs içinde button grayed ve untouchable yapılır */
        if (Time.time < nextReadyTimes[index]) return;
        
        // Seviye çarpanına göre yeteneğin nihai bekleme süresi hesaplanır ve süre başlatılır.
        float cd = GetBaseCD(index) * Mathf.Pow(0.95f, GetCDLvl(index));
        lastFullCD[index] = cd;
        nextReadyTimes[index] = Time.time + cd;
        
        // Yeteneğin aktif kalma süresi hesaplanarak zamanlayıcı asenkron döngüsü başlatılır.
        float dur = GetBaseDur(index) * (1f + (GetDurLvl(index) * 0.25f));
        StartCoroutine(AbilityTimer(index, dur));
    }

    private IEnumerator AbilityTimer(int index, float duration)
    {
        // Yetenek aktif bayrağı kaldırılır, süre boyunca beklenir ve ardından bayrak indirilir.
        SetFlag(index, true);
        yield return new WaitForSeconds(duration);
        SetFlag(index, false);
    }

    private void SetFlag(int index, bool state)
    {
        // Index değerine göre ilgili yeteneğin aktiflik durumu (bool) güncellenir.
        if (index == 0) isBurnActive = state;
        if (index == 1) isSpeedActive = state;
        if (index == 2) isRicoActive = state;
        if (index == 3) isMultiActive = state;
        if (index == 4) isRageActive = state;
    }

    public float GetAttackSpeedMultiplier()
    {
        // Oyuncunun speed yeteneği ve seviyesine bağlı saldırı hızı çarpanı hesaplanır.
        float baseMult = player.attackSpeedMultiplier + 0.6f + (speedMultLvl * 0.2f);
        return CalculateRageEffect(isSpeedActive, baseMult, 1.2f);
    }

    // Yakma hasar değerini hesaplar.
    public float GetBurnDamage() => CalculateRageEffect(isBurnActive, 1.5f + (burnDmgLvl * 1f), 0f);
    
    // Yakma etkisinin aktif süresini döndürür.
    public float GetBurnDuration() => (isBurnActive || isRageActive) ? 3f : 0f;
    
    // Okların sekme sayısını tam sayı olarak hesaplar.
    public int GetBounceCount() => Mathf.FloorToInt(CalculateRageEffect(isRicoActive, 1f + ricoCountLvl, 0f));
    
    // Okların her sekme başı kaybedeceği hasar oranını hesaplar.
    public float GetBounceLoss() => Mathf.Max(0f, 0.35f - (ricoCountLvl * 0.1f));
    
    // Tek seferde fırlatılacak ok sayısını hesaplar.
    public int GetArrowCount() => Mathf.FloorToInt(CalculateRageEffect(isMultiActive, 2f + multiShotCountLvl, 1f));
    
    // Rage yeteneğinin aktif olup olmadığını kontrol eder.
    public bool IsRageActive() => isRageActive;
    
    // Rage yeteneğinin getirdiği güçlendirme değerini hesaplar.
    public float GetRagePowerBuff() => 0.75f + (ragePowerLvl * 0.2f);
    
    private float CalculateRageEffect(bool active, float activeVal, float inactiveVal)
    {
        // Rage yeteneğinin diğer yetenekler üzerindeki etkisini hesaplar.
        if (active) return isRageActive ? activeVal + GetRagePowerBuff() : activeVal;
        if (isRageActive) return activeVal * (0.75f + (ragePowerLvl * 0.2f));
        return inactiveVal;
    }
    
    // Yeteneklerin bekleme sürelerini döndürür.
    private float GetBaseCD(int i) => i==0?30f:i==1?35f:i==2?40f:i==3?45f:60f;
    
    // Yeteneklerin aktif kalma sürelerini döndürür.
    private float GetBaseDur(int i) => i==4?5f:i==0?2f:3f;
    
    // Index değerine karşılık gelen yeteneğin bekleme süresi seviyesini döndürür.
    private int GetCDLvl(int i) => i==0?burnCDLvl:i==1?speedCDLvl:i==2?ricoCDLvl:i==3?multiShotCDLvl:rageCDLvl;
    
    // Index değerine karşılık gelen yeteneğin aktif kalma süresi seviyesini döndürür.
    private int GetDurLvl(int i) => i==0?burnDurLvl:i==1?speedDurLvl:i==2?ricoDurLvl:i==3?multiShotDurLvl:rageDurLvl;
}