using FairyGUI;
using UnityEngine;
using System.Collections.Generic;

public class InGameUIController : MonoBehaviour
{
    public static InGameUIController Instance { get; private set; }

    private GComponent _mainView;
    private GComponent _pauseView;
    private GComponent _gameOverView;
    private GComponent _upgradeView;

    private GTextField _scoreText;
    private GTextField _bestScoreText;
    private GTextField _gameScoreText;
    private GTextField _gameBestScoreText;
    private GProgressBar _expBar;
    private GTextField _notificationText;
    private GButton[] _abilityButtons = new GButton[5];
    private GButton _dashButton;
    
    private GObject _joystickTouch;
    private GObject _joystickCenter;
    private GObject _joystickThumb;
    private GObject _joystickOutline;
    private Vector2 _joystickStartPos;
    private Vector2 _joystickInput;
    private int _joystickTouchId = -1; 

    private Player _player;

    private GButton _upgradeButton1;
    private GButton _upgradeButton2;
    private GButton _upgradeButton3;
    
    public GComponent GetMainView() => _mainView;

    void Awake()
    {
        // Eski singleton örneği varsa arayüz elementleri temizlenip nesne imha edilir.
        if (Instance != null)
        {
            Instance.CleanUp();
            Destroy(Instance.gameObject);
        }
        Instance = this;
        
        // FairyGUI için global içerik ölçekleme katsayıları ve ekran eşitleme modu ayarlanır.
        GRoot.inst.SetContentScaleFactor(1920, 1080, UIContentScaler.ScreenMatchMode.MatchWidthOrHeight);

        LoadAndCreateUI();
    }

    private void LoadAndCreateUI()
    {
        // Eğer paketler daha önce yüklenmediyse FairyGUI kaynak dizininden projeye dahil edilir.
        if (UIPackage.GetByName("packageInGame") == null)
        {
            UIPackage.AddPackage("FairyUI/packageInGame");
            UIPackage.AddPackage("FairyUI/packageShared");
            UIPackage.AddPackage("FairyUI/packageMenu");
        }

        // Oyun içi, duraklatma, oyun bitiş ve geliştirme panelleri oluşturulur.
        _mainView     = UIPackage.CreateObject("packageInGame", "componentInGame").asCom;
        _pauseView    = UIPackage.CreateObject("packageInGame", "componentPause").asCom;
        _gameOverView = UIPackage.CreateObject("packageInGame", "componentGameOver").asCom;
        _upgradeView  = UIPackage.CreateObject("packageInGame", "componentUpgrade").asCom;

        if (_mainView == null) return;

        // Ana görünüm tam ekran yapılır ve FairyGUI kök dizinine eklenir.
        _mainView.MakeFullScreen();
        GRoot.inst.AddChild(_mainView);

        // Katmanların tam ekran yapılıp görünmez olarak kök dizine eklenmesini sağlayan yerel metot.
        void AddOverlay(GComponent v)
        {
            if (v == null) return;
            v.MakeFullScreen();
            v.visible   = false;
            v.touchable = true;
            GRoot.inst.AddChild(v);
        }
        AddOverlay(_pauseView);
        AddOverlay(_gameOverView);
        AddOverlay(_upgradeView);

        // Ana arayüz üzerindeki skor metin alanları hiyerarşik olarak taranıp önbelleğe alınır.
        _scoreText     = FindDeep(_mainView, "textScore")?.asTextField;
        _bestScoreText = FindDeep(_mainView, "textBestScore")?.asTextField;

        if (_gameOverView != null)
        {
            _gameBestScoreText = FindDeep(_gameOverView, "textBestScore")?.asTextField;
            _gameScoreText     = FindDeep(_gameOverView, "textScore")?.asTextField;
        }

        // Tecrübe barı ve bildirim metni referansları alınır.
        _expBar           = FindDeep(_mainView, "barLevel")?.asProgress;
        _notificationText = FindDeep(_mainView, "textNotification")?.asTextField;
        if (_notificationText != null) _notificationText.visible = false;

        // Seviye atlama ekranındaki 3 farklı seçim butonu bulunarak başlangıçta gizlenir.
        if (_upgradeView != null)
        {
            _upgradeButton1 = FindDeep(_upgradeView, "buttonUpgrade1")?.asButton;
            _upgradeButton2 = FindDeep(_upgradeView, "buttonUpgrade2")?.asButton;
            _upgradeButton3 = FindDeep(_upgradeView, "buttonUpgrade3")?.asButton;

            SetButtonVisible(_upgradeButton1, false);
            SetButtonVisible(_upgradeButton2, false);
            SetButtonVisible(_upgradeButton3, false);
        }
    }

    void Start()
    {
        // Sahnedeki oyuncu referansı önbelleğe alınır.
        _player = Object.FindFirstObjectByType<Player>();

        if (_mainView == null) return;

        // En yüksek skor verisi alınarak ilgili metin alanlarına yazdırılır.
        if (GameManager.Instance != null)
        {
            string bestTimeStr = "BEST: " + GameManager.Instance.GetBestTimeText();
            if (_bestScoreText != null) _bestScoreText.text = bestTimeStr;
            if (_gameBestScoreText != null) _gameBestScoreText.text = bestTimeStr;
        }

        // Geliştirme ekranındaki geç butonunun tıklama olayları ve panel kapatma mantığı bağlanır.
        if (_upgradeView != null)
        {
            GButton skipBtn = FindDeep(_upgradeView, "buttonSkip")?.asButton;
            if (skipBtn != null)
            {
                skipBtn.onClick.Add(() =>
                {
                    _player?.LevelCheck();
                    _player?.FullyHeal();
                    HideUpgradeScreen();
                    GameManager.Instance?.ResumeGame();
                });
            }
        }

        // Duraklatma ekranı butonlarının tıklama olayları bağlanır.
        if (_pauseView != null)
        {
            FindDeep(_pauseView, "buttonResume")?.asButton?.onClick.Add(() => GameManager.Instance?.ResumeGame());
            FindDeep(_pauseView, "buttonMenu")?.asButton?.onClick.Add(() => GameManager.Instance?.GoToMainMenu());
            FindDeep(_pauseView, "buttonRestart")?.asButton?.onClick.Add(() => GameManager.Instance?.RestartGame());
        }

        // Oyun bitti ekranı butonlarının tıklama olayları bağlanır.
        if (_gameOverView != null)
        {
            FindDeep(_gameOverView, "buttonRestart")?.asButton?.onClick.Add(() => GameManager.Instance?.RestartGame());
            FindDeep(_gameOverView, "buttonMenu")?.asButton?.onClick.Add(() => GameManager.Instance?.GoToMainMenu());
        }

        // Oyun içi duraklatma butonunun tetikleyicisi bağlanır
        FindDeep(_mainView, "buttonPause")?.asButton?.onClick.Add(() => GameManager.Instance?.PauseGame());

        InitJoystick();
        InitAbilityButtons();
    }

    private void CleanUp()
    {
        // Olay dinleyicileri bellek sızıntısını önlemek amacıyla sökülür ve paneller bellekten temizlenir.
        if (_joystickTouch != null)
        {
            _joystickTouch.onTouchBegin.Remove(OnJoystickTouchBegin);
            _joystickTouch.onTouchMove.Remove(OnJoystickTouchMove);
            _joystickTouch.onTouchEnd.Remove(OnJoystickTouchEnd);
        }

        void Dispose(GComponent c) { if (c != null) { c.RemoveFromParent(); c.Dispose(); } }
        Dispose(_mainView);     _mainView     = null;
        Dispose(_pauseView);    _pauseView    = null;
        Dispose(_gameOverView); _gameOverView = null;
        Dispose(_upgradeView);  _upgradeView  = null;
    }

    private void InitJoystick()
    {
        // Sanal joystick bileşenleri derinlemesine aranarak referansları atanır.
        _joystickTouch    = FindDeep(_mainView, "joystick_touch");
        _joystickCenter   = FindDeep(_mainView, "joystick_center");
        _joystickThumb    = FindDeep(_mainView, "joystick_thumb");
        _joystickOutline  = FindDeep(_mainView, "joystick_outline");

        if (_joystickTouch == null || _joystickThumb == null) return;

        // Dokunma alanının tıklanabilir ve görünmez bir diktörtgen olması sağlanır.
        if (_joystickTouch is GComponent touchCom) 
        {
            touchCom.opaque = true;
        }
        else if (_joystickTouch is GGraph touchGraph) 
        {
            touchGraph.touchable = true;
            touchGraph.DrawRect(touchGraph.width, touchGraph.height, 0, Color.clear, Color.clear); 
        }

        // Joystick merkez noktaları ve tutamacının merkez eksenleri ayarlanır.
        _joystickThumb.pivotAsAnchor = true;
        _joystickThumb.SetPivot(0.5f, 0.5f);

        if (_joystickCenter != null)
        {
            _joystickCenter.pivotAsAnchor = true;
            _joystickCenter.SetPivot(0.5f, 0.5f);
        }

        // Joystick'in ilk başlangıç pozisyonu hesaplanır ve tutamaç oraya yerleştirilir.
        _joystickStartPos = _joystickCenter != null
            ? new Vector2(_joystickCenter.x, _joystickCenter.y)
            : new Vector2(_joystickTouch.x + _joystickTouch.width / 2f, _joystickTouch.y + _joystickTouch.height / 2f);

        _joystickThumb.SetXY(_joystickStartPos.x, _joystickStartPos.y);

        // Dokunma girdileri joystick fonksiyonlarına bağlanır.
        _joystickTouch.touchable = true;
        _joystickTouch.onTouchBegin.Add(OnJoystickTouchBegin);
        _joystickTouch.onTouchMove.Add(OnJoystickTouchMove);
        _joystickTouch.onTouchEnd.Add(OnJoystickTouchEnd);
    }

    private void InitAbilityButtons()
    {
        // Toplam 5 adet yetenek butonunun tıklama olayları index bazlı olarak yetenek yöneticisine bağlanır.
        for (int i = 0; i < 5; i++)
        {
            _abilityButtons[i] = FindDeep(_mainView, "buttonAbility" + i)?.asButton;
            if (_abilityButtons[i] == null) continue;
            int index = i;
            _abilityButtons[i].onClick.Add(() => AbilityManager.Instance?.TryActivateAbility(index));
        }

        // Dash butonunun tıklama olayı oyuncu sınıfına bağlanır ve animasyonu tetiklenir.
        _dashButton = FindDeep(_mainView, "buttonDash")?.asButton;
        if (_dashButton != null)
        {
            _dashButton.onClick.Add(() =>
            {
                _player?.Dash();
                TriggerDash();
            });
        }
    }

    public void ShowScreen(int index)
    {
        // Gönderilen indexe göre duraklatma, oyun bitti veya geliştirme ekranlarının görünürlüğü açılır veya kapatılır.
        if (_pauseView    != null) _pauseView.visible    = index == 1;
        if (_gameOverView != null) _gameOverView.visible = index == 2;
        if (_upgradeView  != null) _upgradeView.visible  = index == 3;
    }

    public void ShowGameOverScreen(string finalTime, string bestTime)
    {
        // Oyun bitiş ekranı açılır ve güncel hayatta kalma süresi metne yazdırılır.
        ShowScreen(2);
        if (GameManager.Instance != null)
        {
            if (_gameScoreText != null) _gameScoreText.text = GameManager.Instance.GetCurrentTimeText();
        }
    }

    public void ShowUpgradeScreen(List<string> selectedIDs)
    {
        // Oyun zaman akışı durdurulur ve geliştirme seçim ekranı gösterilir.
        Time.timeScale = 0f;
        ShowScreen(3);

        // Rastgele çarpanlara göre butonların controller sayfa indexleri ve geliştirme ID'leri belirlenir.
        int statRoll = Random.Range(1, 5);
        int cTypeVal = statRoll - 1;
        int cSizeVal = cTypeVal;
        string btn1ID = statRoll switch
        {
            1 => "HP",
            2 => "ATK",
            3 => "ASPD",
            4 => "SPD",
            _ => "HP"
        };

        int ab2AbilityType  = Random.Range(1, 6);
        int ab2UpgradeType  = Random.Range(1, 4);
        int ab3AbilityType  = Random.Range(1, 5);
        if (ab3AbilityType >= ab2AbilityType) ab3AbilityType++;
        int ab3UpgradeType  = Random.Range(1, 4);

        string ab2ID = AbilityPrefixFromType(ab2AbilityType) + UpgradeSuffixFromType(ab2UpgradeType);
        string ab3ID = AbilityPrefixFromType(ab3AbilityType) + UpgradeSuffixFromType(ab3UpgradeType);

        // 1. Geliştirme butonu tıklama olayı ve FairyGUI UI controller durumları güncellenir.
        SetButtonVisible(_upgradeButton1, true);
        _upgradeButton1.onClick.Clear();
        _upgradeButton1.onClick.Add(() => { LevelManager.Instance?.ExecuteUpgradeByID(btn1ID); HideUpgradeScreen(); GameManager.Instance?.ResumeGame(); });
        SafeSetSelectedIndex(_upgradeButton1.GetController("cType"),     cTypeVal);
        SafeSetSelectedIndex(_upgradeButton1.GetController("cSize"),     cSizeVal);
        SafeSetSelectedIndex(_upgradeButton1.GetController("cFontSize"), 2);

        // 2. Geliştirme butonu tıklama olayı ve ilgili UI controller durumları güncellenir.
        SetButtonVisible(_upgradeButton2, true);
        _upgradeButton2.onClick.Clear();
        _upgradeButton2.onClick.Add(() => { LevelManager.Instance?.ExecuteUpgradeByID(ab2ID); HideUpgradeScreen(); GameManager.Instance?.ResumeGame(); });
        SafeSetSelectedIndex(_upgradeButton2.GetController("cAbilityType"),  ab2AbilityType);
        SafeSetSelectedIndex(_upgradeButton2.GetController("cUpgradeType"),  ab2UpgradeType);
        SafeSetSelectedIndex(_upgradeButton2.GetController("cCooldown"),     1);
        SafeSetSelectedIndex(_upgradeButton2.GetController("cTitle"),        1);
        SafeSetSelectedIndex(_upgradeButton2.GetController("cFontSize"),     0);
        SafeSetSelectedIndex(_upgradeButton2.GetController("cHeaderSize"),   2);
        SafeSetSelectedIndex(_upgradeButton2.GetController("cHeader"),       0);
        SafeSetSelectedIndex(_upgradeButton2.GetController("cHeaderOutline"),0);

        // 3. Geliştirme butonu tıklama olayı ve ilgili UI controller durumları güncellenir.
        SetButtonVisible(_upgradeButton3, true);
        _upgradeButton3.onClick.Clear();
        _upgradeButton3.onClick.Add(() => { LevelManager.Instance?.ExecuteUpgradeByID(ab3ID); HideUpgradeScreen(); GameManager.Instance?.ResumeGame(); });
        SafeSetSelectedIndex(_upgradeButton3.GetController("cAbilityType"),  ab3AbilityType);
        SafeSetSelectedIndex(_upgradeButton3.GetController("cUpgradeType"),  ab3UpgradeType);
        SafeSetSelectedIndex(_upgradeButton3.GetController("cCooldown"),     1);
        SafeSetSelectedIndex(_upgradeButton3.GetController("cTitle"),        1);
        SafeSetSelectedIndex(_upgradeButton3.GetController("cFontSize"),     0);
        SafeSetSelectedIndex(_upgradeButton3.GetController("cHeaderSize"),   2);
        SafeSetSelectedIndex(_upgradeButton3.GetController("cHeader"),       0);
        SafeSetSelectedIndex(_upgradeButton3.GetController("cHeaderOutline"),0);
    }

    // Geliştirme türüne göre ilgili yetenek ön ek dizilimini döndürür.
    private static string AbilityPrefixFromType(int t) => t switch
    {
        1 => "B_",
        2 => "S_",
        3 => "R_",
        4 => "M_",
        5 => "RG_",
        _ => "B_"
    };

    // Geliştirme türüne göre ilgili nitelik son ek dizilimini döndürür.
    private static string UpgradeSuffixFromType(int t) => t switch
    {
        1 => "CD",
        2 => "DUR",
        3 => "PWR",
        _ => "CD"
    };

    private void SafeSetSelectedIndex(Controller controller, int index)
    {
        // Controller'ın belirtilen sayfa indeksine güvenli bir şekilde geçiş yapmasını sağlar.
        if (controller != null && index >= 0 && index < controller.pageCount)
            controller.selectedIndex = index;
    }

    private void HideUpgradeScreen()
    {
        // Geliştirme paneli kapatılır, butonlar gizlenir ve zaman akışı normale döndürülür.
        if (_upgradeView != null) _upgradeView.visible = false;
        SetButtonVisible(_upgradeButton1, false);
        SetButtonVisible(_upgradeButton2, false);
        SetButtonVisible(_upgradeButton3, false);
        Time.timeScale = 1f;
    }

    private static void SetButtonVisible(GButton btn, bool visible)
    {
        // Butonun hem görünürlüğünü hem de tıklanabilirlik durumunu senkronize şekilde değiştirir.
        if (btn == null) return;
        btn.visible   = visible;
        btn.touchable = visible;
    }

    public void UpdateExpBar(float current, float max)
    {
        // Tecrübe barının maksimum kapasitesini ve mevcut doluluk değerini günceller.
        if (_expBar == null) return;
        _expBar.max   = max;
        _expBar.value = current;
    }

    public void ShowNotification(string message)
    {
        // Ekranda geçici bir bildirim mesajı gösterir ve zamanlayıcı döngüsünü başlatır.
        if (_notificationText == null) return;
        _notificationText.visible = true;
        _notificationText.text    = message;
        StartCoroutine(HideNotificationAfterDelay(3f));
    }

    private System.Collections.IEnumerator HideNotificationAfterDelay(float delay)
    {
        // Belirtilen süre kadar bekledikten sonra bildirim metnini ekrandan gizler.
        yield return new WaitForSeconds(delay);
        if (_notificationText != null) _notificationText.visible = false;
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // Anlık geçen oyun süresi skoru her kare güncellenir.
        if (_scoreText != null) _scoreText.text = GameManager.Instance.GetCurrentTimeText();

        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused) return;

        // Bekleme süreleri görselleri, dash butonu ve klavye joystick simülasyonu güncellenir.
        UpdateCooldownVisuals();
        UpdateDashButton();
        UpdateJoystickKeyboard();
    }

    private void UpdateDashButton()
    {
        if (_dashButton == null || _player == null) return;
        GObject cdObj = _dashButton.GetChild("cooldown");
        GObject btn = _dashButton;
        if (cdObj != null)
        {
            // Oyuncunun dash yeteneğinin dolum oranına göre butonun tıklanabilirliği ve karartma efekti ayarlanır.
            float cd     = _player.GetDashCD();
            float maxCd  = _player.GetMaxDashCD();
            float amount = maxCd > 0 ? cd / maxCd : 0f;
            if (amount > 0.001f)
            {
                btn.grayed = true;
                btn.touchable = false;
            }
            else
            {
                btn.grayed = false;
                btn.touchable = true;
            }
            if (cdObj is GImage img)       img.fillAmount = amount;
            else if (cdObj is GLoader ldr) ldr.fillAmount = amount;
            cdObj.visible = amount > 0.001f;
        }
        _dashButton.title = _player.GetDashCD() > 0 ? Mathf.CeilToInt(_player.GetDashCD()).ToString() : string.Empty;
    }

    private void UpdateJoystickKeyboard()
    {
        // Eğer ekranda aktif bir dokunmatik joystick girdisi yoksa klavye yön tuşlarına göre arayüz joystick'i simüle edilir.
        if (_joystickThumb == null || _joystickTouchId != -1) return;

        Vector2 kb = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (kb.sqrMagnitude > 0.001f)
        {
            float rad = Mathf.Atan2(-kb.y, kb.x);
            _joystickThumb.rotation = rad * Mathf.Rad2Deg + 90f;
            float maxDist = _joystickTouch != null ? _joystickTouch.width / 2f : 100f;
            _joystickThumb.SetXY(
                _joystickStartPos.x + kb.x * maxDist,
                _joystickStartPos.y - kb.y * maxDist);
        }
        else
        {
            // Klavye girdisi bittiğinde joystick tutamacı başlangıç pozisyonuna geri çekilir.
            _joystickThumb.SetXY(_joystickStartPos.x, _joystickStartPos.y);
            _joystickThumb.rotation = 0f;
        }
    }

    private void UpdateCooldownVisuals()
    {
        if (AbilityManager.Instance == null) return;
        
        // 5 farklı yetenek butonunun anlık dolum oranları ve kalan saniye başlıkları güncellenir.
        for (int i = 0; i < 5; i++)
        {
            GButton btn = _abilityButtons[i];
            if (btn == null) continue;
            GObject cdObj = btn.GetChild("cooldown");
            if (cdObj != null)
            {
                float amount = AbilityManager.Instance.GetCooldownFill(i);
                if (amount > 0.001f)
                {
                    btn.grayed = true;
                    btn.touchable = false;
                }
                else
                {
                    btn.grayed = false;
                    btn.touchable = true;
                }
                if (cdObj is GImage img)       img.fillAmount = amount;
                else if (cdObj is GLoader ldr) ldr.fillAmount = amount;
                cdObj.visible = amount > 0.001f;
            }
            btn.title = AbilityManager.Instance.GetCooldownText(i);
        }
    }

    private void OnJoystickTouchBegin(EventContext context)
    {
        // Dokunma başladığı an ilgili dokunma ID'si kilitlenir ve hareket fonksiyonuna yönlendirilir.
        if (_joystickTouchId != -1) return;

        InputEvent inputEvt = context.inputEvent;
        _joystickTouchId = inputEvt.touchId;
        context.CaptureTouch();

        HandleJoystickMovement(inputEvt);
    }

    private void OnJoystickTouchMove(EventContext context)
    {
        // Parmağın sürüklenme ID'si kilitli ID ile uyuşuyorsa joystick konumu güncellenir.
        InputEvent inputEvt = context.inputEvent;
        if (_joystickTouchId == inputEvt.touchId)
            HandleJoystickMovement(inputEvt);
    }

    private void OnJoystickTouchEnd(EventContext context)
    {
        // Dokunma bırakıldığında değerler sıfırlanır ve joystick tutamacı merkeze döner.
        InputEvent inputEvt = context.inputEvent;
        if (_joystickTouchId == inputEvt.touchId)
        {
            _joystickTouchId = -1;
            _joystickInput = Vector2.zero;

            if (_joystickThumb != null)
            {
                _joystickThumb.SetXY(_joystickStartPos.x, _joystickStartPos.y);
                _joystickThumb.rotation = 0f;
            }
        }
    }

    private void HandleJoystickMovement(InputEvent inputEvt)
    {
        if (_joystickThumb == null || _mainView == null || _joystickTouch == null) return;

        // Global ekran dokunma koordinatları FairyGUI yerel koordinatlarına çevrilir.
        Vector2 globalPt = new Vector2(inputEvt.x, inputEvt.y);
        Vector2 localPt  = GRoot.inst.GlobalToLocal(globalPt);

        // Joystick merkez noktasından olan uzaklık ve sapma açısı hesaplanır.
        Vector2 offset      = localPt - _joystickStartPos;
        float maxDist       = _joystickTouch.width / 2f;
        float currentDist   = offset.magnitude;

        _joystickThumb.rotation = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg + 90f;

        // Tutamacın çerçevenin dışına taşması engellenir.
        Vector2 clampedOffset = currentDist > maxDist ? offset.normalized * maxDist : offset;

        _joystickThumb.SetXY(_joystickStartPos.x + clampedOffset.x, _joystickStartPos.y + clampedOffset.y);

        // Alınan yön girdisi normale çevrilerek oyuncunun okuyabileceği eksen verisine dönüştürülür.
        _joystickInput = currentDist < 2f ? Vector2.zero : new Vector2(clampedOffset.x, -clampedOffset.y).normalized;
    }

    // Atılma butonunun tıklanma geçiş animasyonunu tetikler.
    private void TriggerDash() => _dashButton?.GetTransition("pressAnim")?.Play();

    public Vector2 GetJoystickAxis()
    {
        // Eğer sanal joystick girdisi varsa o döndürülür, yoksa klavyedeki yön girdileri döndürülür.
        if (_joystickInput.sqrMagnitude > 0.001f) return _joystickInput;
        Vector2 kb = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        return kb.sqrMagnitude > 0.001f ? kb.normalized : Vector2.zero;
    }

    private GObject FindDeep(GComponent parent, string name)
    {
        // Arayüz hiyerarşisi içerisinde derinlemesine isim bazlı bileşen araması yapar.
        if (parent == null) return null;
        GObject direct = parent.GetChild(name);
        if (direct != null) return direct;
        for (int i = 0; i < parent.numChildren; i++)
        {
            GObject child = parent.GetChildAt(i);
            if (child.name == name) return child;
            if (child is GComponent com)
            {
                GObject found = FindDeep(com, name);
                if (found != null) return found;
            }
        }
        return null;
    }

    void OnDestroy() => CleanUp();
}

public static class ControllerExtensions
{
    public static void Apply(this Controller c, System.Action<Controller> action) => action(c);
}