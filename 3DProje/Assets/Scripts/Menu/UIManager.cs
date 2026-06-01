using FairyGUI;
using Menu;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private GComponent _mainView;
    private GComponent _settingsView;
    private GComponent _htpView;
    private GComponent _detailsView;
    private GComponent _generalView;
    private GComponent _cardCom;
    private GTextField _splashTextField;
    
    private Controller _cardController;
    private GButton _btnNext, _btnPrev;

    void Awake() 
    {
        GRoot.inst.SetContentScaleFactor(1920, 1080, UIContentScaler.ScreenMatchMode.MatchWidthOrHeight);
    }

    void Start()
    {
        GRoot.inst.RemoveChildren(0, -1, true);
        UIPackage.RemoveAllPackages();

        UIPackage.AddPackage("FairyUI/packageMenu");
        UIPackage.AddPackage("FairyUI/packageShared");

        _mainView = UIPackage.CreateObject("packageMenu", "componentMainMenu").asCom;
        _settingsView = UIPackage.CreateObject("packageMenu", "componentSettings").asCom;
        _htpView = UIPackage.CreateObject("packageMenu", "componentHTP").asCom;
        _detailsView = UIPackage.CreateObject("packageMenu", "componentDetails").asCom;
        _generalView = UIPackage.CreateObject("packageMenu", "componentGeneral").asCom;
        
        _cardCom = UIPackage.CreateObject("packageMenu", "componentCard").asCom;
        _detailsView.AddChild(_cardCom);
        _cardCom.SetXY(0, 425);

        _cardController = _cardCom.GetController("cCard");
        _btnNext = _cardCom.GetChild("buttonNextCard1").asButton;
        _btnPrev = _cardCom.GetChild("buttonPreviousCard2").asButton;

        SetupView(_mainView);
        SetupView(_settingsView, false);
        SetupView(_htpView, false);
        SetupView(_detailsView, false);
        SetupView(_generalView, false);

        InitializeUI();

        if (StageCamera.main != null) StageCamera.main.allowMSAA = true;
    }

    void SetupView(GComponent view, bool isVisible = true)
    {
        view.SetSize(GRoot.inst.width, GRoot.inst.height);
        view.AddRelation(GRoot.inst, RelationType.Size);
        view.visible = isVisible;
        GRoot.inst.AddChild(view);
    }

    void InitializeUI()
    {
        _splashTextField = _mainView.GetChild("textSplash")?.asTextField;
        if (_splashTextField != null)
        {
            _splashTextField.color = Color.yellow; 
            SetRandomSplashText();
        }

        SetupNavigationButton(_mainView, "buttonPlay", () => PlayGame());
        SetupNavigationButton(_mainView, "buttonSettings", () => SwitchView(_settingsView));
        SetupNavigationButton(_mainView, "buttonHTP", () => SwitchView(_htpView));
        
        SetupNavigationButton(_htpView, "buttonDetails", () => SwitchView(_detailsView));
        SetupNavigationButton(_htpView, "buttonGeneral", () => SwitchView(_generalView));
        SetupNavigationButton(_htpView, "buttonBack", () => SwitchView(_mainView));
        
        SetupNavigationButton(_settingsView, "buttonBack", () => SwitchView(_mainView));
        SetupNavigationButton(_detailsView, "buttonBack", () => SwitchView(_htpView));
        SetupNavigationButton(_generalView, "buttonBack", () => SwitchView(_htpView));

        SetupCategoryButton("buttonEnemies", 0);
        SetupCategoryButton("buttonStats", 3);
        SetupCategoryButton("buttonAbilities", 7);
        SetupCategoryButton("buttonLevels", 13);

        _btnNext.onClick.Set(() => _cardController.selectedIndex++);
        _btnPrev.onClick.Set(() => _cardController.selectedIndex--);
        
        _cardController.onChanged.Add(UpdateCardNavigation);
        UpdateCardNavigation();
    }

    void SetupCategoryButton(string btnName, int index)
    {
        GButton btn = _detailsView.GetChild(btnName)?.asButton;
        if (btn != null) btn.onClick.Set(() => _cardController.selectedIndex = index);
    }

    void UpdateCardNavigation()
    {
        int i = _cardController.selectedIndex;
        _btnPrev.visible = !(i == 0 || i == 3 || i == 7 || i == 13);
        _btnNext.visible = !(i == 2 || i == 6 || i == 12 || i == 15);
    }

    void SwitchView(GComponent targetView)
    {
        _mainView.visible = _settingsView.visible = _htpView.visible = _detailsView.visible = _generalView.visible = false;
        targetView.visible = true;
    }

    void SetRandomSplashText()
    {
        _splashTextField.text = SplashData.SplashTexts[Random.Range(0, SplashData.SplashTexts.Length)];
        _mainView.GetTransition("t0")?.Play(-1, 0, null);
    }

    void SetupNavigationButton(GComponent parent, string btnName, UnityEngine.Events.UnityAction action)
    {
        parent.GetChild(btnName)?.asButton.onClick.Set(() => action());
    }

    void PlayGame()
    {
        GRoot.inst.RemoveChildren(0, -1, true);
        UIPackage.RemoveAllPackages();
        SceneManager.LoadScene("GameScene");
    }
}