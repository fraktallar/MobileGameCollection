using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    // ── Oyun verisi ────────────────────────────────────────────────────────────
    private static readonly (string name, string scene, string path, Color theme)[] Games =
    {
        ("Snake",         "SnakeGame",    "Assets/Games/01_Snake/Scenes/SnakeGame.unity",               new Color(0.15f, 0.80f, 0.25f)),
        ("Brick Breaker", "BrickBreaker", "Assets/Games/02_BrickBreaker/Scenes/BrickBreaker.unity",     new Color(0.95f, 0.45f, 0.15f)),
        ("Flappy Bird",   "FlappyBird",   "Assets/Games/03_FlappyBird/Scenes/FlappyBird.unity",         new Color(0.25f, 0.65f, 0.95f)),
        ("Color Switch",  "ColorSwitch",  "Assets/Games/04_ColorSwitch/Scenes/ColorSwitch.unity",       new Color(0.85f, 0.25f, 0.90f)),
        ("Bubble Shooter","BubbleShooter","Assets/Games/05_BubbleShooter/Scenes/BubbleShooter.unity",   new Color(0.25f, 0.50f, 0.95f)),
    };

    // ── Dil yenileme referansları ──────────────────────────────────────────────
    private readonly List<(string key, TextMeshProUGUI tmp)> _locTexts =
        new List<(string, TextMeshProUGUI)>();

    private readonly List<(Image img, Color activeCol, int idx)> _langBtns =
        new List<(Image, Color, int)>();

    private GameObject settingsPanel;

    // ── Yaşam döngüsü ─────────────────────────────────────────────────────────
    void Start()
    {
        if (LocalizationManager.Instance == null)
            new GameObject("LocalizationManager").AddComponent<LocalizationManager>();

        if (GameAudio.Instance == null)
            new GameObject("GameAudio").AddComponent<GameAudio>();

        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        Camera.main.backgroundColor = new Color(0.06f, 0.07f, 0.14f);
        Camera.main.clearFlags      = CameraClearFlags.SolidColor;

        // Ana menü sahne bilgisini PauseManager'a kaydet (Ana Menü butonu için)
        PauseManager.MainMenuScene = SceneManager.GetActiveScene().name;
        PauseManager.MainMenuPath  = SceneManager.GetActiveScene().path;

        LocalizationManager.OnLanguageChanged += OnLangChanged;
        BuildUI();
    }

    void OnDestroy() => LocalizationManager.OnLanguageChanged -= OnLangChanged;

    void OnLangChanged()
    {
        RefreshTexts();
        RefreshLangButtons();
    }

    // ── Ana UI ─────────────────────────────────────────────────────────────────
    void BuildUI()
    {
        var canvasGO = new GameObject("MainCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var cs = canvasGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1080, 1920);
        cs.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        MakeUI(canvasGO, "BG").AddComponent<Image>().color = new Color(0.07f, 0.08f, 0.15f);

        BuildHeader(canvasGO);
        BuildCards(canvasGO);
        BuildSettingsPanel(canvasGO);
    }

    // ── Header ─────────────────────────────────────────────────────────────────
    void BuildHeader(GameObject canvas)
    {
        var header = MakeChild(canvas, "Header",
            new Vector2(0,1), new Vector2(1,1), new Vector2(0.5f,1), Vector2.zero, new Vector2(0,130));
        header.AddComponent<Image>().color = new Color(0.05f, 0.06f, 0.13f);

        // Alt çizgi
        var line = MakeChild(header, "Line",
            new Vector2(0,0), new Vector2(1,0), new Vector2(0.5f,0), Vector2.zero, new Vector2(0,2));
        line.AddComponent<Image>().color = new Color(0.3f, 0.4f, 0.7f, 0.45f);

        // Başlık
        var titleGO  = MakeUI(header, "Title");
        var trt      = titleGO.GetComponent<RectTransform>();
        trt.offsetMin = new Vector2(24, 0); trt.offsetMax = new Vector2(-120, 0);
        var title    = titleGO.AddComponent<TextMeshProUGUI>();
        title.fontSize  = 42; title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.MidlineLeft;
        title.color     = Color.white;
        RegLoc("app_title", title);

        // Ayarlar butonu ⚙
        var sb  = MakeChild(header, "SettingsBtn",
            new Vector2(1,0.5f), new Vector2(1,0.5f), new Vector2(1,0.5f),
            new Vector2(-18,0), new Vector2(90,90));
        sb.AddComponent<Image>().color = new Color(0.14f, 0.16f, 0.32f);
        sb.AddComponent<Button>().onClick.AddListener(OpenSettings);
        var gear     = MakeUI(sb,"Gear").AddComponent<TextMeshProUGUI>();
        gear.text = "⚙"; gear.fontSize = 44;
        gear.alignment = TextAlignmentOptions.Center;
        gear.color = new Color(0.7f, 0.8f, 1f);
    }

    // ── Kart ızgarası (ScrollView) ─────────────────────────────────────────────
    void BuildCards(GameObject canvas)
    {
        var sv   = new GameObject("ScrollView");
        var svrt = sv.AddComponent<RectTransform>();
        sv.transform.SetParent(canvas.transform, false);
        svrt.anchorMin = Vector2.zero; svrt.anchorMax = Vector2.one;
        svrt.offsetMin = Vector2.zero; svrt.offsetMax = new Vector2(0, -130);
        sv.AddComponent<RectMask2D>();
        var scroll = sv.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.decelerationRate = 0.135f;

        var content     = new GameObject("Content");
        var crt         = content.AddComponent<RectTransform>();
        content.transform.SetParent(sv.transform, false);
        crt.anchorMin = new Vector2(0,1); crt.anchorMax = new Vector2(1,1);
        crt.pivot     = new Vector2(0.5f,1);
        crt.anchoredPosition = Vector2.zero;
        scroll.content = crt;

        const float padX = 20f, padY = 28f, gapX = 16f, gapY = 20f;
        const int   cols = 2;
        float cardW = (1080f - padX * 2 - gapX) / 2f;
        float cardH = cardW * 1.12f;
        int   rows  = Mathf.CeilToInt((float)Games.Length / cols);
        crt.sizeDelta = new Vector2(0, padY + rows * cardH + (rows-1) * gapY + padY);

        for (int i = 0; i < Games.Length; i++)
        {
            int col = i % cols, row = i / cols;
            bool lastOdd = (i == Games.Length - 1 && Games.Length % 2 == 1);
            float x = lastOdd ? 0f : col == 0 ? -(cardW/2f + gapX/2f) : (cardW/2f + gapX/2f);
            float y = -(padY + row * (cardH + gapY) + cardH / 2f);
            var g = Games[i];
            BuildCard(content, g.name, g.scene, g.path, g.theme, x, y, cardW, cardH, i);
        }
    }

    void BuildCard(GameObject parent, string gameName, string scene, string path,
                   Color theme, float x, float y, float w, float h, int idx)
    {
        var card = new GameObject("Card_" + gameName);
        var rt   = card.AddComponent<RectTransform>();
        card.transform.SetParent(parent.transform, false);
        rt.anchorMin = new Vector2(0.5f,1); rt.anchorMax = new Vector2(0.5f,1);
        rt.pivot     = new Vector2(0.5f,0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x, y);
        card.AddComponent<Image>().color = new Color(0.11f, 0.13f, 0.22f);

        // Üst renk şeridi
        var stripe = MakeChild(card,"Stripe",
            new Vector2(0,1), new Vector2(1,1), new Vector2(0.5f,1), Vector2.zero, new Vector2(0,6));
        stripe.AddComponent<Image>().color = theme;

        // İkon arka planı
        float iconH = h * 0.57f;
        var iconBg  = MakeChild(card,"IconBg",
            new Vector2(0,1), new Vector2(1,1), new Vector2(0.5f,1), new Vector2(0,-6), new Vector2(0,iconH));
        iconBg.AddComponent<Image>().color = theme * 0.18f;

        // İkon görseli
        var iconGO  = new GameObject("Icon");
        var irt     = iconGO.AddComponent<RectTransform>();
        iconGO.transform.SetParent(iconBg.transform, false);
        irt.anchorMin = new Vector2(0.1f,0.1f); irt.anchorMax = new Vector2(0.9f,0.9f);
        irt.offsetMin = irt.offsetMax = Vector2.zero;
        var iconImg = iconGO.AddComponent<Image>();
        iconImg.sprite = CreateIcon(idx);
        iconImg.preserveAspect = true;

        // Oyun adı (lokalize edilmez, kendi adı)
        float bottomY = iconH + 6;
        var nameGO  = new GameObject("Name");
        var nrt     = nameGO.AddComponent<RectTransform>();
        nameGO.transform.SetParent(card.transform, false);
        nrt.anchorMin = new Vector2(0,1); nrt.anchorMax = new Vector2(1,1);
        nrt.pivot     = new Vector2(0,1);
        nrt.anchoredPosition = new Vector2(16, -(bottomY + 10));
        nrt.sizeDelta = new Vector2(-32, 52);
        var nameText = nameGO.AddComponent<TextMeshProUGUI>();
        nameText.text = gameName; nameText.fontSize = 29;
        nameText.fontStyle = FontStyles.Bold;
        nameText.color     = Color.white;
        nameText.alignment = TextAlignmentOptions.MidlineLeft;

        // Oyna butonu
        var playBtn = new GameObject("PlayBtn");
        var prt     = playBtn.AddComponent<RectTransform>();
        playBtn.transform.SetParent(card.transform, false);
        prt.anchorMin = new Vector2(0,0); prt.anchorMax = new Vector2(1,0);
        prt.pivot     = new Vector2(0.5f,0);
        prt.sizeDelta = new Vector2(-24, 60);
        prt.anchoredPosition = new Vector2(0, 14);
        playBtn.AddComponent<Image>().color = theme;
        var pBtn = playBtn.AddComponent<Button>();
        var cb   = pBtn.colors;
        cb.highlightedColor = theme * 1.35f; cb.pressedColor = theme * 0.60f;
        pBtn.colors = cb;
        string sn = scene, sp = path;
        pBtn.onClick.AddListener(() => LoadGame(sn, sp));

        var pLabel = MakeUI(playBtn,"Label").AddComponent<TextMeshProUGUI>();
        pLabel.fontSize = 27; pLabel.fontStyle = FontStyles.Bold;
        pLabel.alignment = TextAlignmentOptions.Center;
        pLabel.color = Color.white;
        RegLoc("play", pLabel);
    }

    // ── Ayarlar paneli ─────────────────────────────────────────────────────────
    void BuildSettingsPanel(GameObject canvas)
    {
        settingsPanel = MakeUI(canvas, "SettingsPanel");
        settingsPanel.AddComponent<Image>().color = new Color(0,0,0,0.88f);

        // Kutu
        var box = new GameObject("Box");
        var brt = box.AddComponent<RectTransform>();
        box.transform.SetParent(settingsPanel.transform, false);
        brt.anchorMin = new Vector2(0.5f,0.5f); brt.anchorMax = new Vector2(0.5f,0.5f);
        brt.pivot     = new Vector2(0.5f,0.5f);
        brt.sizeDelta = new Vector2(960, 880);
        brt.anchoredPosition = Vector2.zero;
        box.AddComponent<Image>().color = new Color(0.10f, 0.11f, 0.20f);

        // Başlık
        RegLoc("settings", AddLabel(box, 0, -55, 900, 80, 50, Color.white, FontStyles.Bold));
        AddDivider(box, -128);

        // Ses
        RegLoc("volume", AddLabel(box, -200, -185, 450, 50, 34, new Color(0.75f,0.82f,1f)));
        BuildVolumeSlider(box, -258);
        AddDivider(box, -324);

        // Dil
        RegLoc("language", AddLabel(box, 0, -374, 900, 50, 34, new Color(0.75f,0.82f,1f)));
        BuildLanguageButtons(box, -450);

        // Kapat butonu
        var closeBtn = MakeChild(box,"CloseBtn",
            new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0.5f,0),
            new Vector2(0,38), new Vector2(340,72));
        closeBtn.AddComponent<Image>().color = new Color(0.55f,0.10f,0.10f);
        closeBtn.AddComponent<Button>().onClick.AddListener(CloseSettings);
        var cLabel = MakeUI(closeBtn,"L").AddComponent<TextMeshProUGUI>();
        cLabel.fontSize = 30; cLabel.fontStyle = FontStyles.Bold;
        cLabel.alignment = TextAlignmentOptions.Center;
        cLabel.color = Color.white;
        RegLoc("close", cLabel);

        settingsPanel.SetActive(false);
    }

    void BuildVolumeSlider(GameObject parent, float yOff)
    {
        var go  = new GameObject("VolumeSlider");
        var rt  = go.AddComponent<RectTransform>();
        go.transform.SetParent(parent.transform, false);
        rt.anchorMin = new Vector2(0.5f,1); rt.anchorMax = new Vector2(0.5f,1);
        rt.pivot     = new Vector2(0.5f,1);
        rt.sizeDelta = new Vector2(820,60);
        rt.anchoredPosition = new Vector2(0, yOff);

        var slider = go.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f;
        slider.value    = PlayerPrefs.GetFloat("Volume", 1f);

        // Track
        var track = MakeChild(go,"Track",
            new Vector2(0,0.5f), new Vector2(1,0.5f), new Vector2(0.5f,0.5f),
            Vector2.zero, new Vector2(0,10));
        var trackImg = track.AddComponent<Image>();
        trackImg.color = new Color(0.22f,0.25f,0.42f);
        slider.targetGraphic = trackImg;

        // Fill area
        var fa   = new GameObject("FillArea");
        var fart = fa.AddComponent<RectTransform>();
        fa.transform.SetParent(go.transform, false);
        fart.anchorMin = new Vector2(0,0.5f); fart.anchorMax = new Vector2(1,0.5f);
        fart.offsetMin = new Vector2(5,-5);   fart.offsetMax = new Vector2(-5,5);

        var fill = new GameObject("Fill");
        var frt  = fill.AddComponent<RectTransform>();
        fill.transform.SetParent(fa.transform, false);
        frt.anchorMin = Vector2.zero; frt.anchorMax = new Vector2(1,1);
        frt.offsetMin = frt.offsetMax = Vector2.zero;
        fill.AddComponent<Image>().color = new Color(0.35f,0.58f,1f);
        slider.fillRect = frt;

        // Handle
        var ha   = new GameObject("HandleArea");
        var hart = ha.AddComponent<RectTransform>();
        ha.transform.SetParent(go.transform, false);
        hart.anchorMin = Vector2.zero; hart.anchorMax = Vector2.one;
        hart.offsetMin = new Vector2(10,0); hart.offsetMax = new Vector2(-10,0);

        var handle = new GameObject("Handle");
        var hrt    = handle.AddComponent<RectTransform>();
        handle.transform.SetParent(ha.transform, false);
        hrt.sizeDelta = new Vector2(44,44);
        handle.AddComponent<Image>().color = new Color(0.5f,0.72f,1f);
        slider.handleRect = hrt;

        slider.onValueChanged.AddListener(v => {
            PlayerPrefs.SetFloat("Volume", v);
            PlayerPrefs.Save();
            GameAudio.SetSFXVolume(v);
        });
    }

    void BuildLanguageButtons(GameObject parent, float yOff)
    {
        var langs = new (string label, Color col)[]
        {
            ("Türkçe",  new Color(0.80f,0.08f,0.08f)),
            ("English", new Color(0.08f,0.22f,0.65f)),
            ("Deutsch", new Color(0.15f,0.15f,0.15f)),
            ("ZH",      new Color(0.72f,0.08f,0.08f)),
        };

        float btnW = 206f, btnH = 74f, gap = 10f;
        float total  = langs.Length * btnW + (langs.Length-1) * gap;
        float startX = -total / 2f + btnW / 2f;

        _langBtns.Clear();

        for (int i = 0; i < langs.Length; i++)
        {
            int idx = i;
            bool selected = LocalizationManager.Instance != null &&
                            (int)LocalizationManager.Instance.Current == i;
            Color col = langs[i].col;

            var btn = new GameObject("LangBtn_" + i);
            var rt  = btn.AddComponent<RectTransform>();
            btn.transform.SetParent(parent.transform, false);
            rt.anchorMin = new Vector2(0.5f,1); rt.anchorMax = new Vector2(0.5f,1);
            rt.pivot     = new Vector2(0.5f,1);
            rt.sizeDelta = new Vector2(btnW, btnH);
            rt.anchoredPosition = new Vector2(startX + i*(btnW+gap), yOff);

            var img    = btn.AddComponent<Image>();
            img.color  = selected ? col : col * 0.45f;
            _langBtns.Add((img, col, idx));

            var button = btn.AddComponent<Button>();
            var cb     = button.colors;
            cb.normalColor      = selected ? col : col * 0.45f;
            cb.highlightedColor = col * 1.3f;
            cb.pressedColor     = col * 0.5f;
            button.colors = cb;
            button.onClick.AddListener(() =>
                LocalizationManager.Instance?.SetLanguage((LocalizationManager.Language)idx));

            var t      = MakeUI(btn,"L").AddComponent<TextMeshProUGUI>();
            t.text     = langs[i].label;
            t.fontSize = 27; t.fontStyle = FontStyles.Bold;
            t.alignment = TextAlignmentOptions.Center;
            t.color    = Color.white;
        }
    }

    // ── Dil yenileme ───────────────────────────────────────────────────────────
    void RefreshTexts()
    {
        foreach (var (key, tmp) in _locTexts)
            if (tmp != null) tmp.text = L(key);
    }

    void RefreshLangButtons()
    {
        int cur = LocalizationManager.Instance != null
                  ? (int)LocalizationManager.Instance.Current : 0;

        foreach (var (img, activeCol, idx) in _langBtns)
        {
            if (img == null) continue;
            bool selected = idx == cur;
            img.color = selected ? activeCol : activeCol * 0.45f;
        }
    }

    // ── Sahne yükleme ──────────────────────────────────────────────────────────
    void LoadGame(string sceneName, string scenePath)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    // ── İkon üretici ───────────────────────────────────────────────────────────
    Sprite CreateIcon(int idx)
    {
        var tex = new Texture2D(128, 128);
        tex.filterMode = FilterMode.Bilinear;
        switch (idx)
        {
            case 0: DrawSnake(tex);       break;
            case 1: DrawBricks(tex);      break;
            case 2: DrawFlappy(tex);      break;
            case 3: DrawColorSwitch(tex); break;
            case 4: DrawBubbles(tex);     break;
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,128,128), new Vector2(0.5f,0.5f));
    }

    void DrawSnake(Texture2D t)
    {
        Fill(t,0,0,128,128, new Color(0.04f,0.18f,0.04f));
        Color body = new Color(0.22f,0.85f,0.22f), head = new Color(0.35f,1f,0.35f);
        Fill(t,14,90,96,14,body); Fill(t,14,76,14,14,body);
        Fill(t,14,62,96,14,body); Fill(t,96,48,14,14,body);
        Fill(t,14,34,96,14,body); Fill(t,96,34,14,14,head);
        Fill(t,104,40,4,4,Color.black); Fill(t,98,40,4,4,Color.black);
    }

    void DrawBricks(Texture2D t)
    {
        Fill(t,0,0,128,128, new Color(0.05f,0.05f,0.16f));
        Color[] c = { new Color(0.9f,0.2f,0.2f), new Color(0.9f,0.55f,0.1f),
                       new Color(0.9f,0.85f,0.1f), new Color(0.2f,0.8f,0.3f) };
        for (int r = 0; r < 4; r++)
        {
            int y = 100 - r*17, off = r%2==0 ? 0 : 12;
            for (int col=-1; col<5; col++) Fill(t, off+col*27+2, y, 24, 13, c[r]);
        }
        Circle(t, 64, 22, 9, Color.white);
    }

    void DrawFlappy(Texture2D t)
    {
        Fill(t,0,0,128,128, new Color(0.28f,0.62f,0.90f));
        Fill(t,0,58,36,70,  new Color(0.14f,0.58f,0.14f));
        Fill(t,0,58,42,9,   new Color(0.22f,0.74f,0.22f));
        Fill(t,92,0,36,50,  new Color(0.14f,0.58f,0.14f));
        Fill(t,86,40,42,9,  new Color(0.22f,0.74f,0.22f));
        Oval(t,55,58,22,16, new Color(1f,0.85f,0.08f));
        Oval(t,47,50,14,8,  new Color(0.88f,0.62f,0f));
        Circle(t,64,64,5,Color.white); Circle(t,65,64,3,Color.black);
        Fill(t,72,59,10,5,  new Color(1f,0.48f,0.05f));
    }

    void DrawColorSwitch(Texture2D t)
    {
        Fill(t,0,0,128,128, new Color(0.08f,0.08f,0.13f));
        Color[] s = { new Color(0.95f,0.25f,0.25f), new Color(0.25f,0.75f,0.95f),
                       new Color(0.25f,0.90f,0.40f), new Color(0.95f,0.85f,0.15f) };
        for (int py=0; py<128; py++) for (int px=0; px<128; px++)
        {
            float dx=px-64f, dy=py-64f;
            if (dx*dx+dy*dy > 52f*52f) continue;
            int si = (dx>=0?0:1)+(dy>=0?0:2);
            t.SetPixel(px,py,s[si]);
        }
        Fill(t,62,12,4,104, new Color(0.08f,0.08f,0.13f));
        Fill(t,12,62,104,4, new Color(0.08f,0.08f,0.13f));
        Circle(t,64,64,10,Color.white);
    }

    void DrawBubbles(Texture2D t)
    {
        Fill(t,0,0,128,128, new Color(0.07f,0.08f,0.15f));
        Color[] c = {
            new Color(0.95f,0.25f,0.25f), new Color(0.25f,0.75f,0.95f),
            new Color(0.25f,0.90f,0.40f), new Color(0.95f,0.85f,0.15f),
            new Color(0.85f,0.25f,0.90f), new Color(0.95f,0.60f,0.15f),
        };
        int[] cx={24,64,104,24,64,104}, cy={88,88,88,50,50,50};
        for (int i=0; i<6; i++) Circle(t,cx[i],cy[i],16,c[i]);
        Circle(t,44,16,15,c[2]); Circle(t,84,16,15,c[3]);
    }

    // ── Piksel yardımcıları ────────────────────────────────────────────────────
    void Fill(Texture2D t, int x, int y, int w, int h, Color c)
    {
        x=Mathf.Clamp(x,0,t.width); y=Mathf.Clamp(y,0,t.height);
        w=Mathf.Clamp(w,0,t.width-x); h=Mathf.Clamp(h,0,t.height-y);
        for (int py=y; py<y+h; py++) for (int px=x; px<x+w; px++) t.SetPixel(px,py,c);
    }

    void Circle(Texture2D t, int cx, int cy, int r, Color c)
    {
        for (int py=cy-r; py<=cy+r; py++) for (int px=cx-r; px<=cx+r; px++)
        {
            if (px<0||px>=t.width||py<0||py>=t.height) continue;
            if ((px-cx)*(px-cx)+(py-cy)*(py-cy)<=r*r) t.SetPixel(px,py,c);
        }
    }

    void Oval(Texture2D t, int cx, int cy, int rx, int ry, Color c)
    {
        for (int py=cy-ry; py<=cy+ry; py++) for (int px=cx-rx; px<=cx+rx; px++)
        {
            if (px<0||px>=t.width||py<0||py>=t.height) continue;
            float nx=(float)(px-cx)/rx, ny=(float)(py-cy)/ry;
            if (nx*nx+ny*ny<=1f) t.SetPixel(px,py,c);
        }
    }

    // ── UI yardımcıları ────────────────────────────────────────────────────────

    // Tam ekran dolduran UI child (RectTransform önce eklenir)
    GameObject MakeUI(GameObject parent, string name)
    {
        var go = new GameObject(name);
        var rt = go.AddComponent<RectTransform>();
        go.transform.SetParent(parent.transform, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    // Anchor/pivot/pos/size ile UI child
    GameObject MakeChild(GameObject parent, string name,
        Vector2 ancMin, Vector2 ancMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        var rt = go.AddComponent<RectTransform>();
        go.transform.SetParent(parent.transform, false);
        rt.anchorMin = ancMin; rt.anchorMax = ancMax; rt.pivot = pivot;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        return go;
    }

    // Metin oluştur ve döndür
    TextMeshProUGUI AddLabel(GameObject parent, float x, float y, float w, float h,
        int size, Color color, FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject("Label");
        var rt = go.AddComponent<RectTransform>();
        go.transform.SetParent(parent.transform, false);
        rt.anchorMin = new Vector2(0.5f,1); rt.anchorMax = new Vector2(0.5f,1);
        rt.pivot     = new Vector2(0.5f,1);
        rt.anchoredPosition = new Vector2(x,y); rt.sizeDelta = new Vector2(w,h);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = size; tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        return tmp;
    }

    void AddDivider(GameObject parent, float y)
    {
        var go = MakeChild(parent,"Divider",
            new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1),
            new Vector2(0,y), new Vector2(880,2));
        go.AddComponent<Image>().color = new Color(0.22f,0.26f,0.44f);
    }

    // Dil için kayıt: key değişince text güncellenir
    void RegLoc(string key, TextMeshProUGUI tmp)
    {
        tmp.text = L(key);
        _locTexts.Add((key, tmp));
    }

    void OpenSettings()  => settingsPanel.SetActive(true);
    void CloseSettings() => settingsPanel.SetActive(false);
    string L(string key) => LocalizationManager.Instance?.Get(key) ?? key;
}
