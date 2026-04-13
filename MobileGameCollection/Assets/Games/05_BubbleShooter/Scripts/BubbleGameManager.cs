using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class BubbleGameManager : MonoBehaviour
{
    public static BubbleGameManager Instance;

    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI highScoreText;
    private GameObject gameOverPanel;
    private GameObject winPanel;
    private TextMeshProUGUI finalScoreText;

    private int  score     = 0;
    private int  highScore = 0;
    private bool gameEnded = false;

    private GameObject gridGO;
    private GameObject shooterGO;
    private GameObject borderGO;
    private LineRenderer sideLineLeft;
    private LineRenderer sideLineRight;

    public static Color[] BubbleColors = new Color[]
    {
        new Color(0.95f, 0.25f, 0.25f),
        new Color(0.25f, 0.75f, 0.95f),
        new Color(0.25f, 0.90f, 0.40f),
        new Color(0.95f, 0.85f, 0.15f),
        new Color(0.85f, 0.25f, 0.95f),
    };

    void Awake() { Instance = this; Time.timeScale = 1f; }

    void Start()
    {
        // Kamerayı oyun alanına göre ayarla
        if (Camera.main != null) Camera.main.orthographicSize = 9f;

        PauseManager.OnRestart += Restart;
        highScore = PlayerPrefs.GetInt("Bubble_High", 0);
        BuildUI();
        SpawnGame();
    }

    void OnDestroy() => PauseManager.OnRestart -= Restart;

    void SpawnGame()
    {
        if (borderGO == null) DrawBorder();

        gridGO = new GameObject("BubbleGrid");
        gridGO.AddComponent<BubbleGrid>();

        shooterGO = new GameObject("BubbleShooter");
        shooterGO.AddComponent<BubbleShooter>();
    }

    void DrawBorder()
    {
        Camera cam = Camera.main;
        float h = cam.orthographicSize;
        float w = h * cam.aspect;

        if (borderGO != null) return;

        borderGO = new GameObject("Border");
        sideLineLeft = CreateSideLine("PlayfieldLeft", borderGO.transform);
        sideLineRight = CreateSideLine("PlayfieldRight", borderGO.transform);
        UpdatePlayfieldSideLines(-w, w);
    }

    static LineRenderer CreateSideLine(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = 0.045f;
        lr.endWidth = 0.045f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = new Color(1f, 1f, 1f, 0.92f);
        lr.sortingOrder = 4;
        return lr;
    }

    public void UpdatePlayfieldSideLines(float xLeftEdge, float xRightEdge)
    {
        Camera cam = Camera.main;
        if (cam == null || sideLineLeft == null || sideLineRight == null) return;
        float h = cam.orthographicSize;
        sideLineLeft.SetPosition(0, new Vector3(xLeftEdge, -h, 0));
        sideLineLeft.SetPosition(1, new Vector3(xLeftEdge, h, 0));
        sideLineRight.SetPosition(0, new Vector3(xRightEdge, -h, 0));
        sideLineRight.SetPosition(1, new Vector3(xRightEdge, h, 0));
    }

    public void AddScore(int amount)
    {
        if (gameEnded) return;
        score = Mathf.Max(0, score + amount);
        if (amount > 0 && score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("Bubble_High", highScore);
        }
        scoreText.text     = "Skor: " + score;
        highScoreText.text = "En Iyi: " + highScore;
        if (amount < 0) ShowPenalty();
    }

    void ShowPenalty()
    {
        scoreText.color = new Color(1f, 0.3f, 0.3f);
        Invoke(nameof(ResetScoreColor), 0.4f);
    }

    void ResetScoreColor() => scoreText.color = Color.white;

    public void CheckWin()
    {
        if (BubbleGrid.Instance.IsEmpty()) Win();
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
        finalScoreText.text = "Skor: " + score;
        winPanel.SetActive(true);
    }

    public bool IsEnded() => gameEnded;

    void Restart()
    {
        // Temizle
        if (gridGO    != null) Destroy(gridGO);
        if (shooterGO != null) Destroy(shooterGO);

        // Sıfırla
        score     = 0;
        gameEnded = false;
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
        scoreText.text     = "Skor: 0";
        highScoreText.text = "En Iyi: " + highScore;

        SpawnGame();
    }

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

        // Skor — sol üst, pivot düzeltilmiş
        scoreText = MakeTMP(cGO, "Score", "Skor: 0",
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(16, -16), new Vector2(250, 55), 30,
            TextAlignmentOptions.Left);
        scoreText.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);

        // En iyi — sağ üst, pivot düzeltilmiş
        highScoreText = MakeTMP(cGO, "High", "En Iyi: " + highScore,
            new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-16, -16), new Vector2(240, 55), 28,
            TextAlignmentOptions.Right);
        highScoreText.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f);

        // Game Over
        gameOverPanel = MakePanel(cGO, new Color(0, 0, 0, 0.9f));
        MakeTMP(gameOverPanel, "T", "GAME OVER",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 200), new Vector2(600, 110), 68,
            TextAlignmentOptions.Center, new Color(1f, 0.3f, 0.3f));
        finalScoreText = MakeTMP(gameOverPanel, "S", "",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 80), new Vector2(500, 80), 44,
            TextAlignmentOptions.Center);
        MakeBtn(gameOverPanel, "Tekrar Oyna", new Vector2(0, -80),  Restart);
        MakeBtn(gameOverPanel, "Ana Menu",    new Vector2(0, -190),
            () => SceneManager.LoadScene(PauseManager.MainMenuScene));
        gameOverPanel.SetActive(false);

        // Win
        winPanel = MakePanel(cGO, new Color(0, 0.05f, 0, 0.9f));
        MakeTMP(winPanel, "WT", "TEBRIKLER!",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 200), new Vector2(600, 110), 68,
            TextAlignmentOptions.Center, new Color(0.3f, 1f, 0.4f));
        MakeTMP(winPanel, "WS", "",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 80), new Vector2(500, 80), 44,
            TextAlignmentOptions.Center);
        MakeBtn(winPanel, "Tekrar Oyna", new Vector2(0, -80),  Restart);
        MakeBtn(winPanel, "Ana Menu",    new Vector2(0, -190),
            () => SceneManager.LoadScene(PauseManager.MainMenuScene));
        winPanel.SetActive(false);
    }

    TextMeshProUGUI MakeTMP(GameObject p, string name, string txt,
        Vector2 aMin, Vector2 aMax, Vector2 aPos, Vector2 sz, int fs,
        TextAlignmentOptions al = TextAlignmentOptions.Center,
        Color? col = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(p.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.anchoredPosition = aPos; rt.sizeDelta = sz;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = txt; t.fontSize = fs;
        t.alignment = al; t.color = col ?? Color.white;
        return t;
    }

    GameObject MakePanel(GameObject p, Color bg)
    {
        GameObject go = new GameObject("Panel");
        go.transform.SetParent(p.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = bg;
        return go;
    }

    void MakeBtn(GameObject p, string lbl, Vector2 pos,
                 UnityEngine.Events.UnityAction cb)
    {
        GameObject go = new GameObject(lbl);
        go.transform.SetParent(p.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(360, 85);
        go.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.4f);
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(cb);

        GameObject t = new GameObject("L");
        t.transform.SetParent(go.transform, false);
        var tr = t.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
        var tmp = t.AddComponent<TextMeshProUGUI>();
        tmp.text = lbl; tmp.fontSize = 32;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }
}
