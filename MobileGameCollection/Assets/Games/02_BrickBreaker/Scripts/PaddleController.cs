using UnityEngine;

public class PaddleController : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 12f;
    public float halfWidth = 1.5f;

    private float baseHalfWidth;
    private float leftBound;
    private float rightBound;
    private SpriteRenderer sr;
    private float wideBuffTimer;

    void Start()
    {
        baseHalfWidth = halfWidth;
        Camera cam = Camera.main;
        float camW = cam.orthographicSize * cam.aspect;
        RefreshMovementBounds(camW);
        transform.position = new Vector3(0, -8f, 0);
        ApplyVisualSize();

        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = CreateRoundedPaddleSprite();
        sr.color = new Color(0.4f, 0.6f, 1f);
        sr.sortingOrder = 2;
    }

    void Update()
    {
        if (wideBuffTimer > 0f)
        {
            wideBuffTimer -= Time.deltaTime;
            if (wideBuffTimer <= 0f)
            {
                halfWidth = baseHalfWidth;
                ApplyVisualSize();
                RefreshMovementBounds(Camera.main.orthographicSize * Camera.main.aspect);
            }
        }

        float move = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) move = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) move = 1f;

        if (Input.touchCount > 0)
        {
            float screenMid = Screen.width / 2f;
            foreach (Touch t in Input.touches)
            {
                if (t.phase == TouchPhase.Began ||
                    t.phase == TouchPhase.Moved ||
                    t.phase == TouchPhase.Stationary)
                    move += t.position.x < screenMid ? -1f : 1f;
            }
            move = Mathf.Clamp(move, -1f, 1f);
        }

        float newX = transform.position.x + move * speed * Time.deltaTime;
        newX = Mathf.Clamp(newX, leftBound, rightBound);
        transform.position = new Vector3(newX, transform.position.y, 0);
    }

    public void ApplyWidePaddleBuff(float durationSeconds, float widthMultiplier)
    {
        wideBuffTimer = durationSeconds;
        halfWidth = baseHalfWidth * widthMultiplier;
        ApplyVisualSize();
        RefreshMovementBounds(Camera.main.orthographicSize * Camera.main.aspect);
    }

    void RefreshMovementBounds(float camW)
    {
        leftBound = -camW + halfWidth + 0.3f;
        rightBound = camW - halfWidth - 0.3f;
    }

    const float PaddleHeightWorld = 0.4f;
    const float SpritePPU = 48f;

    void ApplyVisualSize()
    {
        float nativeW = 256f / SpritePPU;
        float nativeH = 48f / SpritePPU;
        transform.localScale = new Vector3(
            (halfWidth * 2f) / nativeW,
            PaddleHeightWorld / nativeH,
            1f);
    }

    public Bounds GetWorldBounds()
    {
        if (sr != null) return sr.bounds;
        return new Bounds(transform.position, new Vector3(halfWidth * 2f, PaddleHeightWorld, 0f));
    }

    /// <summary>Capsule / pill sprite so corners stay round when scaled.</summary>
    Sprite CreateRoundedPaddleSprite()
    {
        int w = 256;
        int h = 48;
        float r = h / 2f - 0.5f;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 leftC = new Vector2(r, h / 2f);
        Vector2 rightC = new Vector2(w - 1f - r, h / 2f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool inside =
                    (x >= leftC.x && x <= rightC.x) ||
                    Vector2.Distance(new Vector2(x, y), leftC) <= r + 0.01f ||
                    Vector2.Distance(new Vector2(x, y), rightC) <= r + 0.01f;
                tex.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), SpritePPU);
    }
}
