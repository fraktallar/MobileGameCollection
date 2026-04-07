using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SnakeGameManager : MonoBehaviour
{
    public static SnakeGameManager Instance;

    public float moveInterval = 0.2f;

    private int score = 0;
    private int highScore = 0;
    private bool isGameOver = false;

    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI highScoreText;
    private GameObject gameOverPanel;
    private TextMeshProUGUI finalScoreText;

    void Awake()
    {
        Instance = this;
        highScore = PlayerPrefs.GetInt("Snake_HighScore", 0);
        Camera.main.orthographicSize = 11f;
    }

    void Start()
    {
        CreateUI();
        UpdateUI();
        DrawBorders();
        SpawnGameObjects();
    }

    void SpawnGameObjects()
    {
        GameObject headGO = new GameObject("SnakeHead");
        headGO.AddComponent<SnakeHead>();

        GameObject spawnerGO = new GameObject("FoodSpawner");
        spawnerGO.AddComponent<FoodSpawner>();
    }

    void CreateUI()
    {
        // Canvas
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Skor (sol üst)
        scoreText = CreateLabel(canvasGO, "ScoreText", "Skor: 0",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(220, 65), new Vector2(16, -16));
        scoreText.fontSize = 34;
        scoreText.fontStyle = FontStyles.Bold;

        // En iyi (sağ üst)
        highScoreText = CreateLabel(canvasGO, "HighScoreText", "En Iyi: 0",
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(220, 65), new Vector2(-16, -16));
        highScoreText.fontSize = 34;
        highScoreText.fontStyle = FontStyles.Bold;
        highScoreText.alignment = TextAlignmentOptions.TopRight;

        // Game Over paneli (orta, gizli)
        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasGO.transform, false);
        RectTransform panelRect = gameOverPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(400, 300);
        panelRect.anchoredPosition = Vector2.zero;
        Image panelImg = gameOverPanel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.8f);

        // "GAME OVER" başlık
        TextMeshProUGUI title = CreateLabel(gameOverPanel, "Title", "GAME OVER",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(380, 70), new Vector2(0, -20));
        title.fontSize = 40;
        title.alignment = TextAlignmentOptions.Center;
        title.color = new Color(1f, 0.3f, 0.3f);

        // Final skor
        finalScoreText = CreateLabel(gameOverPanel, "FinalScore", "Skor: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(380, 50), new Vector2(0, 30));
        finalScoreText.fontSize = 28;
        finalScoreText.alignment = TextAlignmentOptions.Center;

        // Yeniden Oyna butonu
        CreateButton(gameOverPanel, "Yeniden Oyna", new Vector2(0, -40), RestartGame);

        // Ana Menu butonu
        CreateButton(gameOverPanel, "Ana Menu", new Vector2(0, -110), GoToMainMenu);

        gameOverPanel.SetActive(false);
    }

    TextMeshProUGUI CreateLabel(GameObject parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 offset)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMin;  // pivot anchor ile aynı → kenar hizalama doğru çalışır
        rt.sizeDelta = size;
        rt.anchoredPosition = offset;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 22;
        tmp.color = Color.white;
        return tmp;
    }

    void CreateButton(GameObject parent, string label, Vector2 position,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(label + "Btn");
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(240, 55);
        rt.anchoredPosition = position;
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 0.2f);
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        RectTransform trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    void DrawBorders()
    {
        Camera cam = Camera.main;
        float camH = cam.orthographicSize;
        float camW = camH * cam.aspect;

        float top    =  camH - 0.5f;
        float bottom = -(camH - 0.5f);
        float right  =  camW - 0.5f;
        float left   = -(camW - 0.5f);

        CreateBorderLine(new Vector3(0, top, 0),    new Vector3(camW * 2, 0.15f, 1));
        CreateBorderLine(new Vector3(0, bottom, 0), new Vector3(camW * 2, 0.15f, 1));
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
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        sr.color = new Color(1f, 1f, 1f, 0.3f);
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
        GameAudio.PlayGameOver();
        CameraShake.Shake(0.4f, 0.2f);
        gameOverPanel.SetActive(true);
        finalScoreText.text = "Skor: " + score;
    }

    void UpdateUI()
    {
        scoreText.text = "Skor: " + score;
        highScoreText.text = "En Iyi: " + highScore;
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

    public void IncreaseSpeed()
    {
        moveInterval = Mathf.Max(0.08f, moveInterval - 0.005f);
    }
}
