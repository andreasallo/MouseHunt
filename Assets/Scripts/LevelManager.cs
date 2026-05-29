using UnityEngine;
using UnityEngine.SceneManagement;

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

    public LevelData CurrentLevel => levelData;

    private void Start()
    {
        if (mouseSpawner != null)
        {
            mouseSpawner.ApplyLevelSettings(levelData);
        }
        else
        {
            Debug.LogWarning("LevelManager: MouseSpawner is not assigned.");
        }

        Debug.Log("Loaded level: " + levelData.levelName);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}