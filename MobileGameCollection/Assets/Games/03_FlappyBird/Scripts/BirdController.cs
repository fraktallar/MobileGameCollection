using UnityEngine;

public class BirdController : MonoBehaviour
{
    public float gravity      = -18f;
    public float jumpForce    =   7f;
    public float maxFallSpeed = -12f;

    private float velocityY  = 0f;
    private bool  isDead     = false;
    private const float ballRadius = 0.22f;

    private Transform wing;
    private float wingAngle = 0f;

    void Start()
    {
        transform.localScale = Vector3.one;
        CreateBirdVisual();
    }

    void CreateBirdVisual()
    {
        GameObject wingGO = CreatePart("Wing", new Vector3(-0.07f, -0.04f, 0),
            new Vector3(0.3f, 0.18f, 1f),
            CreateOval(18, 12), new Color(0.88f, 0.58f, 0.08f), 3);
        wing = wingGO.transform;

        CreatePart("Belly", new Vector3(-0.03f, -0.09f, 0),
            new Vector3(0.44f, 0.36f, 1f),
            CreateOval(30, 24), new Color(1f, 0.9f, 0.42f), 4);

        CreatePart("Body", new Vector3(0, 0, 0),
            new Vector3(0.55f, 0.44f, 1f),
            CreateOval(36, 28), new Color(1f, 0.76f, 0.12f), 5);

        CreatePart("BodyShine", new Vector3(-0.08f, 0.06f, 0),
            new Vector3(0.22f, 0.14f, 1f),
            CreateOval(20, 14), new Color(1f, 0.92f, 0.45f, 0.5f), 6);

        CreatePart("Beak", new Vector3(0.26f, -0.01f, 0),
            new Vector3(0.2f, 0.11f, 1f),
            CreateRoundedBeak(22, 14, 5), new Color(1f, 0.52f, 0.12f), 7);

        CreatePart("EyeWhite", new Vector3(0.15f, 0.10f, 0),
            new Vector3(0.13f, 0.13f, 1f),
            CreateCircle(12), Color.white, 8);

        CreatePart("Pupil", new Vector3(0.18f, 0.09f, 0),
            new Vector3(0.06f, 0.06f, 1f),
            CreateCircle(8), new Color(0.1f, 0.05f, 0.15f), 9);
    }

    GameObject CreatePart(string partName, Vector3 localPos, Vector3 localScale,
                          Sprite sprite, Color color, int order)
    {
        GameObject go = new GameObject(partName);
        go.transform.SetParent(transform);
        go.transform.localPosition = localPos;
        go.transform.localScale    = localScale;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        return go;
    }

    void Update()
    {
        if (isDead) return;

        bool tapped = Input.GetKeyDown(KeyCode.Space) ||
                      (Input.touchCount > 0 &&
                       Input.GetTouch(0).phase == TouchPhase.Began);

        if (tapped)
        {
            FlappyGameManager.Instance.StartGame();
            velocityY = jumpForce;
            GameAudio.PlayChirp();
        }

        AnimateWing();

        if (!FlappyGameManager.Instance.IsGameStarted()) return;

        velocityY += gravity * Time.deltaTime;
        velocityY  = Mathf.Max(velocityY, maxFallSpeed);
        transform.position += Vector3.up * velocityY * Time.deltaTime;

        float angle = Mathf.Clamp(velocityY * 4f, -85f, 30f);
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (transform.position.y > 5.2f) velocityY = -2f;

        Vector2 pos = transform.position;

        if (pos.y <= -4.7f) { Die(); return; }

        GameObject[] pipes = GameObject.FindGameObjectsWithTag("Pipe");
        foreach (GameObject pipe in pipes)
        {
            if (pipe == null) continue;
            if (OverlapsPipe(pos, pipe)) { Die(); return; }
        }

        GameObject[] zones = GameObject.FindGameObjectsWithTag("ScoreZone");
        foreach (GameObject zone in zones)
        {
            if (zone == null) continue;
            if (OverlapsScoreZone(transform.position, zone))
            {
                FlappyGameManager.Instance.AddScore();
                zone.tag = "Untagged";
            }
        }
    }

    void AnimateWing()
    {
        if (wing == null) return;
        float targetAngle = velocityY > 1f ? 30f : (velocityY < -3f ? -25f : 0f);
        wingAngle = Mathf.LerpAngle(wingAngle, targetAngle, Time.deltaTime * 12f);
        wing.localRotation = Quaternion.Euler(0, 0, wingAngle);
    }

    bool OverlapsPipe(Vector2 birdPos, GameObject pipe)
    {
        SpriteRenderer[] srs = pipe.GetComponentsInChildren<SpriteRenderer>();
        if (srs == null || srs.Length == 0) return false;
        Bounds b = srs[0].bounds;
        for (int i = 1; i < srs.Length; i++)
            b.Encapsulate(srs[i].bounds);
        float cx = Mathf.Clamp(birdPos.x, b.min.x, b.max.x);
        float cy = Mathf.Clamp(birdPos.y, b.min.y, b.max.y);
        float dx = birdPos.x - cx, dy = birdPos.y - cy;
        return (dx * dx + dy * dy) < (ballRadius * ballRadius);
    }

    bool OverlapsScoreZone(Vector2 birdPos, GameObject zone)
    {
        Vector3 p = zone.transform.position;
        Vector3 s = zone.transform.localScale;
        return birdPos.x >= p.x - s.x / 2f && birdPos.x <= p.x + s.x / 2f &&
               birdPos.y >= p.y - s.y / 2f && birdPos.y <= p.y + s.y / 2f;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        GameAudio.PlayGameOver();
        CameraShake.Shake(0.3f, 0.2f);
        FlappyGameManager.Instance.GameOver();
    }

    // ── Sprite oluşturucular ────────────────────────────

    Sprite CreateOval(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 c = new Vector2(w / 2f, h / 2f);
        float rx = w / 2f - 0.5f, ry = h / 2f - 0.5f;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = (x - c.x) / rx, dy = (y - c.y) / ry;
                tex.SetPixel(x, y, dx * dx + dy * dy <= 1f
                    ? Color.white : Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h),
                             new Vector2(0.5f, 0.5f), Mathf.Max(w, h));
    }

    Sprite CreateCircle(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 c = new Vector2(size / 2f, size / 2f);
        float r = size / 2f - 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y,
                    Vector2.Distance(new Vector2(x, y), c) <= r
                    ? Color.white : Color.clear);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size),
                             new Vector2(0.5f, 0.5f), size);
    }

    Sprite CreateRoundedBeak(int w, int h, int r)
    {
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, InBeakRect(x, y, w, h, r) ? Color.white : Color.clear);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0f, 0.5f), Mathf.Max(w, h) / 0.85f);
    }

    static bool InBeakRect(int x, int y, int w, int h, int rad)
    {
        if (x < 0 || x >= w || y < 0 || y >= h) return false;
        if (x >= rad && x < w - rad) return true;
        if (y >= rad && y < h - rad) return true;
        Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
        float rr = rad + 0.3f;
        if (x < rad && y < rad)
            return Vector2.Distance(p, new Vector2(rad - 0.5f, rad - 0.5f)) <= rr;
        if (x >= w - rad && y < rad)
            return Vector2.Distance(p, new Vector2(w - rad - 0.5f, rad - 0.5f)) <= rr;
        if (x < rad && y >= h - rad)
            return Vector2.Distance(p, new Vector2(rad - 0.5f, h - rad - 0.5f)) <= rr;
        if (x >= w - rad && y >= h - rad)
            return Vector2.Distance(p, new Vector2(w - rad - 0.5f, h - rad - 0.5f)) <= rr;
        return false;
    }
}
