using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelData
    {
        public string levelName = "Easy";

        [Header("Mouse Settings")]
        public float mouseSpeed = 2f;
        public int startingMice = 1;
        public int maxMice = 1;
        public float spawnInterval = 10f;

        [Header("Game Settings")]
        public int capturesToWin = 3;
        public float levelTime = 60f;

        [Header("Line Settings")]
        public float lineMaxLength = 2.5f;
        public float lineLifetime = 6f;
    }

    [Header("Current Level Settings")]
    [SerializeField] private LevelData levelData = new LevelData();

    [Header("References")]
    [SerializeField] private MouseSpawner mouseSpawner;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;

    [Header("Capture Animation")]
    [SerializeField] private FloatingText floatingTextPrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Vector3 floatingTextWorldOffset = new Vector3(0f, 1.2f, 0f);

    [Header("End Scenes")]
    [SerializeField] private string winSceneName = "WinScene";
    [SerializeField] private string loseSceneName = "LoseScene";

    private int capturedMice = 0;
    private float timeRemaining;
    private bool gameEnded = false;

    public LevelData CurrentLevel => levelData;
    public int CapturedMice => capturedMice;
    public float TimeRemaining => timeRemaining;

    private void Start()
    {
        Time.timeScale = 1f;

        timeRemaining = levelData.levelTime;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mouseSpawner != null)
        {
            mouseSpawner.ApplyLevelSettings(levelData);
        }
        else
        {
            Debug.LogWarning("LevelManager: MouseSpawner is not assigned.");
        }

        UpdateScoreUI();
        UpdateTimerUI();

        Debug.Log("Loaded level: " + levelData.levelName);
        Debug.Log("Goal: Capture " + levelData.capturesToWin + " mice.");
        Debug.Log("Time limit: " + levelData.levelTime + " seconds.");
    }

    private void Update()
    {
        if (gameEnded)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            UpdateTimerUI();
            LoseGame();
            return;
        }

        UpdateTimerUI();
    }

    public void RegisterMouseCaptured(Vector3 mouseWorldPosition)
    {
        if (gameEnded)
        {
            return;
        }

        capturedMice++;

        Debug.Log("Mouse captured: " + capturedMice + " / " + levelData.capturesToWin);

        UpdateScoreUI();
        ShowCaptureAnimation(mouseWorldPosition);

        if (capturedMice >= levelData.capturesToWin)
        {
            WinGame();
        }
    }

    public void RegisterMouseCaptured()
    {
        RegisterMouseCaptured(Vector3.zero);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = capturedMice + " / " + levelData.capturesToWin;
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        }
    }

    private void ShowCaptureAnimation(Vector3 mouseWorldPosition)
    {
        if (floatingTextPrefab == null)
        {
            Debug.LogWarning("LevelManager: FloatingText prefab is not assigned.");
            return;
        }

        if (canvas == null)
        {
            Debug.LogWarning("LevelManager: Canvas is not assigned.");
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            Debug.LogWarning("LevelManager: No main camera found.");
            return;
        }

        FloatingText floatingText = Instantiate(floatingTextPrefab, canvas.transform);

        RectTransform floatingRect = floatingText.GetComponent<RectTransform>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        if (floatingRect == null || canvasRect == null)
        {
            return;
        }

        Vector3 worldPosition = mouseWorldPosition + floatingTextWorldOffset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        Vector2 canvasPosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out canvasPosition
        );

        floatingRect.anchoredPosition = canvasPosition;

        floatingText.SetText("+1");
    }

    private void WinGame()
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;
        PlayerPrefs.SetInt("FinalScore", capturedMice);
        PlayerPrefs.Save();
        Debug.Log("You win!");

        Time.timeScale = 1f;
        SceneManager.LoadScene(winSceneName);
    }

    private void LoseGame()
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;
        PlayerPrefs.SetInt("FinalScore", capturedMice);
        PlayerPrefs.Save();
        Debug.Log("Time ran out. You lose.");

        Time.timeScale = 1f;
        SceneManager.LoadScene(loseSceneName);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}