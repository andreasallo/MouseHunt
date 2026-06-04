using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text timerText;

    [Header("Game Settings")]
    public float gameDuration = 60f;

    [Header("Scenes")]
    [SerializeField] private string endSceneName = "EndScene";

    private int score = 0;
    private float timeRemaining;
    private bool gameEnded = false;

    void Start()
    {
        Time.timeScale = 1f;
        timeRemaining = gameDuration;
        UpdateScoreUI();
        UpdateTimerUI();
    }

    void Update()
    {
        if (gameEnded) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            UpdateTimerUI();
            EndGame();
            return;
        }

        UpdateTimerUI();
    }

    public void AddPoint()
    {
        if (gameEnded) return;

        score++;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        scoreText.text = score.ToString();
    }

    void UpdateTimerUI()
    {
        timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
    }

    void EndGame()
    {
        gameEnded = true;

        PlayerPrefs.SetInt("FinalScore", score);
        PlayerPrefs.Save();

        Debug.Log("Game Over! Final score: " + score);

        Time.timeScale = 1f;
        SceneManager.LoadScene(endSceneName);
    }
}