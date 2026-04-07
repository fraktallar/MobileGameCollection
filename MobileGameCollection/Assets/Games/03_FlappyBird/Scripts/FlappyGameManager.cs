using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class FlappyGameManager : MonoBehaviour
{
    public static FlappyGameManager Instance;

    // UI — koddan oluşturulacak
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI highScoreText;
    private TextMeshProUGUI startText;
    private GameObject gameOverPanel;
    private TextMeshProUGUI finalScoreText;

    private int score = 0;
    private int highScore = 0;
    private bool gameStarted = false;
    private bool gameEnded = false;

    void Awake() => Instance = this;

    void Start()
    {
        highScore = PlayerPrefs.GetInt("Flappy_HighScore", 0);
        BuildUI();
        SpawnWorld();
    }

    // ── Dünya nesnelerini oluştur ──
    void SpawnWorld()
{
    Camera.main.backgroundColor = new Color(0.3f, 0.75f, 0.79f);

    // Görsel zemin (sadece görsel, çarpışma Y ile kontrol ediliyor)
    CreateVisualGround();

    // Kuş
    GameObject bird = new GameObject("Bird");
    bird.transform.position = new Vector3(-1.5f, 0, 0);
    bird.AddComponent<BirdController>();

    // Pipe Spawner
    new GameObject("PipeSpawner").AddComponent<PipeSpawner>();
}

void CreateVisualGround()
{
    GameObject ground = new GameObject("Ground");
    ground.transform.position = new Vector3(0, -5f, 0);
    ground.transform.localScale = new Vector3(30f, 1f, 1f);

    SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
    Texture2D tex = new Texture2D(1, 1);
    tex.SetPixel(0, 0, Color.white); tex.Apply();
    sr.sprite = Sprite.Create(tex, new Rect(0,0,1,1),
                              new Vector2(0.5f,0.5f), 1f);
    sr.color  = new Color(0.55f, 0.4f, 0.2f);
    sr.sortingOrder = 2;
}

    void CreateBoundary(string name, Vector3 pos, Vector3 scale,
                        Color? color = null, bool visible = false)
    {
        GameObject go = new GameObject(name);
        go.tag = "Ground";
        go.transform.position = pos;
        go.transform.localScale = scale;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();

        if (visible && color.HasValue)
        {
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSprite();
            sr.color  = color.Value;
            sr.sortingOrder = 2;
        }
    }

    // ── UI ──
    void BuildUI()
    {
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(720, 1280);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Skor — ortada üst
        scoreText = CreateText(canvasGO, "ScoreText", "0",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -80), new Vector2(200, 100), 72,
            TextAlignmentOptions.Center);

        // En iyi — sağ üst
        highScoreText = CreateText(canvasGO, "HighScore",
            "En İyi: " + highScore,
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-80, -50), new Vector2(250, 60), 28,
            TextAlignmentOptions.Right);

        // Başlangıç mesajı — orta
        startText = CreateText(canvasGO, "StartText",
            "Başlamak için\ndokun veya SPACE'e bas",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -80), new Vector2(500, 120), 30,
            TextAlignmentOptions.Center,
            new Color(1f, 1f, 0.8f));

        // Game Over Panel
        gameOverPanel = CreateFullscreenPanel(canvasGO,
            new Color(0f, 0f, 0f, 0.85f));

        CreateText(gameOverPanel, "GOTitle", "GAME OVER",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 220), new Vector2(600, 120), 72,
            TextAlignmentOptions.Center, new Color(1f, 0.3f, 0.3f));

        finalScoreText = CreateText(gameOverPanel, "GOScore", "Skor: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 100), new Vector2(500, 80), 48,
            TextAlignmentOptions.Center);

        CreateText(gameOverPanel, "GOHigh",
            "En İyi: " + highScore,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 20), new Vector2(500, 70), 36,
            TextAlignmentOptions.Center,
            new Color(1f, 0.85f, 0.2f));

        CreateButton(gameOverPanel, "Tekrar Oyna",
            new Vector2(0, -100), () => Restart());
        CreateButton(gameOverPanel, "Ana Menü",
            new Vector2(0, -210), () => GoToMainMenu());

        gameOverPanel.SetActive(false);
    }

    // ── Public metodlar ──
    public bool IsGameStarted() => gameStarted;
    public bool IsGameEnded()   => gameEnded;

    public void StartGame()
    {
        if (gameStarted || gameEnded) return;
        gameStarted = true;
        if (startText != null) startText.gameObject.SetActive(false);
    }

    public void AddScore()
    {
        if (gameEnded) return;
        score++;
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("Flappy_HighScore", highScore);
        }
        scoreText.text = score.ToString();
        highScoreText.text = "En İyi: " + highScore;
    }

    public void GameOver()
    {
        if (gameEnded) return;
        gameEnded = true;
        finalScoreText.text = "Skor: " + score + "\nEn İyi: " + highScore;
        gameOverPanel.SetActive(true);

        // Tüm boruları durdur
        foreach (var pipe in FindObjectsOfType<PipeMover>())
            pipe.Stop();
    }

    void Restart()     => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    void GoToMainMenu()=> SceneManager.LoadScene("MainMenu");

    // ── Yardımcılar ──
    TextMeshProUGUI CreateText(GameObject parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos,
        Vector2 size, int fontSize,
        TextAlignmentOptions align = TextAlignmentOptions.Center,
        Color? color = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos; rt.sizeDelta = size;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = color ?? Color.white;
        return tmp;
    }

    GameObject CreateFullscreenPanel(GameObject parent, Color bgColor)
    {
        GameObject go = new GameObject("GameOverPanel");
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = bgColor;
        return go;
    }

    void CreateButton(GameObject parent, string label,
        Vector2 offset, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(label + "Btn");
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = offset;
        rt.sizeDelta = new Vector2(380, 90);
        go.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.4f);
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

        GameObject txt = new GameObject("Label");
        txt.transform.SetParent(go.transform, false);
        RectTransform trt = txt.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = txt.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 34;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    Sprite CreateSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white); tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,1,1),
                             new Vector2(0.5f,0.5f), 1f);
    }
}