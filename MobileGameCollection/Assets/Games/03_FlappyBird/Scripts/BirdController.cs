using UnityEngine;

public class BirdController : MonoBehaviour
{
    [Header("Fizik")]
    public float gravity      = -18f;
    public float jumpForce    =   7f;
    public float maxFallSpeed = -12f;

    private float velocityY = 0f;
    private bool  isDead    = false;
    private float ballRadius = 0.25f;

    void Start()
    {
        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color  = new Color(1f, 0.85f, 0.1f);
        sr.sortingOrder = 5;
        transform.localScale = new Vector3(0.6f, 0.6f, 1f);
    }

    void Update()
    {
        if (isDead) return;
        
        GameAudio.PlayJump();

        // Input
        bool tapped = Input.GetKeyDown(KeyCode.Space) ||
                      (Input.touchCount > 0 &&
                       Input.GetTouch(0).phase == TouchPhase.Began);

        if (tapped)
        {
            FlappyGameManager.Instance.StartGame();
            velocityY = jumpForce;
        }

        if (!FlappyGameManager.Instance.IsGameStarted()) return;

        // Yerçekimi
        velocityY += gravity * Time.deltaTime;
        velocityY  = Mathf.Max(velocityY, maxFallSpeed);
        transform.position += Vector3.up * velocityY * Time.deltaTime;

        // Rotasyon
        float angle = Mathf.Clamp(velocityY * 4f, -90f, 30f);
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Üst sınır
        if (transform.position.y > 5.2f)
            velocityY = -2f;

        Vector2 pos = transform.position;

        // ── Zemin kontrolü (Y ekseninde sabit sınır) ──
        if (pos.y <= -4.7f)
        {
            Die(); return;
        }

        // ── Boru çarpışma kontrolü (bounds) ──
        GameObject[] pipes = GameObject.FindGameObjectsWithTag("Pipe");
        foreach (GameObject pipe in pipes)
        {
            if (pipe == null) continue;
            if (OverlapsObject(pos, pipe))
            {
                Die(); return;
            }
        }

        // ── Skor bölgesi kontrolü ──
        // ── Skor bölgesi kontrolü ──
GameObject[] zones = GameObject.FindGameObjectsWithTag("ScoreZone");
foreach (GameObject zone in zones)
{
    if (zone == null) continue;
    if (OverlapsScoreZone(transform.position, zone))
    {
        FlappyGameManager.Instance.AddScore();
        zone.tag = "Untagged"; // bir kez sayılsın
    }
}
    }

    // Boru çarpışması için — SpriteRenderer bounds kullanır
    bool OverlapsObject(Vector2 birdPos, GameObject obj)
{
    SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
    if (sr == null) return false;

    Bounds b = sr.bounds;
    float closestX = Mathf.Clamp(birdPos.x, b.min.x, b.max.x);
    float closestY = Mathf.Clamp(birdPos.y, b.min.y, b.max.y);
    float dx = birdPos.x - closestX;
    float dy = birdPos.y - closestY;
    return (dx * dx + dy * dy) < (ballRadius * ballRadius);
}

// Skor bölgesi için — transform pozisyon ve scale kullanır
    bool OverlapsScoreZone(Vector2 birdPos, GameObject zone)
{
    Vector3 zonePos   = zone.transform.position;
    Vector3 zoneScale = zone.transform.localScale;

    float halfW = zoneScale.x / 2f;
    float halfH = zoneScale.y / 2f;

    return birdPos.x >= zonePos.x - halfW &&
           birdPos.x <= zonePos.x + halfW &&
           birdPos.y >= zonePos.y - halfH &&
           birdPos.y <= zonePos.y + halfH;
}

    void Die()
    {
        GameAudio.PlayGameOver();
        CameraShake.Shake(0.3f, 0.2f);
        if (isDead) return;
        isDead = true;
        FlappyGameManager.Instance.GameOver();
    }

    Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(size / 2f, size / 2f);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y,
                    Vector2.Distance(new Vector2(x, y), center) <= size/2f-1
                    ? Color.white : Color.clear);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,size,size),
                             new Vector2(0.5f,0.5f), size);
    }
}