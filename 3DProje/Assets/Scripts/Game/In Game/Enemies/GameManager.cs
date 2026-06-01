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
        // Singleton yapısı kurulur ve oyun zaman akışı normal hıza eşitlenir.
        Instance = this;
        Time.timeScale = 1;
    }

    public void PauseGame()
    {
        // Oyun duraklatma durumu aktif edilir ve zaman akışı tamamen durdurulur.
        IsPaused = true;
        Time.timeScale = 0;
        
        // Arayüz yöneticisi üzerinden ilgili duraklatma ekranı paneli açılır.
        if (InGameUIController.Instance != null) InGameUIController.Instance.ShowScreen(1);
    }

    public void ResumeGame()
    {
        // Oyun duraklatma durumu kaldırılır ve zaman akışı normal hızına döndürülür.
        IsPaused = false;
        Time.timeScale = 1;
        
        // Arayüz yöneticisi üzerinden oyun içi ana görünüm paneline geri dönülür.
        if (InGameUIController.Instance != null) InGameUIController.Instance.ShowScreen(0);
    }

    public void GameOver()
    {
        // Oyun bitiş durumu aktif edilir ve arka plandaki tüm zaman akışı durdurulur.
        IsGameOver = true;
        Time.timeScale = 0;

        // Geçen toplam süre alınarak yerel hafızadaki en yüksek skor ile karşılaştırılır.
        int finalTime = spawner.elapsedTime;
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);

        // Eğer yeni bir rekor kırıldıysa bu veri yerel hafızaya kalıcı olarak kaydedilir.
        if (finalTime > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", finalTime);
            PlayerPrefs.Save();
            currentHighScore = finalTime;
        }

        // Oyun bitiş ekranı, mevcut süre ve en yüksek skor verileri formatlanarak arayüze gönderilir.
        if (InGameUIController.Instance != null) InGameUIController.Instance.ShowGameOverScreen(FormatTime(finalTime), FormatTime(currentHighScore));
    }

    // Harici sınıfların o anki süreyi metin formatında almasını sağlar.
    public string GetCurrentTimeText() => FormatTime(spawner.elapsedTime);
    
    // Harici sınıfların kayıtlı en yüksek skoru metin formatında almasını sağlar.
    public string GetBestTimeText() => FormatTime(PlayerPrefs.GetInt("HighScore", 0));

    private string FormatTime(int seconds)
    {
        // Saniye cinsinden gelen süre verisi "Dakika:Saniye" (00:00) formatına dönüştürülür.
        int m = seconds / 60;
        int s = seconds % 60;
        return string.Format("{0:00}:{1:00}", m, s);
    }

    public void RestartGame()
    {
        // Zaman akışı sıfırlanır ve mevcut aktif sahne yeniden yüklenerek oyun baştan başlatılır.
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        // Zaman akışı sıfırlanır ve ana menü sahnesine geçiş yapılır.
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}