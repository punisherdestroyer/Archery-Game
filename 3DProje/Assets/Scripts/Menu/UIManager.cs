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
        // FairyGUI için global içerik ölçekleme katsayıları ve ekran eşitleme modu ayarlanır.
        GRoot.inst.SetContentScaleFactor(1920, 1080, UIContentScaler.ScreenMatchMode.MatchWidthOrHeight);
    }

    void Start()
    {
        // Sahne başlangıcında eski arayüz elementleri temizlenir ve paket bellekleri boşaltılır.
        GRoot.inst.RemoveChildren(0, -1, true);
        UIPackage.RemoveAllPackages();

        // Menü ve ortak kullanılan arayüz paketleri FairyGUI kaynak dizininden projeye dahil edilir.
        UIPackage.AddPackage("FairyUI/packageMenu");
        UIPackage.AddPackage("FairyUI/packageShared");

        // Gerekli tüm görünüm bileşenleri ilgili paketlerden üretilir.
        _mainView = UIPackage.CreateObject("packageMenu", "componentMainMenu").asCom;
        _settingsView = UIPackage.CreateObject("packageMenu", "componentSettings").asCom;
        _htpView = UIPackage.CreateObject("packageMenu", "componentHTP").asCom;
        _detailsView = UIPackage.CreateObject("packageMenu", "componentDetails").asCom;
        _generalView = UIPackage.CreateObject("packageMenu", "componentGeneral").asCom;
        
        // Kart bileşeni üretilir, detaylar paneline alt nesne olarak eklenir ve konumu ayarlanır.
        _cardCom = UIPackage.CreateObject("packageMenu", "componentCard").asCom;
        _detailsView.AddChild(_cardCom);
        _cardCom.SetXY(0, 425);

        // Kart controller'ı ve kart değiştirme butonlarının referansları önbelleğe alınır.
        _cardController = _cardCom.GetController("cCard");
        _btnNext = _cardCom.GetChild("buttonNextCard1").asButton;
        _btnPrev = _cardCom.GetChild("buttonPreviousCard2").asButton;

        // Üretilen tüm görünümler ekran boyutlarına ve görünürlük durumlarına göre kurulur.
        SetupView(_mainView);
        SetupView(_settingsView, false);
        SetupView(_htpView, false);
        SetupView(_detailsView, false);
        SetupView(_generalView, false);

        InitializeUI();

        // Eğer sahnede UI kamerası aktifse çoklu örnekleme kenar yumuşatması açılır.
        if (StageCamera.main != null) StageCamera.main.allowMSAA = true;
    }

    void SetupView(GComponent view, bool isVisible = true)
    {
        // Görünümün boyutu GRoot boyutuna eşitlenir ve ekran boyutu değiştiğinde otomatik boyutlanması için ilişki eklenir.
        view.SetSize(GRoot.inst.width, GRoot.inst.height);
        view.AddRelation(GRoot.inst, RelationType.Size);
        view.visible = isVisible;
        GRoot.inst.AddChild(view);
    }

    void InitializeUI()
    {
        // Ana menüdeki rastgele metin alanı bulunur, rengi sarı yapılır ve ilk metin atanır.
        _splashTextField = _mainView.GetChild("textSplash")?.asTextField;
        if (_splashTextField != null)
        {
            _splashTextField.color = Color.yellow; 
            SetRandomSplashText();
        }

        // Ana menü, nasıl oynanır ve ayarlar panelleri arasındaki buton yönlendirmeleri kurulur.
        SetupNavigationButton(_mainView, "buttonPlay", () => PlayGame());
        SetupNavigationButton(_mainView, "buttonSettings", () => SwitchView(_settingsView));
        SetupNavigationButton(_mainView, "buttonHTP", () => SwitchView(_htpView));
        
        SetupNavigationButton(_htpView, "buttonDetails", () => SwitchView(_detailsView));
        SetupNavigationButton(_htpView, "buttonGeneral", () => SwitchView(_generalView));
        SetupNavigationButton(_htpView, "buttonBack", () => SwitchView(_mainView));
        
        SetupNavigationButton(_settingsView, "buttonBack", () => SwitchView(_mainView));
        SetupNavigationButton(_detailsView, "buttonBack", () => SwitchView(_htpView));
        SetupNavigationButton(_generalView, "buttonBack", () => SwitchView(_htpView));

        // Detaylar sayfasındaki kategori butonlarının hedef kart indeksleri tanımlanır.
        SetupCategoryButton("buttonEnemies", 0);
        SetupCategoryButton("buttonStats", 3);
        SetupCategoryButton("buttonAbilities", 7);
        SetupCategoryButton("buttonLevels", 13);

        // Kart yönlendirme butonlarına basıldığında controller'ın sayfa indeksi artırılır veya azaltılır.
        _btnNext.onClick.Set(() => _cardController.selectedIndex++);
        _btnPrev.onClick.Set(() => _cardController.selectedIndex--);
        
        // Controller'ın sayfası değiştiğinde butonların görünürlüğünü güncelleyecek olay bağlanır.
        _cardController.onChanged.Add(UpdateCardNavigation);
        UpdateCardNavigation();
    }

    void SetupCategoryButton(string btnName, int index)
    {
        // Detaylar panelindeki kategori butonunu bularak doğrudan ilgili sayfa indeksine geçiş olayı atar.
        GButton btn = _detailsView.GetChild(btnName)?.asButton;
        if (btn != null) btn.onClick.Set(() => _cardController.selectedIndex = index);
    }

    void UpdateCardNavigation()
    {
        // Kart kategorilerinin başlangıç ve bitiş indekslerine göre ileri veya geri butonlarının görünürlüğü dinamik olarak kapatılır veya açılır.
        int i = _cardController.selectedIndex;
        _btnPrev.visible = !(i == 0 || i == 3 || i == 7 || i == 13);
        _btnNext.visible = !(i == 2 || i == 6 || i == 12 || i == 15);
    }

    void SwitchView(GComponent targetView)
    {
        // Tüm ana paneller gizlenir ve sadece hedef gösterilen panel görünür hale getirilir.
        _mainView.visible = _settingsView.visible = _htpView.visible = _detailsView.visible = _generalView.visible = false;
        targetView.visible = true;
    }

    void SetRandomSplashText()
    {
        // Veri havuzundan rastgele bir açılış metni seçilir, metin alanına yazılır ve giriş geçiş animasyonu sonsuz döngüde oynatılır.
        _splashTextField.text = SplashData.SplashTexts[Random.Range(0, SplashData.SplashTexts.Length)];
        _mainView.GetTransition("t0")?.Play(-1, 0, null);
    }

    void SetupNavigationButton(GComponent parent, string btnName, UnityEngine.Events.UnityAction action)
    {
        // Ebeveyn panel altındaki butonu bularak onClick olayına ilgili aksiyonu set olarak bağlar.
        parent.GetChild(btnName)?.asButton.onClick.Set(() => action());
    }

    void PlayGame()
    {
        // Oyun sahnesine geçmeden önce mevcut menü arayüz elementleri ve paketleri bellekten tamamen temizlenir.
        GRoot.inst.RemoveChildren(0, -1, true);
        UIPackage.RemoveAllPackages();
        SceneManager.LoadScene("GameScene");
    }
}