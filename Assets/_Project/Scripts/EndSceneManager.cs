using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndSceneManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text scoreNumText;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    void Start()
    {
        Time.timeScale = 1f;

        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);

        if (scoreNumText != null)
        {
            scoreNumText.text = finalScore.ToString();
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
