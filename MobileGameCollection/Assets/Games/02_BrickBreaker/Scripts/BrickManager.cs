using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private TextMeshProUGUI winScoreText;

    public int columns = 10;
    [Tooltip("Dinamik ızgarada minimum satır; gerçek satır sayısı ekran ortasına kadar hesaplanır.")]
    public int rows = 8;

    private int score = 0;
    private int lives = 3;
    private int totalBricks;
    private int brokenBricks = 0;
    private bool gameEnded = false;

    // Restart için referanslar
    private readonly List<GameObject> brickObjects = new List<GameObject>();
    private readonly List<GameObject> activeBalls = new List<GameObject>();
    private GameObject paddleGO;

    private float ballSpeedMultiplier = 1f;
    private float slowBallTimer;
    private const float BaseBallSpeed = 12f;

    private Color[] rowColors = new Color[]
    {
        new Color(1f,   0.3f, 0.3f),
        new Color(1f,   0.6f, 0.2f),
        new Color(1f,   0.9f, 0.2f),
        new Color(0.3f, 0.9f, 0.4f),
        new Color(0.3f, 0.6f, 1f),
    };

    void Awake() { Instance = this; Time.timeScale = 1f; }
    void OnDestroy() => PauseManager.OnRestart -= Restart;

    void Start()
    {
        PauseManager.OnRestart += Restart;
        BuildUI();
        CreateSpaceBackground();
        SpawnBricks();
        SpawnPaddle();
        SpawnBall();
        UpdateUI();
    }

    void Update()
    {
        if (slowBallTimer > 0f)
        {
            slowBallTimer -= Time.deltaTime;
            if (slowBallTimer <= 0f)
            {
                slowBallTimer = 0f;
                ballSpeedMultiplier = 1f;
            }
        }
    }

    // ── UI'ı tamamen koddan kur ──────────────────────────
    void BuildUI()
{
    if (FindObjectOfType<EventSystem>() == null)
    {
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

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

    winScoreText = CreateText(winPanel, "WinScore", "Skor: 0",
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
        float padding = 0.06f;
        float brickW = (camW * 2f - 0.6f) / columns;
        float brickH = 0.38f;
        float startX = -camW + brickW / 2f + 0.3f;
        float rowStep = brickH + padding;
        float topMargin = 1.2f;
        float startY = cam.orthographicSize - topMargin + cam.transform.position.y;
        float clipZ = Mathf.Abs(cam.transform.position.z);
        if (clipZ < 0.01f) clipZ = 10f;
        float midlineY = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, clipZ)).y;
        float minCenterBottomRow = midlineY + brickH * 0.5f + 0.05f;
        int brickRows = Mathf.FloorToInt((startY - minCenterBottomRow) / rowStep) + 1;
        brickRows = Mathf.Clamp(brickRows, 4, 48);

        brickObjects.Clear();
        bool[,] cellOn = new bool[brickRows, columns];
        float fill = UnityEngine.Random.Range(0.78f, 0.95f);

        for (int r = 0; r < brickRows; r++)
            for (int c = 0; c < columns; c++)
                if (UnityEngine.Random.value < fill) cellOn[r, c] = true;

        totalBricks = 0;
        for (int r = 0; r < brickRows; r++)
            for (int c = 0; c < columns; c++)
                if (cellOn[r, c]) totalBricks++;

        int minBricks = Mathf.Max(columns * 5, brickRows * columns * 62 / 100);
        while (totalBricks < minBricks)
        {
            int r = Random.Range(0, brickRows);
            int c = Random.Range(0, columns);
            if (cellOn[r, c]) continue;
            cellOn[r, c] = true;
            totalBricks++;
        }

        int powerCap = Mathf.Clamp(totalBricks / 4, 10, 28);

        for (int r = 0; r < brickRows; r++)
        {
            Color brickColor = rowColors[r % rowColors.Length];
            int points = (brickRows - r) * 10;
            int hp = r < 3 ? 2 : 1;

            for (int c = 0; c < columns; c++)
            {
                if (!cellOn[r, c]) continue;

                float x = startX + c * brickW;
                float y = startY - r * rowStep;

                GameObject brick = new GameObject("Brick");
                brick.transform.position = new Vector3(x, y, 0);
                brick.transform.localScale = new Vector3(brickW - padding, brickH, 1f);
                brick.tag = "Brick";

                SpriteRenderer sr = brick.AddComponent<SpriteRenderer>();
                sr.sprite = CreateSprite();
                sr.sortingOrder = 1;

                brick.AddComponent<BoxCollider2D>();
                BrickData bd = brick.AddComponent<BrickData>();
                bd.pointValue = points;

                BrickPowerUp drop = BrickPowerUp.None;
                if (powerCap > 0 && UnityEngine.Random.value < 0.30f)
                {
                    drop = (BrickPowerUp)UnityEngine.Random.Range(1, 4);
                    powerCap--;
                }

                bd.Init(hp, brickColor, drop);
                brickObjects.Add(brick);
            }
        }

        EnsureMinimumPowerUpBricks(brickRows);
    }

    void EnsureMinimumPowerUpBricks(int brickRows)
    {
        int CountPowers()
        {
            int n = 0;
            foreach (GameObject go in brickObjects)
            {
                if (go == null) continue;
                BrickData bd = go.GetComponent<BrickData>();
                if (bd != null && bd.powerUp != BrickPowerUp.None) n++;
            }
            return n;
        }

        int target = Mathf.Clamp(4 + totalBricks / 10, 6, 18);
        if (brickRows < 6) target = Mathf.Min(target, 5);

        var noPow = new List<BrickData>();
        foreach (GameObject go in brickObjects)
        {
            if (go == null) continue;
            BrickData bd = go.GetComponent<BrickData>();
            if (bd != null && bd.powerUp == BrickPowerUp.None) noPow.Add(bd);
        }

        for (int k = CountPowers(); k < target && noPow.Count > 0; k++)
        {
            int i = Random.Range(0, noPow.Count);
            BrickData bd = noPow[i];
            bd.powerUp = (BrickPowerUp)Random.Range(1, 4);
            noPow.RemoveAt(i);
        }
    }

    void SpawnPaddle()
    {
        paddleGO     = new GameObject("Paddle");
        paddleGO.tag = "Paddle";
        paddleGO.AddComponent<PaddleController>();
    }

    void SpawnBall()
    {
        GameObject ball = new GameObject("Ball");
        ball.AddComponent<BallController>();
    }

    public void RegisterBall(GameObject ball)
    {
        if (ball != null && !activeBalls.Contains(ball))
            activeBalls.Add(ball);
    }

    public void NotifyBallLost(GameObject ball)
    {
        activeBalls.Remove(ball);
        if (gameEnded) return;
        if (activeBalls.Count > 0) return;

        GameAudio.PlayGameOver();
        CameraShake.Shake(0.3f, 0.15f);
        lives--;
        UpdateUI();
        if (lives <= 0) GameOver();
        else SpawnBall();
    }

    public float GetBallSpeed() => BaseBallSpeed * ballSpeedMultiplier;

    public Bounds GetPaddleWorldBounds()
    {
        if (paddleGO == null) return new Bounds(Vector3.zero, Vector3.one);
        PaddleController pc = paddleGO.GetComponent<PaddleController>();
        return pc != null ? pc.GetWorldBounds() : new Bounds(paddleGO.transform.position, Vector3.one);
    }

    public void DropPowerUp(Vector3 brickWorldPos, BrickPowerUp type)
    {
        if (type == BrickPowerUp.None || gameEnded) return;
        GameObject go = new GameObject("PowerUp");
        go.transform.position = brickWorldPos + Vector3.down * 0.15f;
        PowerUpPickup p = go.AddComponent<PowerUpPickup>();
        p.Init(type);
    }

    public void ApplyPowerUp(BrickPowerUp type)
    {
        GameAudio.PlayPop();
        switch (type)
        {
            case BrickPowerUp.WidePaddle:
                if (paddleGO != null)
                {
                    PaddleController pc = paddleGO.GetComponent<PaddleController>();
                    if (pc != null) pc.ApplyWidePaddleBuff(14f, 1.7f);
                }
                break;
            case BrickPowerUp.MultiBall:
                SpawnExtraBall();
                break;
            case BrickPowerUp.SlowBall:
                slowBallTimer = 11f;
                ballSpeedMultiplier = 0.68f;
                break;
        }
    }

    void SpawnExtraBall()
    {
        BallController[] balls = FindObjectsOfType<BallController>();
        Vector3 pos;
        Vector2 dir = new Vector2(UnityEngine.Random.Range(-0.38f, 0.38f), 1f).normalized;

        BallController src = null;
        foreach (BallController b in balls)
        {
            if (b != null && b.IsLaunched) { src = b; break; }
        }

        if (src != null)
        {
            pos = src.transform.position + new Vector3(UnityEngine.Random.Range(-0.12f, 0.12f), 0.06f, 0f);
            Vector2 v = src.CurrentVelocity;
            if (v.sqrMagnitude > 0.0001f)
            {
                float ang = UnityEngine.Random.Range(-32f, 32f) * Mathf.Deg2Rad;
                float cos = Mathf.Cos(ang), sin = Mathf.Sin(ang);
                dir = new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos).normalized;
            }
        }
        else if (paddleGO != null)
            pos = paddleGO.transform.position + Vector3.up * 0.55f;
        else
            return;

        GameObject go = new GameObject("Ball");
        BallController nb = go.AddComponent<BallController>();
        nb.ConfigureImmediateLaunch(pos, dir);
    }

    public static readonly Color SpaceDeepColor = new Color(0.03f, 0.02f, 0.11f);
    public static readonly Color SpaceTopColor = new Color(0.07f, 0.03f, 0.2f);

    void CreateSpaceBackground()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = SpaceDeepColor;

        GameObject bg = new GameObject("SpaceBackground");
        bg.transform.position = new Vector3(0f, 0f, 10f);
        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        const int sz = 512;
        sr.sprite = BuildSpaceSprite(sz);
        sr.sortingOrder = -30;

        float targetH = cam.orthographicSize * 2.45f;
        float targetW = targetH * cam.aspect;
        bg.transform.localScale = new Vector3(targetW / sz, targetH / sz, 1f);
    }

    static Sprite BuildSpaceSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;
        Color deep = SpaceDeepColor;
        Color top = SpaceTopColor;
        for (int y = 0; y < size; y++)
        {
            float t = y / (float)size;
            Color row = Color.Lerp(deep, top, t);
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, row);
        }

        var rng = new System.Random(UnityEngine.Random.Range(1, int.MaxValue));
        for (int i = 0; i < 520; i++)
        {
            int x = rng.Next(2, size - 2);
            int y = rng.Next(2, size - 2);
            float s = rng.Next(3) == 0 ? 1f : 0.5f;
            tex.SetPixel(x, y, new Color(0.95f, 0.95f, 1f) * s);
            if (rng.Next(2) == 0)
                tex.SetPixel(x + 1, y, new Color(0.95f, 0.95f, 1f) * (s * 0.6f));
        }

        for (int k = 0; k < 7; k++)
        {
            int cx = rng.Next(size / 5, 4 * size / 5);
            int cy = rng.Next(size / 5, 4 * size / 5);
            int rad = rng.Next(size / 16, size / 4);
            Color neb = new Color(
                rng.Next(2) == 0 ? 0.28f : 0.12f,
                rng.Next(2) == 0 ? 0.1f : 0.22f,
                rng.Next(2) == 0 ? 0.38f : 0.2f,
                0.14f);
            for (int y = Mathf.Max(0, cy - rad); y < Mathf.Min(size, cy + rad); y++)
            {
                for (int x = Mathf.Max(0, cx - rad); x < Mathf.Min(size, cx + rad); x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy)) / rad;
                    if (d < 1f)
                    {
                        Color o = tex.GetPixel(x, y);
                        tex.SetPixel(x, y, Color.Lerp(o, neb, (1f - d) * (1f - d) * 0.55f));
                    }
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 1f);
    }

    // ── Oyun olayları ────────────────────────────────────
    public void BrickDestroyed(int points)
    {
        GameAudio.PlayPop();
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
        if (winScoreText != null) winScoreText.text = "Skor: " + score;
        winPanel.SetActive(true);
    }

    void UpdateUI()
    {
        scoreText.text = "Skor: " + score;
        livesText.text = "Can: " + lives;
    }

    void Restart()
    {
        foreach (var b in brickObjects) if (b != null) Destroy(b);
        brickObjects.Clear();
        foreach (var ball in activeBalls) if (ball != null) Destroy(ball);
        activeBalls.Clear();
        foreach (var p in FindObjectsOfType<PowerUpPickup>())
            if (p != null) Destroy(p.gameObject);
        if (paddleGO != null) Destroy(paddleGO);

        score = 0;
        lives = 3;
        brokenBricks = 0;
        gameEnded = false;
        ballSpeedMultiplier = 1f;
        slowBallTimer = 0f;
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);

        SpawnBricks();
        SpawnPaddle();
        SpawnBall();
        UpdateUI();
    }

    void GoToMainMenu() =>
        SceneManager.LoadScene(PauseManager.MainMenuScene);

    Sprite CreateSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,1,1),
                             new Vector2(0.5f,0.5f), 1f);
    }
}