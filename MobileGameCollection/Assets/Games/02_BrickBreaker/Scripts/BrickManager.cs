using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class BrickManager : MonoBehaviour
{
    public static BrickManager Instance;

    // UI — koddan oluşturulacak
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI livesText;
    private GameObject gameOverPanel;
    private GameObject winPanel;
    private TextMeshProUGUI finalScoreText;

    [Header("Grid Ayarları")]
    public int columns = 8;
    public int rows    = 5;

    private int score = 0;
    private int lives = 3;
    private int totalBricks;
    private int brokenBricks = 0;
    private bool gameEnded = false;

    private Color[] rowColors = new Color[]
    {
        new Color(1f,   0.3f, 0.3f),
        new Color(1f,   0.6f, 0.2f),
        new Color(1f,   0.9f, 0.2f),
        new Color(0.3f, 0.9f, 0.4f),
        new Color(0.3f, 0.6f, 1f),
    };

    void Awake() => Instance = this;

    void Start()
    {
        BuildUI();
        SpawnBricks();
        SpawnPaddle();
        SpawnBall();
        UpdateUI();
    }

    // ── UI'ı tamamen koddan kur ──────────────────────────
    void BuildUI()
{
    // Canvas
    GameObject canvasGO = new GameObject("Canvas");
    Canvas canvas = canvasGO.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = new Vector2(720, 1280);
    scaler.matchWidthOrHeight = 0.5f;
    canvasGO.AddComponent<GraphicRaycaster>();

    // ── Skor — sol üst, içeride ──
    scoreText = CreateText(canvasGO, "ScoreText", "Skor: 0",
        new Vector2(0, 1), new Vector2(0, 1),
        new Vector2(120, -40), new Vector2(200, 50), 28,
        TextAlignmentOptions.Left);

    // ── Can — sağ üst, içeride ──
    livesText = CreateText(canvasGO, "LivesText", "Can: 3",
        new Vector2(1, 1), new Vector2(1, 1),
        new Vector2(-120, -40), new Vector2(200, 50), 28,
        TextAlignmentOptions.Right);

    // ── Game Over Paneli ──
    gameOverPanel = CreateFullscreenPanel(canvasGO, "GameOverPanel",
        new Color(0f, 0f, 0f, 0.92f));

    CreateText(gameOverPanel, "GOTitle", "GAME OVER",
        new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
        new Vector2(0, 200), new Vector2(600, 120), 72,
        TextAlignmentOptions.Center, new Color(1f, 0.3f, 0.3f));

    finalScoreText = CreateText(gameOverPanel, "GOScore", "Skor: 0",
        new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
        new Vector2(0, 80), new Vector2(500, 80), 48,
        TextAlignmentOptions.Center);

    CreateButton(gameOverPanel, "Tekrar Oyna",
        new Vector2(0.5f, 0.5f), new Vector2(0, -60), () => Restart());
    CreateButton(gameOverPanel, "Ana Menü",
        new Vector2(0.5f, 0.5f), new Vector2(0, -180), () => GoToMainMenu());

    gameOverPanel.SetActive(false);

    // ── Win Paneli ──
    winPanel = CreateFullscreenPanel(canvasGO, "WinPanel",
        new Color(0f, 0.05f, 0f, 0.92f));

    CreateText(winPanel, "WinTitle", "TEBRİKLER!",
        new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
        new Vector2(0, 200), new Vector2(600, 120), 72,
        TextAlignmentOptions.Center, new Color(0.3f, 1f, 0.4f));

    CreateText(winPanel, "WinScore", "Skor: 0",
        new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
        new Vector2(0, 80), new Vector2(500, 80), 48,
        TextAlignmentOptions.Center);

    CreateButton(winPanel, "Tekrar Oyna",
        new Vector2(0.5f, 0.5f), new Vector2(0, -60), () => Restart());
    CreateButton(winPanel, "Ana Menü",
        new Vector2(0.5f, 0.5f), new Vector2(0, -180), () => GoToMainMenu());

    winPanel.SetActive(false);
}

// Tam ekranı kaplayan panel
GameObject CreateFullscreenPanel(GameObject parent, string name, Color bgColor)
{
    GameObject go = new GameObject(name);
    go.transform.SetParent(parent.transform, false);
    RectTransform rt = go.AddComponent<RectTransform>();
    rt.anchorMin = Vector2.zero;
    rt.anchorMax = Vector2.one;
    rt.offsetMin = Vector2.zero;
    rt.offsetMax = Vector2.zero;
    Image img = go.AddComponent<Image>();
    img.color = bgColor;
    return go;
}

// Butonu anchor + offset ile konumlandır
void CreateButton(GameObject parent, string label,
    Vector2 anchor, Vector2 offset,
    UnityEngine.Events.UnityAction onClick)
{
    GameObject go = new GameObject(label + "Btn");
    go.transform.SetParent(parent.transform, false);
    RectTransform rt = go.AddComponent<RectTransform>();
    rt.anchorMin = anchor;
    rt.anchorMax = anchor;
    rt.anchoredPosition = offset;
    rt.sizeDelta = new Vector2(380, 90); // ← daha büyük

    Image img = go.AddComponent<Image>();
    img.color = new Color(0.15f, 0.15f, 0.4f, 1f);
    Button btn = go.AddComponent<Button>();
    ColorBlock cb = btn.colors;
    cb.highlightedColor = new Color(0.3f, 0.3f, 0.6f);
    cb.pressedColor     = new Color(0.1f, 0.1f, 0.25f);
    btn.colors = cb;
    btn.onClick.AddListener(onClick);

    GameObject txt = new GameObject("Label");
    txt.transform.SetParent(go.transform, false);
    RectTransform trt = txt.AddComponent<RectTransform>();
    trt.anchorMin = Vector2.zero;
    trt.anchorMax = Vector2.one;
    trt.offsetMin = Vector2.zero;
    trt.offsetMax = Vector2.zero;
    TextMeshProUGUI tmp = txt.AddComponent<TextMeshProUGUI>();
    tmp.text      = label;
    tmp.fontSize  = 34; // ← daha büyük
    tmp.alignment = TextAlignmentOptions.Center;
    tmp.color     = Color.white;
}

    // ── Yardımcı UI oluşturucular ────────────────────────
    TextMeshProUGUI CreateText(GameObject parent, string name, string text,
    Vector2 anchorMin, Vector2 anchorMax,
    Vector2 anchoredPos, Vector2 size, int fontSize,
    TextAlignmentOptions align = TextAlignmentOptions.Left,
    Color? color = null)
{
    GameObject go = new GameObject(name);
    go.transform.SetParent(parent.transform, false);
    RectTransform rt = go.AddComponent<RectTransform>();
    rt.anchorMin      = anchorMin;
    rt.anchorMax      = anchorMax;
    rt.anchoredPosition = anchoredPos;
    rt.sizeDelta      = size;
    TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
    tmp.text      = text;
    tmp.fontSize  = fontSize;
    tmp.alignment = align;
    tmp.color     = color ?? Color.white;
    return tmp;
}

    GameObject CreatePanel(GameObject parent, string name, Color bgColor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image img = go.AddComponent<Image>();
        img.color = bgColor;
        return go;
    }

    void CreateButton(GameObject parent, string label,
        Vector2 anchor, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(label + "Btn");
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(220, 50);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.4f, 1f);
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

        // Buton yazısı
        GameObject txt = new GameObject("Label");
        txt.transform.SetParent(go.transform, false);
        RectTransform trt = txt.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = txt.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    // ── Oyun nesneleri ───────────────────────────────────
    void SpawnBricks()
    {
        Camera cam = Camera.main;
        float camW = cam.orthographicSize * cam.aspect;
        float brickW  = (camW * 2f - 1f) / columns;
        float brickH  = 0.55f;
        float padding = 0.08f;
        float startX  = -camW + brickW / 2f + 0.5f;
        float startY  = cam.orthographicSize - 1.5f;

        totalBricks = columns * rows;

        for (int r = 0; r < rows; r++)
        {
            Color brickColor = rowColors[r % rowColors.Length];
            int points = (rows - r) * 10;

            for (int c = 0; c < columns; c++)
            {
                float x = startX + c * brickW;
                float y = startY - r * (brickH + padding);

                GameObject brick = new GameObject("Brick");
                brick.transform.position = new Vector3(x, y, 0);
                brick.transform.localScale =
                    new Vector3(brickW - padding, brickH, 1f);
                brick.tag = "Brick";

                SpriteRenderer sr = brick.AddComponent<SpriteRenderer>();
                sr.sprite = CreateSprite();
                sr.color  = brickColor;
                sr.sortingOrder = 1;

                brick.AddComponent<BoxCollider2D>();
                BrickData bd = brick.AddComponent<BrickData>();
                bd.pointValue = points;
            }
        }
    }

    void SpawnPaddle()
    {
        GameObject paddle = new GameObject("Paddle");
        paddle.tag = "Paddle";
        paddle.AddComponent<PaddleController>();
    }

    void SpawnBall()
    {
        GameObject ball = new GameObject("Ball");
        ball.AddComponent<BallController>();
    }

    // ── Oyun olayları ────────────────────────────────────
    public void BrickDestroyed(int points)
    {
        if (gameEnded) return;
        score += points;
        brokenBricks++;
        UpdateUI();
        if (brokenBricks >= totalBricks) Win();
    }

    public void GameOver()
    {
        if (gameEnded) return;
        gameEnded = true;
        finalScoreText.text = "Skor: " + score;
        gameOverPanel.SetActive(true);
    }

    void Win()
    {
        if (gameEnded) return;
        gameEnded = true;
        winPanel.SetActive(true);
    }

    void UpdateUI()
    {
        scoreText.text = "Skor: " + score;
        livesText.text = "Can: " + lives;
    }

    public void LoseLife()
    {
        if (gameEnded) return;
        lives--;
        UpdateUI();
        if (lives <= 0) GameOver();
        else SpawnBall();
    }

    void Restart() =>
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    void GoToMainMenu() =>
        SceneManager.LoadScene("MainMenu");

    Sprite CreateSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,1,1),
                             new Vector2(0.5f,0.5f), 1f);
    }
}