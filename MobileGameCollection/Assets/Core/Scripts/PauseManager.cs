using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    private GameObject pausePanel;
    private bool isPaused = false;
    private int  resumeFrame = -100;

    // Game manager'lar buraya Restart metodlarını bağlar
    public static System.Action OnRestart;

    // MainMenuManager.Start() tarafından doldurulur
    public static string MainMenuScene = "SampleScene";
    public static string MainMenuPath  = "Assets/Scenes/SampleScene.unity";

    public bool IsInputBlocked => Time.frameCount <= resumeFrame + 1;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BuildPauseUI();
    }

    void Update()
    {
        // ESC veya Android geri tuşu
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    void BuildPauseUI()
    {
        // Canvas bul veya oluştur
        Canvas canvas = FindObjectOfType<Canvas>();
        GameObject canvasGO;
        if (canvas == null)
        {
            canvasGO = new GameObject("PauseCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvasGO = canvas.gameObject;
        }

        // Canvas'ın üstünde gözüksün
        canvas.sortingOrder = 100;

        // Pause panel
        pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(canvasGO.transform, false);

        RectTransform rt = pausePanel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image bg = pausePanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.88f);

        // DURAKLATILDI yazısı
        MakeText(pausePanel, "DURAKLATILDI",
            new Vector2(0.5f, 0.5f), new Vector2(0, 130),
            new Vector2(500, 100), 58,
            new Color(1f, 1f, 0.6f));

        // Devam et butonu
        MakeButton(pausePanel, "▶  Devam Et",
            new Vector2(0, 10), () => Resume(),
            new Color(0.15f, 0.45f, 0.15f));

        // Yeniden başla butonu
        MakeButton(pausePanel, "↺  Yeniden Başla",
            new Vector2(0, -90), () => Restart(),
            new Color(0.2f, 0.2f, 0.45f));

        // Ana Menü butonu
        MakeButton(pausePanel, "⌂  Ana Menü",
            new Vector2(0, -190), () => MainMenu(),
            new Color(0.4f, 0.15f, 0.15f));

        pausePanel.SetActive(false);
        
        MakePauseButton(canvasGO);
    }

    void MakePauseButton(GameObject canvasGO)
{
    GameObject go = new GameObject("PauseButton");
    go.transform.SetParent(canvasGO.transform, false);

    RectTransform rt = go.AddComponent<RectTransform>();
    rt.anchorMin = new Vector2(0.5f, 1f);
    rt.anchorMax = new Vector2(0.5f, 1f);
    rt.anchoredPosition = new Vector2(0, -45);
    rt.sizeDelta = new Vector2(70, 50);

    Image img = go.AddComponent<Image>();
    img.color = new Color(1f, 1f, 1f, 0.15f);

    Button btn = go.AddComponent<Button>();
    btn.onClick.AddListener(() => TogglePause());

    GameObject t = new GameObject("Label");
    t.transform.SetParent(go.transform, false);
    RectTransform tr = t.AddComponent<RectTransform>();
    tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
    tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
    TextMeshProUGUI tmp = t.AddComponent<TextMeshProUGUI>();
    tmp.text = "⏸"; tmp.fontSize = 28;
    tmp.alignment = TextAlignmentOptions.Center;
    tmp.color = Color.white;
}

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }

    public void Resume()
    {
        isPaused = false;
        resumeFrame = Time.frameCount;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }

    public bool IsPaused() => isPaused;

    void Restart()
    {
        isPaused = false;
        resumeFrame = Time.frameCount;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        OnRestart?.Invoke();
    }

    void MainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(MainMenuScene);
    }

    // ── UI yardımcıları ──────────────────────────────────
    void MakeText(GameObject parent, string text,
        Vector2 anchor, Vector2 offset, Vector2 size,
        int fontSize, Color color)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.anchoredPosition = offset; rt.sizeDelta = size;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.fontStyle = FontStyles.Bold;
    }

    void MakeButton(GameObject parent, string label,
        Vector2 offset, UnityEngine.Events.UnityAction onClick,
        Color bgColor)
    {
        GameObject go = new GameObject(label);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = offset;
        rt.sizeDelta = new Vector2(340, 75);

        Image img = go.AddComponent<Image>();
        img.color = bgColor;

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = bgColor * 1.3f;
        cb.pressedColor     = bgColor * 0.7f;
        btn.colors = cb;
        btn.onClick.AddListener(onClick);

        // Yazı
        GameObject t = new GameObject("Label");
        t.transform.SetParent(go.transform, false);
        RectTransform tr = t.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = t.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }
}