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
            _expBar = FindChildRecursive(_mainView, "Level Bar")?.asProgress;
            _notificationText = FindChildRecursive(_mainView, "Notification")?.asTextField;
            if (_notificationText != null) _notificationText.visible = false;

            CacheAndResetUpgradeButtons();

            _joystick = FindChildRecursive(_mainView, "Joystick")?.asCom;
            if (_joystick != null)
            {
                _joystick.touchable = true;
                _joystickThumb = FindChildRecursive(_joystick, "Joystick Handle") ?? FindChildRecursive(_joystick, "thumb");
                if (_joystickThumb != null)
                {
                    _joystickStartPos = _joystickThumb.xy;
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
                if (ph != null)
                {
                    btn.xy = ph.xy;
                    Debug.Log("[UpgradeCache] Button '" + id + "' cached and moved to Ability Placeholder " + phIdx);
                }
                else
                {
                    Debug.LogWarning("[UpgradeCache] Placeholder 'Ability Placeholder " + phIdx + "' not found for button '" + id + "'");
                }
            }
            else
            {
                Debug.LogWarning("[UpgradeCache] Button not found for ID: " + id);
            }
        }
        Debug.Log("[UpgradeCache] Total buttons cached: " + _upgradeButtonCache.Count);
    }

    public void ShowUpgradeScreen()
    {
        Debug.Log("[Upgrade] ShowUpgradeScreen called");
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
            Debug.Log("[Upgrade] CAbility 1 (stat only) selected: " + chosen[0]);
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
            Debug.Log("[Upgrade] CAbility 2 (mix) selected: " + chosen[1]);
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
            Debug.Log("[Upgrade] CAbility 3 (ability only) selected: " + chosen[2]);
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
                    Debug.Log("[Upgrade] Button '" + chosen[i] + "' placed at " + cPlaceholderNames[i] + ", visible=true");
                }
                else
                {
                    Debug.LogWarning("[Upgrade] " + cPlaceholderNames[i] + " not found!");
                }
            }
        }
    }

    public void ShowUpgradeScreen(List<UpgradeOption> options, System.Action<string> onSelect)
    {
        Debug.Log("[Upgrade] ShowUpgradeScreen (legacy) called with " + (options != null ? options.Count : 0) + " options");
        ShowUpgradeScreen();
    }

    private void OnUpgradeSelected(string id)
    {
        Debug.Log("[Upgrade] OnUpgradeSelected: " + id);
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ExecuteUpgradeByID(id);
            Debug.Log("[Upgrade] ExecuteUpgradeByID called for: " + id);
        }
        else
        {
            Debug.LogError("[Upgrade] LevelManager.Instance is null!");
        }

        Controller managerI = _mainView.GetController("ManagerI");
        if (managerI != null)
        {
            managerI.selectedIndex = 0;
            Debug.Log("[Upgrade] ManagerI controller set to 0 (gameplay scene)");
        }
        else
        {
            Debug.LogError("[Upgrade] ManagerI controller not found!");
        }

        Time.timeScale = 1;
        Debug.Log("[Upgrade] Time.timeScale set to 1");
    }

    private void OnJoystickTouchBegin(EventContext context)
    {
        context.CaptureTouch();
        UpdateJoystickInput(context);
    }

    private void OnJoystickTouchMove(EventContext context)
    {
        UpdateJoystickInput(context);
    }

    private void OnJoystickTouchEnd(EventContext context)
    {
        if (_joystickThumb != null) _joystickThumb.xy = _joystickStartPos;
        _joystickInput = Vector2.zero;
    }

    private void UpdateJoystickInput(EventContext context)
    {
        if (_joystick == null || _joystickThumb == null) return;
        Vector2 localPt = _joystick.GlobalToLocal(context.inputEvent.position);
        Vector2 center = new Vector2(_joystick.width / 2, _joystick.height / 2);
        Vector2 dist = localPt - center;
        float maxDist = _joystick.width / 2;
        _joystickInput = dist.normalized;
        if (dist.magnitude <= maxDist) _joystickThumb.xy = localPt;
        else _joystickThumb.xy = center + (_joystickInput * maxDist);
        if (dist.magnitude < 2f) _joystickInput = Vector2.zero;
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
        if (_bestScoreText != null) _bestScoreText.text = "Best: " + GameManager.Instance.GetBestTimeText();

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
            _mainView.GetTransition("ShowNotification")?.Play(() => {
                _notificationText.visible = false;
            });
        }
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
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }
    void OnDestroy() => CleanUp();
}
