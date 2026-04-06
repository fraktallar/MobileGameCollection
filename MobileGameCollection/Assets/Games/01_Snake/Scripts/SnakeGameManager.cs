using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SnakeGameManager : MonoBehaviour
{
    public static SnakeGameManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;

    [Header("Settings")]
    public float moveInterval = 0.2f;

    private int score = 0;
    private int highScore = 0;
    private bool isGameOver = false;

    void Awake()
    {
        Instance = this;
        highScore = PlayerPrefs.GetInt("Snake_HighScore", 0);
    }

    void Start()
    {
        gameOverPanel.SetActive(false);
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("Snake_HighScore", highScore);
        }
        UpdateUI();
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        gameOverPanel.SetActive(true);
        finalScoreText.text = "Skor: " + score;
    }

    void UpdateUI()
    {
        scoreText.text = "Skor: " + score;
        highScoreText.text = "En İyi: " + highScore;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public float GetMoveInterval() => moveInterval;
}