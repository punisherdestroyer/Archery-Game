using FairyGUI;
using UnityEngine;
using System.Collections.Generic;
using System;

public class InGameUIController : MonoBehaviour
{
    public static InGameUIController Instance { get; private set; }
    public GComponent _mainView;
    private Controller _screenController;
    private GTextField _scoreText;
    private GTextField _bestScoreText;
    private GTextField _gameBestScoreText;
    private GTextField _gameScoreText;
    private GProgressBar _expBar;
    private GTextField _notificationText;
    private GButton[] _abilityButtons = new GButton[5];
    private GButton _dashButton;
    private GComponent _joystick;
    private GObject _joystickThumb;
    private Vector2 _joystickStartPos;
    private Vector2 _joystickInput;

    private Player _player;

    private Dictionary<string, GButton> _upgradeButtonCache = new Dictionary<string, GButton>();
    public Vector2 upgradeOffset = Vector2.zero;

    private static readonly string[] STAT_IDS = { "HP", "ATK", "ASPD", "SPD" };
    private static readonly string[] ABILITY_IDS = { "B_DMG", "B_CD", "B_DUR", "S_PWR", "S_CD", "S_DUR", "R_PWR", "R_CD", "R_DUR", "M_PWR", "M_CD", "M_DUR", "RG_PWR", "RG_CD", "RG_DUR" };
    private static readonly string[] ALL_IDS = { "HP", "ATK", "ASPD", "SPD", "B_DMG", "B_CD", "B_DUR", "S_PWR", "S_CD", "S_DUR", "R_PWR", "R_CD", "R_DUR", "M_PWR", "M_CD", "M_DUR", "RG_PWR", "RG_CD", "RG_DUR" };

    void Awake()
    {
        if (Instance != null)
        {
            Instance.CleanUp();
            Destroy(Instance.gameObject);
        }
        Instance = this;
        GRoot.inst.SetContentScaleFactor(1920, 1080, UIContentScaler.ScreenMatchMode.MatchWidthOrHeight);
    }

    private void CleanUp()
    {
        if (_mainView != null)
        {
            _mainView.RemoveFromParent();
            _mainView.Dispose();
        }
    }

    void Start()
    {
        _player = UnityEngine.Object.FindFirstObjectByType<Player>();
        if (UIPackage.GetByName("InGameUI") == null)
        {
            UIPackage.AddPackage("FairyUI/InGameUI");
            UIPackage.AddPackage("FairyUI/UIAssets");
        }

        _mainView = UIPackage.CreateObject("InGameUI", "InGameUI").asCom;
        if (_mainView != null)
        {
            _mainView.MakeFullScreen();
            GRoot.inst.AddChild(_mainView);

            _screenController = _mainView.GetController("Manager");
            if (_screenController != null) _screenController.selectedIndex = 0;
            _scoreText = FindChildRecursive(_mainView, "IScore")?.asTextField;
            _bestScoreText = FindChildRecursive(_mainView, "IBest Score")?.asTextField;
            _gameBestScoreText = FindChildRecursive(_mainView, "GBest Score")?.asTextField;
            _gameScoreText = FindChildRecursive(_mainView, "GScore")?.asTextField;
            _expBar = FindChildRecursive(_mainView, "Level Bar")?.asProgress;
            _notificationText = FindChildRecursive(_mainView, "Notification")?.asTextField;
            if (_notificationText != null) _notificationText.visible = false;

            CacheAndResetUpgradeButtons();

            _joystick = FindChildRecursive(_mainView, "joystick")?.asCom;
            if (_joystick != null)
            {
                _joystick.touchable = true;
                _joystickThumb = FindChildRecursive(_joystick, "thumb");
                GObject center = FindChildRecursive(_joystick, "joystick_center");
                if (_joystickThumb != null)
                {
                    _joystickThumb.pivotAsAnchor = true;
                    _joystickThumb.SetPivot(0.5f, 0.5f);
                    if (center != null)
                        _joystickStartPos = new Vector2(center.x + center.width / 2, center.y + center.height / 2);
                    else
                        _joystickStartPos = new Vector2(_joystick.width / 2, _joystick.height / 2);
                    _joystickThumb.SetXY(_joystickStartPos.x - _joystickThumb.width / 2, _joystickStartPos.y - _joystickThumb.height / 2);
                    _joystick.onTouchBegin.Add(OnJoystickTouchBegin);
                    _joystick.onTouchMove.Add(OnJoystickTouchMove);
                    _joystick.onTouchEnd.Add(OnJoystickTouchEnd);
                }
            }

            for (int i = 0; i < 5; i++)
            {
                _abilityButtons[i] = FindChildRecursive(_mainView, "Ability_" + i)?.asButton;
                if (_abilityButtons[i] != null)
                {
                    int index = i;
                    _abilityButtons[i].onClick.Add(() => AbilityManager.Instance?.TryActivateAbility(index));
                }
            }

            _dashButton = FindChildRecursive(_mainView, "Dash Button")?.asButton;
            if (_dashButton != null)
            {
                _dashButton.onClick.Add(() => {
                    if (_player != null) _player.Dash();
                    TriggerDash();
                });
            }

            FindChildRecursive(_mainView, "Pause Button")?.asButton?.onClick.Add(() => GameManager.Instance?.PauseGame());
            FindChildRecursive(_mainView, "Resume Button")?.asButton?.onClick.Add(() => GameManager.Instance?.ResumeGame());
            FindChildRecursive(_mainView, "PMenu Button")?.asButton?.onClick.Add(() => GameManager.Instance?.GoToMainMenu());
            FindChildRecursive(_mainView, "Restart Button")?.asButton?.onClick.Add(() => GameManager.Instance?.RestartGame());
            FindChildRecursive(_mainView, "GRestart Button")?.asButton?.onClick.Add(() => GameManager.Instance?.RestartGame());
            FindChildRecursive(_mainView, "GMenu Button")?.asButton?.onClick.Add(() => GameManager.Instance?.GoToMainMenu());
            FindChildRecursive(_mainView, "Skip Button")?.asButton?.onClick.Add(() => GameManager.Instance?.ResumeGame());
        }
    }

    private void CacheAndResetUpgradeButtons()
    {
        _upgradeButtonCache.Clear();
        GComponent target = FindChildRecursive(_mainView, "Upgrade")?.asCom ?? _mainView;

        for (int i = 0; i < ALL_IDS.Length; i++)
        {
            string id = ALL_IDS[i];
            GButton btn = FindUpgradeButtonInFairy(target, id);
            if (btn != null)
            {
                _upgradeButtonCache[id] = btn;
                btn.visible = false;
                btn.touchable = false;
                int phIdx = i + 1;
                GObject ph = FindChildRecursive(target, "Ability Placeholder " + phIdx);
                if (ph != null) btn.xy = ph.xy;
            }
        }
    }

    public void ShowUpgradeScreen()
    {
        ShowScreen(3);
        GComponent target = FindChildRecursive(_mainView, "Upgrade")?.asCom ?? _mainView;
        target.touchable = true;

        foreach (var kvp in _upgradeButtonCache)
        {
            kvp.Value.visible = false;
            kvp.Value.touchable = false;
            kvp.Value.onClick.Clear();
        }

        for (int i = 0; i < ALL_IDS.Length; i++)
        {
            string id = ALL_IDS[i];
            if (_upgradeButtonCache.TryGetValue(id, out GButton btn))
            {
                int phIdx = i + 1;
                GObject ph = FindChildRecursive(target, "Ability Placeholder " + phIdx);
                if (ph != null) btn.xy = ph.xy;
            }
        }

        HashSet<string> usedIDs = new HashSet<string>();
        string[] chosen = new string[3];

        List<string> statList = new List<string>(STAT_IDS);
        if (statList.Count > 0)
        {
            int r = UnityEngine.Random.Range(0, statList.Count);
            chosen[0] = statList[r];
            usedIDs.Add(chosen[0]);
        }

        List<string> mixList = new List<string>();
        foreach (string id in ALL_IDS)
        {
            if (!usedIDs.Contains(id)) mixList.Add(id);
        }
        if (mixList.Count > 0)
        {
            int r = UnityEngine.Random.Range(0, mixList.Count);
            chosen[1] = mixList[r];
            usedIDs.Add(chosen[1]);
        }

        List<string> abilityList = new List<string>();
        foreach (string id in ABILITY_IDS)
        {
            if (!usedIDs.Contains(id)) abilityList.Add(id);
        }
        if (abilityList.Count > 0)
        {
            int r = UnityEngine.Random.Range(0, abilityList.Count);
            chosen[2] = abilityList[r];
            usedIDs.Add(chosen[2]);
        }

        string[] cPlaceholderNames = { "CAbility Placeholder 1", "CAbility Placeholder 2", "CAbility Placeholder 3" };
        for (int i = 0; i < 3; i++)
        {
            if (string.IsNullOrEmpty(chosen[i])) continue;
            if (_upgradeButtonCache.TryGetValue(chosen[i], out GButton btn))
            {
                GObject cph = FindChildRecursive(target, cPlaceholderNames[i]);
                if (cph != null)
                {
                    btn.xy = cph.xy + upgradeOffset;
                    btn.visible = true;
                    btn.touchable = true;
                    string capturedID = chosen[i];
                    btn.onClick.Set(() => OnUpgradeSelected(capturedID));
                }
            }
        }
    }

    public void ShowUpgradeScreen(List<UpgradeOption> options, System.Action<string> onSelect)
    {
        ShowUpgradeScreen();
    }

    private void OnUpgradeSelected(string id)
    {
        if (LevelManager.Instance != null) LevelManager.Instance.ExecuteUpgradeByID(id);

        Controller managerI = _mainView.GetController("ManagerI");
        if (managerI != null) managerI.selectedIndex = 0;

        Time.timeScale = 1;
    }

    private void OnJoystickTouchBegin(EventContext context)
    {
        context.CaptureTouch();
        _joystickInput = Vector2.zero;
        UpdateJoystickInput(context);
    }

    private void OnJoystickTouchMove(EventContext context)
    {
        UpdateJoystickInput(context);
    }

    private void OnJoystickTouchEnd(EventContext context)
    {
        _joystickInput = Vector2.zero;
        if (_joystickThumb != null)
        {
            _joystickThumb.SetXY(_joystickStartPos.x - _joystickThumb.width / 2, _joystickStartPos.y - _joystickThumb.height / 2);
            _joystickThumb.rotation = 0f;
        }
    }

    private void UpdateJoystickInput(EventContext context)
    {
        if (_joystick == null || _joystickThumb == null) return;
        Vector2 localPt = _joystick.GlobalToLocal(context.inputEvent.position);
        Vector2 center = _joystickStartPos;
        Vector2 offset = localPt - center;
        float maxDist = _joystick.width / 2f;
        float rad = Mathf.Atan2(offset.y, offset.x);
        _joystickThumb.rotation = rad * Mathf.Rad2Deg + 90f;
        float dist = offset.magnitude;
        Vector2 clamped = dist > maxDist ? offset.normalized * maxDist : offset;
        _joystickThumb.SetXY(center.x + clamped.x - _joystickThumb.width / 2, center.y + clamped.y - _joystickThumb.height / 2);
        if (dist < 5f)
            _joystickInput = Vector2.zero;
        else
            _joystickInput = new Vector2(offset.x, -offset.y).normalized;
    }

    private GObject FindChildRecursive(GComponent parent, string partialName)
    {
        if (parent == null) return null;
        GObject directChild = parent.GetChild(partialName);
        if (directChild != null) return directChild;
        for (int i = 0; i < parent.numChildren; i++)
        {
            var child = parent.GetChildAt(i);
            if (child.name == partialName) return child;
            if (child is GComponent childCom)
            {
                GObject found = FindChildRecursive(childCom, partialName);
                if (found != null) return found;
            }
        }
        return null;
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        if (_scoreText != null) _scoreText.text = GameManager.Instance.GetCurrentTimeText();
        if (_bestScoreText != null) _bestScoreText.text = "BEST: " + GameManager.Instance.GetBestTimeText();
        if (_gameBestScoreText != null) _gameBestScoreText.text = "BEST: " + GameManager.Instance.GetBestTimeText();
        if (_gameScoreText != null) _gameScoreText.text = GameManager.Instance.GetCurrentTimeText();

        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused) return;

        UpdateCooldownVisuals();

        if (_dashButton != null && _player != null)
        {
            GObject cdObj = _dashButton.GetChild("cooldown");
            if (cdObj != null)
            {
                float cd = _player.GetDashCD();
                float maxCd = _player.GetMaxDashCD();
                float amount = maxCd > 0 ? (cd / maxCd) : 0;
                if (cdObj is GImage image) image.fillAmount = amount;
                else if (cdObj is GLoader loader) loader.fillAmount = amount;
                cdObj.visible = amount > 0.001f;
            }
            _dashButton.title = _player.GetDashCD() > 0 ? Mathf.CeilToInt(_player.GetDashCD()).ToString() : "";
        }

        Vector2 kb = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (_joystickThumb != null && _joystickInput.sqrMagnitude < 0.001f)
        {
            if (kb.sqrMagnitude > 0.001f)
            {
                float rad = Mathf.Atan2(-kb.y, kb.x);
                _joystickThumb.rotation = rad * Mathf.Rad2Deg + 90f;
                float maxDist = _joystick.width / 2f;
                _joystickThumb.SetXY(_joystickStartPos.x + kb.x * maxDist - _joystickThumb.width / 2, _joystickStartPos.y + (-kb.y) * maxDist - _joystickThumb.height / 2);
            }
            else
            {
                _joystickThumb.SetXY(_joystickStartPos.x - _joystickThumb.width / 2, _joystickStartPos.y - _joystickThumb.height / 2);
                _joystickThumb.rotation = 0f;
            }
        }
    }

    public void ShowScreen(int index)
    {
        Controller c = _mainView.GetController("ManagerI");
        if (c != null) c.selectedIndex = index;
    }

    public void ShowGameOverScreen(string finalTime, string bestTime) => ShowScreen(2);

    public void ShowNotification(string message)
    {
        if (_notificationText != null && _mainView != null)
        {
            _notificationText.visible = true;
            _notificationText.text = message;
            StartCoroutine(HideNotificationAfterDelay(3f));
        }
    }

    private System.Collections.IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_notificationText != null) _notificationText.visible = false;
    }

    public void UpdateExpBar(float current, float max)
    {
        if (_expBar != null)
        {
            _expBar.max = max;
            _expBar.value = current;
        }
    }

    private GButton FindUpgradeButtonInFairy(GComponent parent, string upgradeID)
    {
        if (parent == null) return null;
        for (int i = 0; i < parent.numChildren; i++)
        {
            GObject child = parent.GetChildAt(i);
            if (child is GButton btn && (child.name == upgradeID || child.name.Contains("{" + upgradeID + "}"))) return btn;
            if (child is GComponent com)
            {
                GButton found = FindUpgradeButtonInFairy(com, upgradeID);
                if (found != null) return found;
            }
        }
        return null;
    }

    private void UpdateCooldownVisuals()
    {
        if (AbilityManager.Instance == null) return;
        for (int i = 0; i < 5; i++)
        {
            GButton btn = _abilityButtons[i];
            if (btn == null) continue;
            GObject cdObj = btn.GetChild("cooldown");
            if (cdObj != null)
            {
                float amount = AbilityManager.Instance.GetCooldownFill(i);
                if (cdObj is GImage image) image.fillAmount = amount;
                else if (cdObj is GLoader loader) loader.fillAmount = amount;
                cdObj.visible = amount > 0.001f;
            }
            btn.title = AbilityManager.Instance.GetCooldownText(i);
        }
    }

    private void TriggerDash() => _dashButton?.GetTransition("pressAnim")?.Play();

    public Vector2 GetJoystickAxis()
    {
        if (_joystickInput.sqrMagnitude > 0.001f) return _joystickInput;
        Vector2 kb = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (kb.sqrMagnitude > 0.001f) return kb.normalized;
        return Vector2.zero;
    }

    void OnDestroy() => CleanUp();
}