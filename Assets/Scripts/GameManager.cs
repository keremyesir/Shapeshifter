using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Menuleri")]
    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject gameOverCanvas;

    private bool isPaused = false;
    private bool isInMainMenu = true;
    private bool isGameOver = false;

    public bool IsInMainMenu => isInMainMenu;
    public bool IsGameOver => isGameOver;

    // Restart atildiginda sahnenin direkt baslamasi icin static durum degiskeni
    private static bool playImmediatelyOnLoad = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (playImmediatelyOnLoad)
        {
            playImmediatelyOnLoad = false;
            PlayGame();
        }
        else
        {
            ShowMainMenu();
        }
    }

    private void Update()
    {
        // Game over durumunda sol tiklanirsa ana menuye don
        if (isGameOver)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isGameOver = false;
                LoadMainMenu();
            }
            return;
        }

        // ESC tusu ile oyunu durdurma
        if (Input.GetKeyDown(KeyCode.Escape) && !isInMainMenu)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ShowMainMenu()
    {
        isInMainMenu = true;
        isPaused = false;
        isGameOver = false;
        Time.timeScale = 0f;

        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (pauseMenuCanvas != null) pauseMenuCanvas.SetActive(false);
        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);

        // Ana menude BGM durdur
        if (AudioManager.Instance != null) AudioManager.Instance.StopBGM();
    }

    public void StartGameplay()
    {
        isInMainMenu = false;
        isPaused = false;
        isGameOver = false;
        Time.timeScale = 1f;

        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (pauseMenuCanvas != null) pauseMenuCanvas.SetActive(false);
        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);

        // Oyun baslayinca BGM baslat
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBGM();
    }

    // Oyuncu oldugunde cagrilir
    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;

        if (gameOverCanvas != null) gameOverCanvas.SetActive(true);

        // BGM durdur ve kaybetme sesi cal
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
            AudioManager.Instance.PlayLose();
        }
        Debug.Log("GAME OVER! Ana menuye donmek icin ekrana sol tiklayin.");
    }

    // --- BUTON FONKSIYONLARI ---

    public void PlayGame()
    {
        isInMainMenu = false;
        isGameOver = false;
        if (SceneManager.GetActiveScene().name != "SampleScene")
        {
            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            StartGameplay();
        }
    }

    public void PauseGame()
    {
        if (isInMainMenu || isGameOver) return;

        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuCanvas != null) pauseMenuCanvas.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuCanvas != null) pauseMenuCanvas.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        playImmediatelyOnLoad = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan cikiliyor...");
        Application.Quit();
    }
}
