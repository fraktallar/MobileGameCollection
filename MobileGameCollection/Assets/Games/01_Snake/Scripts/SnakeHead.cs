using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : MonoBehaviour
{
    [Header("Settings")]
    public int initialSize = 3;
    public float moveInterval = 0.2f;

    private Vector2 direction = Vector2.right;
    private Vector2 nextDirection = Vector2.right;
    private List<Vector3> positionHistory = new List<Vector3>();
    private List<GameObject> bodyParts = new List<GameObject>();
    private float moveTimer = 0f;
    private bool isDead = false;

    private Vector2 touchStartPos;
    private bool swipeRegistered = false;
    private const float SWIPE_THRESHOLD = 40f;


    void Start()
    {
        moveInterval = SnakeGameManager.Instance.GetMoveInterval();
        transform.position = Vector3.zero;

        // Head'e görsel + collider ekle (Inspector'a gerek yok)
        SetupVisual(gameObject, Color.white, isHead: true);
        SetupCollider(gameObject, true);

        // Başlangıç body'leri
        for (int i = 0; i < initialSize - 1; i++)
            SpawnBodyPart();
    }

    // Sprite'ı koddan oluştur
    void SetupVisual(GameObject obj, Color color, bool isHead = false)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = isHead ? CreateHeadSprite() : CreateBodySprite(color);
        sr.color  = Color.white; // renk texture'a gömülü
        sr.sortingOrder = 1;
    }

    // Collider'ı koddan ekle
    void SetupCollider(GameObject obj, bool isTrigger)
    {
        BoxCollider2D col = obj.GetComponent<BoxCollider2D>();
        if (col == null) col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.9f, 0.9f);
        col.isTrigger = isTrigger;

        if (isTrigger)
        {
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb == null) rb = obj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
        }
    }

    // ── Kafa sprite: oval + gözler + dil ────────────────────────────────────
    Sprite CreateHeadSprite()
    {
        int sz = 64;
        var tex = new Texture2D(sz, sz) { filterMode = FilterMode.Bilinear };
        for (int y = 0; y < sz; y++) for (int x = 0; x < sz; x++) tex.SetPixel(x, y, Color.clear);

        Color bodyGreen = new Color(0.22f, 0.82f, 0.32f);
        Color darkGreen = new Color(0.12f, 0.52f, 0.18f);
        Color outline   = new Color(0.08f, 0.35f, 0.10f);

        // Oval gövde (sağa uzun — yön sağ kabul)
        for (int y = 4; y < 60; y++)
        for (int x = 4; x < 62; x++)
        {
            float nx = (x - 33f) / 27f, ny = (y - 32f) / 26f;
            float d  = nx * nx + ny * ny;
            if (d > 1f) continue;
            Color c = Color.Lerp(bodyGreen, darkGreen, Mathf.Clamp01(d * 1.2f + ny * 0.3f));
            tex.SetPixel(x, y, d > 0.88f ? outline : c);
        }

        // Sağ burun ucu (biraz daha sivri)
        for (int y = 20; y < 44; y++)
        for (int x = 52; x < 64; x++)
        {
            float nx = (x - 54f) / 10f, ny = (y - 32f) / 11f;
            if (nx * nx + ny * ny < 1f) tex.SetPixel(x, y, darkGreen);
        }

        // Gözler (sağ tarafa yakın, üst + alt)
        DrawEye(tex, 46, 44); // üst göz
        DrawEye(tex, 46, 20); // alt göz

        // Dil (kırmızı, iki çatal, sağdan çıkıyor)
        Color tongue = new Color(0.90f, 0.10f, 0.15f);
        for (int x = 57; x < 64; x++) tex.SetPixel(x, 32, tongue);
        for (int x = 62; x < 66 && x < sz; x++) { tex.SetPixel(x, 34, tongue); tex.SetPixel(x, 30, tongue); }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), sz);
    }

    void DrawEye(Texture2D tex, int cx, int cy)
    {
        // Beyaz kısım
        for (int dy = -6; dy <= 6; dy++) for (int dx = -6; dx <= 6; dx++)
            if (dx*dx+dy*dy <= 36 && cx+dx >= 0 && cx+dx < tex.width && cy+dy >= 0 && cy+dy < tex.height)
                tex.SetPixel(cx+dx, cy+dy, Color.white);
        // Siyah bebek
        for (int dy = -3; dy <= 3; dy++) for (int dx = -2; dx <= 2; dx++)
            if (dx*dx*2+dy*dy <= 10 && cx+dx >= 0 && cx+dx < tex.width && cy+dy >= 0 && cy+dy < tex.height)
                tex.SetPixel(cx+dx, cy+dy, new Color(0.05f, 0.05f, 0.05f));
        // Parlama
        if (cx+2 < tex.width && cy+3 < tex.height) tex.SetPixel(cx+2, cy+3, Color.white);
    }

    // ── Gövde sprite: pürüzsüz daire ────────────────────────────────────────
    Sprite CreateBodySprite(Color tint)
    {
        int sz = 64;
        var tex = new Texture2D(sz, sz) { filterMode = FilterMode.Bilinear };
        for (int y = 0; y < sz; y++) for (int x = 0; x < sz; x++) tex.SetPixel(x, y, Color.clear);

        Color bodyGreen = new Color(0.18f, 0.72f, 0.28f);
        Color darkGreen = new Color(0.10f, 0.45f, 0.15f);
        Color outline   = new Color(0.07f, 0.30f, 0.09f);

        for (int y = 3; y < 61; y++)
        for (int x = 3; x < 61; x++)
        {
            float nx = (x - 32f) / 27f, ny = (y - 32f) / 27f;
            float d  = nx * nx + ny * ny;
            if (d > 1f) continue;
            Color c = Color.Lerp(bodyGreen, darkGreen, Mathf.Clamp01(d * 1.3f));
            tex.SetPixel(x, y, d > 0.88f ? outline : c);
        }
        // Küçük parlama
        for (int y = 38; y < 48; y++) for (int x = 20; x < 30; x++)
        {
            float nx = (x-25f)/5f, ny = (y-43f)/4f;
            if (nx*nx+ny*ny < 1f)
                tex.SetPixel(x, y, new Color(0.45f, 0.95f, 0.55f, 0.6f));
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), sz);
    }

    void Update()
    {
        if (isDead) return;
        HandleKeyboardInput();
        HandleTouchInput();

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            moveTimer = 0f;
            Move();
        }
    }

    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) && direction != Vector2.down)
            nextDirection = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow) && direction != Vector2.up)
            nextDirection = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && direction != Vector2.right)
            nextDirection = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow) && direction != Vector2.left)
            nextDirection = Vector2.right;
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            touchStartPos = touch.position;
            swipeRegistered = false;
        }

        if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Ended) && !swipeRegistered)
        {
            Vector2 delta = touch.position - touchStartPos;
            if (delta.magnitude < SWIPE_THRESHOLD) return;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                if (delta.x > 0 && direction != Vector2.left)
                    nextDirection = Vector2.right;
                else if (delta.x < 0 && direction != Vector2.right)
                    nextDirection = Vector2.left;
            }
            else
            {
                if (delta.y > 0 && direction != Vector2.down)
                    nextDirection = Vector2.up;
                else if (delta.y < 0 && direction != Vector2.up)
                    nextDirection = Vector2.down;
            }

            // Bir sonraki swipe için sıfırla (parmak kaldırmadan arka arkaya yön değiştirme)
            swipeRegistered = true;
            touchStartPos = touch.position;
        }

        if (touch.phase == TouchPhase.Ended)
            swipeRegistered = false;
    }

    void Move()
{
    direction = nextDirection;

    // Kafa yönü rotasyonu
    float angle = direction == Vector2.right ?   0f :
                  direction == Vector2.left  ? 180f :
                  direction == Vector2.up    ?  90f : -90f;
    transform.rotation = Quaternion.Euler(0, 0, angle);

    positionHistory.Insert(0, transform.position);

    Vector3 newPos = transform.position + (Vector3)direction;

    // Sınır kontrolü — kamera boyutuna göre dinamik
    Camera cam = Camera.main;
    if (cam == null) return;
    float camH = cam.orthographicSize;
    float camW = camH * cam.aspect;

    if (newPos.x >= camW - 0.5f || newPos.x <= -camW + 0.5f ||
        newPos.y >= camH - 0.5f || newPos.y <= -(camH - 0.5f))
    {
        Die(); return;
    }

    // Kendine çarpma kontrolü
    for (int i = 0; i < bodyParts.Count; i++)
    {
        if (Vector3.Distance(newPos, bodyParts[i].transform.position) < 0.5f)
        {
            Die(); return;
        }
    }

        transform.position = newPos;

        // Body'leri güncelle
        for (int i = 0; i < bodyParts.Count; i++)
        {
            if (i < positionHistory.Count)
                bodyParts[i].transform.position = positionHistory[i];
        }
    }

    void Die()
    {
        isDead = true;
        SnakeGameManager.Instance.GameOver();
    }

    public void Cleanup()
    {
        foreach (var part in bodyParts)
            if (part != null) Destroy(part);
        bodyParts.Clear();
    }

    void SpawnBodyPart()
    {
        GameObject part = new GameObject("BodyPart_" + bodyParts.Count) { tag = "BodyPart" };

        SetupVisual(part, Color.white);

        // Başlangıçta head'in soluna diz
        part.transform.position = new Vector3(
            transform.position.x - (bodyParts.Count + 1),
            transform.position.y, 0);

        bodyParts.Add(part);
    }

    public void GrowBody()
    {
        GameObject part = new GameObject("BodyPart_" + bodyParts.Count) { tag = "BodyPart" };
        SetupVisual(part, Color.white);

        Vector3 spawnPos = bodyParts.Count > 0
            ? bodyParts[bodyParts.Count - 1].transform.position
            : transform.position;

        part.transform.position = spawnPos;
        bodyParts.Add(part);
    }

    void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Food"))
    {
        GrowBody();
        GameAudio.PlayCollect();
        SnakeGameManager.Instance.AddScore(10);
        SnakeGameManager.Instance.IncreaseSpeed();  // ← hız artışı
        moveInterval = SnakeGameManager.Instance.GetMoveInterval(); // ← yeni hızı al
        Destroy(other.gameObject);
        FindObjectOfType<FoodSpawner>().SpawnFood();
    }
}
}