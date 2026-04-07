using UnityEngine;

public class BallController : MonoBehaviour
{
    public float speed = 12f;

    private Vector2 velocity;
    private bool launched = false;
    private float camH, camW;
    private float ballRadius = 0.2f;

    void Start()
    {
        Camera cam = Camera.main;
        camH = cam.orthographicSize;
        camW = camH * cam.aspect;

        transform.position = new Vector3(0, -6.5f, 0);
        transform.localScale = new Vector3(ballRadius * 2, ballRadius * 2, 1f);

        // Görsel
        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color  = new Color(1f, 0.9f, 0.3f);
        sr.sortingOrder = 3;
    }

    void Update()
    {
        // Paddle'ı bul
        GameObject paddle = GameObject.FindWithTag("Paddle");

        if (!launched)
        {
            // Top paddle'ın üstünde beklesin
            if (paddle != null)
                transform.position = new Vector3(
                    paddle.transform.position.x,
                    paddle.transform.position.y + 0.5f, 0);

            // Başlat: Space veya dokunuş
            if (Input.GetKeyDown(KeyCode.Space) ||
                (Input.touchCount > 0 &&
                 Input.GetTouch(0).phase == TouchPhase.Began))
            {
                velocity = new Vector2(0.4f, 1f).normalized * speed;
                launched = true;
            }
            return;
        }

        // Hareket
        Vector2 pos = transform.position;
        pos += velocity * Time.deltaTime;

        // ── Sol / Sağ duvar ──
        if (pos.x - ballRadius <= -camW)
        {
            pos.x = -camW + ballRadius;
            velocity.x = Mathf.Abs(velocity.x);
        }
        else if (pos.x + ballRadius >= camW)
        {
            pos.x = camW - ballRadius;
            velocity.x = -Mathf.Abs(velocity.x);
        }

        // ── Üst duvar ──
        if (pos.y + ballRadius >= camH)
        {
            pos.y = camH - ballRadius;
            velocity.y = -Mathf.Abs(velocity.y);
        }

        // ── Alt — can kaybet ──
        if (pos.y < -camH - 1f)
        {
            BrickManager.Instance.LoseLife();
            Destroy(gameObject);
            return;
        }

        // ── Paddle çarpışması ──
        if (paddle != null)
        {
            Bounds pb = GetBounds(paddle);
            if (BallOverlapsBounds(pos, pb) && velocity.y < 0)
            {
                pos.y = pb.max.y + ballRadius;
                // Paddle'ın neresine çarptığına göre açı belirle
                float hit = (pos.x - pb.center.x) / pb.extents.x;
                velocity.x = hit * speed * 0.9f;
                velocity.y = Mathf.Abs(velocity.y);
                velocity = velocity.normalized * speed;
            }
        }

        // ── Tuğla çarpışması ──
        GameObject[] bricks = GameObject.FindGameObjectsWithTag("Brick");
        foreach (GameObject brick in bricks)
        {
            if (brick == null) continue;
            Bounds bb = GetBounds(brick);

            if (BallOverlapsBounds(pos, bb))
            {
                // Hangi yönden çarptı?
                float overlapLeft   = (pos.x + ballRadius) - bb.min.x;
                float overlapRight  = bb.max.x - (pos.x - ballRadius);
                float overlapTop    = (pos.y + ballRadius) - bb.min.y;
                float overlapBottom = bb.max.y - (pos.y - ballRadius);

                float minOverlapX = Mathf.Min(overlapLeft, overlapRight);
                float minOverlapY = Mathf.Min(overlapTop, overlapBottom);

                if (minOverlapX < minOverlapY)
                    velocity.x = -velocity.x;
                else
                    velocity.y = -velocity.y;

                // Tuğlayı yok et
                BrickData bd = brick.GetComponent<BrickData>();
                if (bd != null)
                    BrickManager.Instance.BrickDestroyed(bd.pointValue);
                Destroy(brick);
                break; // Aynı frame'de tek tuğla
            }
        }

        transform.position = new Vector3(pos.x, pos.y, 0);
    }

    // Objenin Bounds'unu SpriteRenderer'dan al
    Bounds GetBounds(GameObject obj)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null) return sr.bounds;

        // SpriteRenderer yoksa transform'dan hesapla
        return new Bounds(obj.transform.position,
                          obj.transform.localScale);
    }

    // Top (daire) ile dikdörtgen çakışıyor mu?
    bool BallOverlapsBounds(Vector2 ballPos, Bounds bounds)
    {
        // Dikdörtgene en yakın noktayı bul
        float closestX = Mathf.Clamp(ballPos.x, bounds.min.x, bounds.max.x);
        float closestY = Mathf.Clamp(ballPos.y, bounds.min.y, bounds.max.y);
        float distX = ballPos.x - closestX;
        float distY = ballPos.y - closestY;
        return (distX * distX + distY * distY) <= (ballRadius * ballRadius);
    }

    Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 1f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y,
                    Vector2.Distance(new Vector2(x, y), center) <= radius
                    ? Color.white : Color.clear);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size),
                             new Vector2(0.5f, 0.5f), size);
    }
}