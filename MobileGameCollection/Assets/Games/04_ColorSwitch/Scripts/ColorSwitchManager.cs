using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ColorSwitchManager : MonoBehaviour
{
    public static ColorSwitchManager Instance;

    public static Color[] GameColors = new Color[]
    {
        new Color(0.98f, 0.27f, 0.35f),
        new Color(0.18f, 0.80f, 0.44f),
        new Color(0.20f, 0.60f, 1.00f),
        new Color(1.00f, 0.80f, 0.10f),
    };

    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI highScoreText;
    private TextMeshProUGUI startText;
    private GameObject gameOverPanel;
    private TextMeshProUGUI finalScoreText;

    private int   score       = 0;
    private int   highScore   = 0;
    private bool  gameStarted = false;
    private bool  gameEnded   = false;

    [Header("Çemberler arası dikey mesafe (dünya birimi)")]
    public float minWheelGap = 6f;
    public float maxWheelGap = 10f;

    [Header("Spawn hızı (saniye — küçük değer = daha sık çember)")]
    [Tooltip("Oyun başladığında iki çember arasındaki ilk bekleme süresi.")]
    public float spawnIntervalStart = 2.5f;
    [Tooltip("Zamanla inilebilecek en kısa süre (en hızlı spawn tavanı).")]
    public float spawnIntervalMin = 1.05f;
    [Tooltip("Her yeni çemberden sonra interval'den düşen miktar; büyük = daha çabuk hızlanır.")]
    public float spawnIntervalDecrease = 0.06f;

    private float lastWheelWorldY = 2.5f;
    private float spawnInterval;
    private float timer         = 0f;
    private float cameraFollowY = 0f;

    private GameObject ballGO;

    void Awake() { Instance = this; Time.timeScale = 1f; }

    void Start()
    {
        PauseManager.OnRestart += Restart;
        highScore = PlayerPrefs.GetInt("ColorSwitch_High", 0);
        BuildUI();
        spawnInterval = Mathf.Max(spawnIntervalMin, spawnIntervalStart);
        SpawnBall();
        lastWheelWorldY = 2.5f;
        SpawnWheel(lastWheelWorldY);
    }

    void OnDestroy() => PauseManager.OnRestart -= Restart;

    void Update()
    {
        if (!gameStarted || gameEnded) return;

        if (ballGO != null)
        {
            float targetY = Mathf.Max(cameraFollowY,
                                      ballGO.transform.position.y - 1f);
            cameraFollowY = targetY;
            Camera.main.transform.position = new Vector3(0, cameraFollowY, -10);
        }

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            float lo = Mathf.Min(minWheelGap, maxWheelGap);
            float hi = Mathf.Max(minWheelGap, maxWheelGap);
            lastWheelWorldY += Random.Range(lo, hi);
            SpawnWheel(lastWheelWorldY);
            float dec = Mathf.Max(0.01f, spawnIntervalDecrease);
            spawnInterval = Mathf.Max(spawnIntervalMin, spawnInterval - dec);
        }
    }

    void SpawnBall()
    {
        ballGO     = new GameObject("Ball");
        ballGO.tag = "Ball";
        ballGO.transform.position = new Vector3(0, -5f, 0);
        ballGO.AddComponent<ColorSwitchBall>();
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
        if (startText != null) startText.gameObject.SetActive(false);
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
        highScoreText.text = "En Iyi: " + highScore;
    }

    public void GameOver()
    {
        if (gameEnded) return;
        gameEnded = true;
        finalScoreText.text = "Skor: " + score + "\nEn Iyi: " + highScore;
        gameOverPanel.SetActive(true);
    }

    public bool IsStarted() => gameStarted;
    public bool IsEnded()   => gameEnded;

    void Restart()
    {
        // Tüm çemberleri temizle
        foreach (var w in FindObjectsOfType<ColorWheel>())
            Destroy(w.gameObject);

        // Topu temizle
        if (ballGO != null) Destroy(ballGO);

        // Kamerayı sıfırla
        cameraFollowY = 0f;
        Camera.main.transform.position = new Vector3(0, 0, -10);

        // State sıfırla
        score         = 0;
        gameStarted   = false;
        gameEnded     = false;
        spawnInterval = Mathf.Max(spawnIntervalMin, spawnIntervalStart);
        timer         = 0f;

        // UI sıfırla
        gameOverPanel.SetActive(false);
        scoreText.text     = "0";
        highScoreText.text = "En Iyi: " + highScore;
        if (startText != null) startText.gameObject.SetActive(true);

        SpawnBall();
        lastWheelWorldY = 2.5f;
        SpawnWheel(lastWheelWorldY);
    }

    // ── UI ──────────────────────────────────────────────
    void BuildUI()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        GameObject cGO = new GameObject("Canvas");
        Canvas canvas  = cGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler cs = cGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(720, 1280);
        cs.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // Skor — pause butonunun altında
        scoreText = MakeText(cGO, "Score", "0",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -160), new Vector2(200, 100), 72);

        // En iyi — sağ üst, pivot düzeltilmiş
        highScoreText = MakeText(cGO, "High", "En Iyi: " + highScore,
            new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-16, -16), new Vector2(240, 55), 26,
            TextAlignmentOptions.Right);
        highScoreText.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f);

        // Başlangıç mesajı
        startText = MakeText(cGO, "Start",
            "Başlamak için dokun\nveya SPACE'e bas",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -60), new Vector2(500, 120), 30,
            TextAlignmentOptions.Center, new Color(1, 1, 0.8f));

        // Game Over panel
        gameOverPanel = MakePanel(cGO, new Color(0, 0, 0, 0.9f));

        MakeText(gameOverPanel, "GOT", "GAME OVER",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 220), new Vector2(600, 120), 72,
            TextAlignmentOptions.Center, new Color(1f, 0.3f, 0.3f));

        finalScoreText = MakeText(gameOverPanel, "GOS", "",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 60), new Vector2(500, 120), 40,
            TextAlignmentOptions.Center);

        MakeButton(gameOverPanel, "Tekrar Oyna", new Vector2(0, -100), Restart);
        MakeButton(gameOverPanel, "Ana Menu",    new Vector2(0, -210),
            () => SceneManager.LoadScene(PauseManager.MainMenuScene));

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
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = offset;
        rt.sizeDelta        = new Vector2(380, 90);
        go.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.4f);
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(cb);

        GameObject t = new GameObject("Lbl");
        t.transform.SetParent(go.transform, false);
        RectTransform tr = t.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
        var tmp = t.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 34;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
    }
}
