using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelData
    {
        public string levelName;
        public GameObject levelRoot;

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

    [Header("Levels")]
    [SerializeField] private LevelData[] levels;

    [Header("References")]
    [SerializeField] private MouseSpawner mouseSpawner;

    [Header("Start Settings")]
    [SerializeField] private bool loadLevelOnStart = false;
    [SerializeField] private int startLevelIndex = 0;

    public int CurrentLevelIndex { get; private set; }
    public LevelData CurrentLevel => levels[CurrentLevelIndex];

    private void Start()
    {
        DeactivateAllLevels();
        LoadLevel(GameSettings.selectedLevel);


    }

    private void Update()
    {
        // Temporary testing keys.
        // You can remove this later when the menu buttons work.
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            LoadTutorial();
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            LoadNormal();
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            LoadHard();
        }
    }

    public void LoadTutorial()
    {
        LoadLevel(0);
    }

    public void LoadNormal()
    {
        LoadLevel(1);
    }

    public void LoadHard()
    {
        LoadLevel(2);
    }

    public void LoadLevel(int levelIndex)
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("LevelManager: No levels assigned.");
            return;
        }

        if (levelIndex < 0 || levelIndex >= levels.Length)
        {
            Debug.LogError("LevelManager: Invalid level index: " + levelIndex);
            return;
        }

        CurrentLevelIndex = levelIndex;

        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i].levelRoot != null)
            {
                levels[i].levelRoot.SetActive(i == levelIndex);
            }
        }

        if (mouseSpawner != null)
        {
            mouseSpawner.ApplyLevelSettings(CurrentLevel);
        }
        else
        {
            Debug.LogWarning("LevelManager: MouseSpawner is not assigned.");
        }

        Debug.Log("Loaded level: " + CurrentLevel.levelName);
    }

    public void LoadNextLevel()
    {
        int nextLevelIndex = CurrentLevelIndex + 1;

        if (nextLevelIndex >= levels.Length)
        {
            Debug.Log("All levels completed!");
            return;
        }

        LoadLevel(nextLevelIndex);
    }

    public void ReturnToMenu()
    {
        DeactivateAllLevels();

        if (mouseSpawner != null)
        {
            mouseSpawner.ClearAllMice();
        }
    }

    private void DeactivateAllLevels()
    {
        if (levels == null)
        {
            return;
        }

        foreach (LevelData level in levels)
        {
            if (level.levelRoot != null)
            {
                level.levelRoot.SetActive(false);
            }
        }
    }
}