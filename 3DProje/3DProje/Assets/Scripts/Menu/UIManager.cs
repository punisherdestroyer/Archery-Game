using FairyGUI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private GComponent _mainView;
    private GTextField _splashTextField;
    private string[] splashTexts = {
        "Error 404", "Splash Text 0", "Splash Text Not Found:D", "Archery, Mastery", "This Is Fine", "The Floor Is Lava",
        "It's Too Dangerous To Go Alone", "Hurry Up!", "New Record Waiting...", "Cap?", "MC Energy!", "Chipi Chipi Chapa Chapa!",
        "One Shot", "Rage Mode: ON", "The Cake Is A Lie", "Say Hello To The Bad Cop", "Emotional Damage", "Boy!", "Coffe In, Code Out",
        "Also Try Terraria", "Also Try Minecraft", "Also Try Archero", "Praise The Sun", "It's Hero Time!", "It's The Lag, I Swear",
        "Finish Him!", "War Never Changes", "Fus Ro Dah!", "Rush B!", "Gotta Go Fast", "Change My Mind", "GG WP", "EZ", "GL HF",
        "Prepare To Die", "YOU DIED", "It Works On My Machine!", "Hello, World!", "No Cap.", "Modern Problems Requires Archery",
        "Bug? No, It's A Feature", "To Be Or Not To Be", "Optimization Is A Lie", "He's A 10 But He Misses Shots", "C# Is Better Than Java",
        "Bullseye!", "Arrow To The Knee", "Split The Arrow", "Just One More Run...", "That's What She Said.", "Spoiler: You Might Die.",
        "Play, Die, Repeat...", "Bless The RNG!", "Roguelike Or Roguelite?", "Stonks!", "Nanomachines, son!", "Designed In İzmir",
        "Cricital Hit!", "To Infinity And Beyond!", "You Shall Not Pass!", "Loot Time!", "Snake? Snake?! SNAAAKE!!!", "Wind: 99 - Player: 0.",
        "Houston, We Have A Problem", "Why So Serious?", "Hakuna Matata!", "Skill Issue?", "Do You Even Aim Bro?", "RNG Gives, RNG takes.",
        "Just Do It", "NANI?!", "Is This A JoJo Reference?", "Rickroll Incoming?", "Born To Aim, Forced The Code", "Hit The Apple, Not The Head!",
        "Your Princesses Is In Another Castle", "Press F", "Reality Is A Simulation", "Are You Really Read This? Wow.", "Wasted!",
        "You're Finally Awake", "Why Mr. Andreson?", "Do A Barrel Roll!", "Git Gud.", "The End... Or Is It?", "ben.nezu Solos!",
        "The Arrow Knows The Way", "Heart Of The Marksman", "Gravity Is Just A Suggetion", "My Code Works, I Don't Know Why.", "Meow Or Not?",
        "RNGesus, Hear My Prayer!", "Try Again?", "Fixed One Bug, Created Five...", "My Code Doesn’t Work, I Don't Know Why.",
        "Brain.exe Stopped Working.", "Low Poly, High Fun!", "Beta Tester's Nightmare", "This Text Is 100% Organic", "Critical Miss",
        "Task Failed Succesfully", "Mission Completed, Respect+", "Insert Coin To Continue",      
    };

    void Awake() {
        GRoot.inst.SetContentScaleFactor(1920, 1080, UIContentScaler.ScreenMatchMode.MatchWidthOrHeight);
        UIConfig.defaultFont = "ARIAL @MainMenu"; 
        Stage.inst.pixelPerfect = true;
    }

    void Start()
    {
        GRoot.inst.RemoveChildren(0, -1, true);
        UIPackage.RemoveAllPackages();

        UIPackage pkg = UIPackage.AddPackage("FairyUI/MainMenu");
        UIPackage.AddPackage("FairyUI/UIAssets");

        if (pkg != null)
        {
            _mainView = UIPackage.CreateObject("MainMenu", "Main Menu Background").asCom;

            if (_mainView != null)
            {
                _mainView.SetSize(GRoot.inst.width, GRoot.inst.height);
                _mainView.AddRelation(GRoot.inst, RelationType.Size);
                GRoot.inst.AddChild(_mainView);

                _splashTextField = _mainView.GetChild("Splash Text").asTextField;
                if (_splashTextField != null)
                {
                    _splashTextField.color = Color.yellow; 
                    SetRandomSplashText();
                }

                SetupButton("Play Button", () => Debug.Log("Oyun Başlıyor!"));
            }
        }

        if (StageCamera.main != null)
        {
            StageCamera.main.allowMSAA = true;
        }
    }

    void SetRandomSplashText()
    {
        string randomText = splashTexts[Random.Range(0, splashTexts.Length)];
        _splashTextField.text = $"{randomText}";

        Transition trans = _mainView.GetTransition("t0");
        if (trans != null)
        {
            trans.Play(-1, 0, null);
        }
    }

    void SetupButton(string name, UnityEngine.Events.UnityAction action)
    {
        if (_mainView == null) return;
        
        GObject obj = _mainView.GetChild(name);
        if (obj != null)
        {
            GButton btn = obj.asButton;

            btn.onClick.Add(() =>
            {
                action();
                if (_mainView != null)
                {
                    _mainView.Dispose();
                }
                SceneManager.LoadScene("GameScene");
            });
        }
    }
}