using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }

    [SerializeField] private EnemySpawner spawner;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1;
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0;
        if (InGameUIController.Instance != null) InGameUIController.Instance.ShowScreen(1);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1;
        if (InGameUIController.Instance != null) InGameUIController.Instance.ShowScreen(0);
    }

    public void GameOver()
    {
        IsGameOver = true;
        Time.timeScale = 0;

        int finalTime = spawner.elapsedTime;
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);

        if (finalTime > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", finalTime);
            PlayerPrefs.Save();
            currentHighScore = finalTime;
        }

        if (InGameUIController.Instance != null) InGameUIController.Instance.ShowGameOverScreen(FormatTime(finalTime), FormatTime(currentHighScore));
    }

    public string GetCurrentTimeText() => FormatTime(spawner.elapsedTime);
    public string GetBestTimeText() => FormatTime(PlayerPrefs.GetInt("HighScore", 0));

    private string FormatTime(int seconds)
    {
        int m = seconds / 60;
        int s = seconds % 60;
        return string.Format("{0:00}:{1:00}", m, s);
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}