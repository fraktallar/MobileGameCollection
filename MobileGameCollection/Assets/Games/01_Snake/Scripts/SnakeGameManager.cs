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
    DrawBorders(); // ← ekle
}

void DrawBorders()
{
    // Kamera boyutuna göre dinamik hesapla
    Camera cam = Camera.main;
    float camH = cam.orthographicSize;           // 11
    float camW = camH * cam.aspect;              // ekran oranına göre (örn. 16/9 = ~19.5)

    // Oyun alanı sınırları
    float top    =  10.5f;
    float bottom = -10.5f;
    float right  =  10.5f;
    float left   = -10.5f;

    // Üst ve alt — tam genişlikte
    CreateBorderLine(new Vector3(0, top, 0),    new Vector3(camW * 2, 0.15f, 1));
    CreateBorderLine(new Vector3(0, bottom, 0), new Vector3(camW * 2, 0.15f, 1));

    // Sol ve sağ — tam yükseklikte
    CreateBorderLine(new Vector3(right, 0, 0),  new Vector3(0.15f, camH * 2, 1));
    CreateBorderLine(new Vector3(left, 0, 0),   new Vector3(0.15f, camH * 2, 1));
}

void CreateBorderLine(Vector3 position, Vector3 scale)
{
    GameObject line = new GameObject("Border");
    line.transform.position = position;
    line.transform.localScale = scale;

    SpriteRenderer sr = line.AddComponent<SpriteRenderer>();
    Texture2D tex = new Texture2D(1, 1);
    tex.SetPixel(0, 0, Color.white);
    tex.Apply();
    sr.sprite = Sprite.Create(tex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), 1f);
    sr.color = new Color(1f, 1f, 1f, 0.3f); // yarı şeffaf beyaz
    sr.sortingOrder = 0;
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

        // Yem yenince SnakeHead'den çağrılır
        public void IncreaseSpeed()
    {
    moveInterval = Mathf.Max(0.08f, moveInterval - 0.005f);
    }
}