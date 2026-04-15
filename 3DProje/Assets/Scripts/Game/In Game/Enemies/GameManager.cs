using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private TMP_Text gameOverTimeText;
    [SerializeField] private TMP_Text gameOverHighscoreText;
    [SerializeField] private TMP_Text hudHighscoreText;
    [SerializeField] private EnemySpawner spawner;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1;
        
        int highscore = PlayerPrefs.GetInt("HighScore", 0);
        if (hudHighscoreText != null) hudHighscoreText.text = "Best: " + FormatTime(highscore);
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        pausePanel.SetActive(true);
        IsPaused=true;
    }

    public void GameOver()
    {
        IsGameOver = true;
        Time.timeScale = 0;
        gameOverPanel.SetActive(true);

        int finalTime = spawner.elapsedTime;
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);

        if (finalTime > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", finalTime);
            PlayerPrefs.Save();
            currentHighScore = finalTime;
        }

        gameOverTimeText.text = "Time: " + FormatTime(finalTime);
        gameOverHighscoreText.text = "Highscore: " + FormatTime(currentHighScore);
    }

    private string FormatTime(int seconds)
    {
        int m = seconds / 60;
        int s = seconds % 60;
        return string.Format("{0:00}:{1:00}", m, s);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        pausePanel.SetActive(false);
        IsPaused=false;
    }
}