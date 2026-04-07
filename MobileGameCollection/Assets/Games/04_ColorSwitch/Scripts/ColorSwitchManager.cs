using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ColorSwitchManager : MonoBehaviour
{
    public static ColorSwitchManager Instance;

    // Oyun renkleri — 4 sabit renk
    public static Color[] GameColors = new Color[]
    {
        new Color(0.98f, 0.27f, 0.35f), // kırmızı
        new Color(0.18f, 0.80f, 0.44f), // yeşil
        new Color(0.20f, 0.60f, 1.00f), // mavi
        new Color(1.00f, 0.80f, 0.10f), // sarı
    };

    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI highScoreText;
    private TextMeshProUGUI startText;
    private GameObject gameOverPanel;
    private TextMeshProUGUI finalScoreText;

    private int score = 0;
    private int highScore = 0;
    private bool gameStarted = false;
    private bool gameEnded   = false;

    // Çember spawn
    private float spawnY       = 6f;   // kameranın üstünde doğar
    private float spawnInterval = 4f;  // başlangıç aralığı
    private float timer = 0f;
    private float cameraFollowY = 0f;  // kamera takip Y

    void Awake() => Instance = this;

    void Start()
    {
        highScore = PlayerPrefs.GetInt("ColorSwitch_High", 0);
        BuildUI();
        SpawnBall();
        // İlk çemberi hemen oluştur
        SpawnWheel(2.5f);
    }

    void Update()
    {
        if (!gameStarted || gameEnded) return;

        // Kamera topu yukarı takip et
        GameObject ball = GameObject.FindWithTag("Ball");
        if (ball != null)
        {
            float targetY = Mathf.Max(cameraFollowY,
                                      ball.transform.position.y - 1f);
            cameraFollowY = targetY;
            Camera.main.transform.position = new Vector3(
                0, cameraFollowY, -10);

            // spawn Y'yi kamera ile yukarı taşı
            spawnY = cameraFollowY + 10f;
        }

        // Çember spawn
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnWheel(spawnY);
            // Giderek daha sık spawn olsun
            spawnInterval = Mathf.Max(2f, spawnInterval - 0.05f);
        }
    }

    void SpawnBall()
    {
        GameObject ball = new GameObject("Ball");
        ball.tag = "Ball";
        ball.transform.position = new Vector3(0, -5f, 0);
        ball.AddComponent<ColorSwitchBall>();
    }

    void SpawnWheel(float yPos)
    {
        GameObject wheel = new GameObject("ColorWheel");
        wheel.transform.position = new Vector3(0, yPos, 0);
        wheel.AddComponent<ColorWheel>();
    }

    public void StartGame()
    {
        if (gameStarted) return;
        gameStarted = true;
        if (startText != null)
            startText.gameObject.SetActive(false);
    }

    public void AddScore()
    {
        score++;
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("ColorSwitch_High", highScore);
        }
        scoreText.text     = score.ToString();
        highScoreText.text = "En İyi: " + highScore;
    }

    public void GameOver()
    {
        if (gameEnded) return;
        gameEnded = true;
        finalScoreText.text = "Skor: " + score +
                              "\nEn İyi: " + highScore;
        gameOverPanel.SetActive(true);
    }

    public bool IsStarted() => gameStarted;
    public bool IsEnded()   => gameEnded;

    // ── UI ──────────────────────────────────────────────
    void BuildUI()
    {
        GameObject cGO = new GameObject("Canvas");
        Canvas canvas  = cGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler cs = cGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(720, 1280);
        cs.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // Skor — üst orta
        scoreText = MakeText(cGO, "Score", "0",
            new Vector2(0.5f,1), new Vector2(0.5f,1),
            new Vector2(0,-80), new Vector2(200,100), 72);

        // En iyi — sağ üst
        highScoreText = MakeText(cGO, "High", "En İyi: " + highScore,
            new Vector2(1,1), new Vector2(1,1),
            new Vector2(-80,-50), new Vector2(260,55), 28,
            TextAlignmentOptions.Right);

        // Başlangıç mesajı
        startText = MakeText(cGO, "Start",
            "Başlamak için dokun\nveya SPACE'e bas",
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(0,-60), new Vector2(500,120), 30,
            TextAlignmentOptions.Center,
            new Color(1,1,0.8f));

        // Game Over panel
        gameOverPanel = MakePanel(cGO, new Color(0,0,0,0.9f));

        MakeText(gameOverPanel, "GOT", "GAME OVER",
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(0,220), new Vector2(600,120), 72,
            TextAlignmentOptions.Center,
            new Color(1f,0.3f,0.3f));

        finalScoreText = MakeText(gameOverPanel, "GOS", "",
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(0,60), new Vector2(500,120), 40,
            TextAlignmentOptions.Center);

        MakeButton(gameOverPanel, "Tekrar Oyna",
            new Vector2(0,-100),
            () => SceneManager.LoadScene(
                  SceneManager.GetActiveScene().name));
        MakeButton(gameOverPanel, "Ana Menü",
            new Vector2(0,-210),
            () => SceneManager.LoadScene("MainMenu"));

        gameOverPanel.SetActive(false);
    }

    TextMeshProUGUI MakeText(GameObject parent, string name,
        string text, Vector2 aMin, Vector2 aMax, Vector2 aPos,
        Vector2 size, int fs,
        TextAlignmentOptions align = TextAlignmentOptions.Center,
        Color? col = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.anchoredPosition = aPos; rt.sizeDelta = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fs;
        tmp.alignment = align;
        tmp.color = col ?? Color.white;
        return tmp;
    }

    GameObject MakePanel(GameObject parent, Color bg)
    {
        GameObject go = new GameObject("Panel");
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = bg;
        return go;
    }

    void MakeButton(GameObject parent, string label,
        Vector2 offset, UnityEngine.Events.UnityAction cb)
    {
        GameObject go = new GameObject(label);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f,0.5f);
        rt.anchorMax = new Vector2(0.5f,0.5f);
        rt.anchoredPosition = offset;
        rt.sizeDelta = new Vector2(380, 90);
        go.AddComponent<Image>().color = new Color(0.15f,0.15f,0.4f);
        go.AddComponent<Button>().onClick.AddListener(cb);

        GameObject t = new GameObject("Lbl");
        t.transform.SetParent(go.transform, false);
        RectTransform tr = t.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
        var tmp = t.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 34;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }
}