using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject generalGame;
    [SerializeField] private GameObject abilities;

    void Start()
    {
        settingsPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        generalGame.SetActive(false);
        abilities.SetActive(false);
    }

    public void PlayButton()
    {
        int tutorialSeen = PlayerPrefs.GetInt("TutorialSeen", 0);
        if (tutorialSeen == 0)
        {
            OpenHowToPlay();
            PlayerPrefs.SetInt("TutorialSeen", 1);
            PlayerPrefs.Save();
        }
        else
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void OpenHowToPlay()
    {
        howToPlayPanel.SetActive(true);
    }

    public void CloseHowToPlay()
    {
        howToPlayPanel.SetActive(false);
        int tutorialSeen = PlayerPrefs.GetInt("TutorialSeen", 0);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void SetVolume(float vol)
    {
        AudioListener.volume = vol;
    }

    public void OpenGeneralGame()
    {
        generalGame.SetActive(true);
    }

    public void CloseGeneralGame()
    {
        generalGame.SetActive(false);
    }

    public void OpenAbilities()
    {
        abilities.SetActive(true);
    }

    public void CloseAbilities()
    {
        abilities.SetActive(false);
    }
}