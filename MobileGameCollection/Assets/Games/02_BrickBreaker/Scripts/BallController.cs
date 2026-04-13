using UnityEngine;

public class BallController : MonoBehaviour
{
    public float speed = 12f;

    private Vector2 velocity;
    private bool launched = false;
    private float camH, camW;
    private const float ballRadius = 0.2f;
    private GameObject paddleCache;

    private bool pendingLaunch;
    private Vector3 pendingPos;
    private Vector2 pendingVel;

    /// <summary>Call immediately after AddComponent, before Start runs.</summary>
    public void ConfigureImmediateLaunch(Vector3 worldPos, Vector2 direction)
    {
        pendingLaunch = true;
        pendingPos = worldPos;
        pendingVel = direction.sqrMagnitude > 0.0001f ? direction.normalized : new Vector2(0.15f, 1f).normalized;
    }

    void Start()
    {
        Camera cam = Camera.main;
        camH = cam.orthographicSize;
        camW = camH * cam.aspect;

        transform.localScale = new Vector3(ballRadius * 2, ballRadius * 2, 1f);

        paddleCache = GameObject.FindWithTag("Paddle");

        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(1f, 0.9f, 0.3f);
        sr.sortingOrder = 3;

        if (pendingLaunch)
        {
            transform.position = pendingPos;
            float s = EffectiveSpeed();
            velocity = pendingVel * s;
            launched = true;
            pendingLaunch = false;
        }
        else
            transform.position = new Vector3(0, -6.5f, 0);

        if (BrickManager.Instance != null)
            BrickManager.Instance.RegisterBall(gameObject);
    }

    float EffectiveSpeed()
    {
        if (BrickManager.Instance != null)
            return BrickManager.Instance.GetBallSpeed();
        return speed;
    }

    public bool IsLaunched => launched;
    public Vector2 CurrentVelocity => velocity;

    void Update()
    {
        GameObject paddle = paddleCache;

        if (!launched)
        {
            if (paddle != null)
                transform.position = new Vector3(
                    paddle.transform.position.x,
                    paddle.transform.position.y + 0.5f, 0);

            if (Input.GetKeyDown(KeyCode.Space) ||
                (Input.touchCount > 0 &&
                 Input.GetTouch(0).phase == TouchPhase.Began))
            {
                velocity = new Vector2(0.4f, 1f).normalized * EffectiveSpeed();
                launched = true;
            }
            return;
        }

        float spd = EffectiveSpeed();
        if (velocity.sqrMagnitude > 0.0001f)
            velocity = velocity.normalized * spd;

        Vector2 pos = transform.position;
        pos += velocity * Time.deltaTime;

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

        if (pos.y + ballRadius >= camH)
        {
            pos.y = camH - ballRadius;
            velocity.y = -Mathf.Abs(velocity.y);
        }

        if (pos.y < -camH - 1f)
        {
            if (BrickManager.Instance != null)
                BrickManager.Instance.NotifyBallLost(gameObject);
            Destroy(gameObject);
            return;
        }

        if (paddle != null)
        {
            Bounds pb = GetBounds(paddle);
            if (BallOverlapsBounds(pos, pb) && velocity.y < 0)
            {
                pos.y = pb.max.y + ballRadius;
                float hit = (pos.x - pb.center.x) / Mathf.Max(0.01f, pb.extents.x);
                velocity.x = hit * spd * 0.9f;
                velocity.y = Mathf.Abs(velocity.y);
                velocity = velocity.normalized * spd;
            }
        }

        GameObject[] bricks = GameObject.FindGameObjectsWithTag("Brick");
        GameObject hitBrick = null;
        Bounds hitBounds = default;
        float hitDistSq = float.MaxValue;

        foreach (GameObject brick in bricks)
        {
            if (brick == null) continue;
            Bounds bb = GetBounds(brick);
            if (!BallOverlapsBounds(pos, bb)) continue;
            float d = Vector2.SqrMagnitude((Vector2)brick.transform.position - pos);
            if (d < hitDistSq) { hitDistSq = d; hitBrick = brick; hitBounds = bb; }
        }

        if (hitBrick != null)
        {
            float overlapLeft = (pos.x + ballRadius) - hitBounds.min.x;
            float overlapRight = hitBounds.max.x - (pos.x - ballRadius);
            float overlapTop = (pos.y + ballRadius) - hitBounds.min.y;
            float overlapBottom = hitBounds.max.y - (pos.y - ballRadius);

            if (Mathf.Min(overlapLeft, overlapRight) < Mathf.Min(overlapTop, overlapBottom))
                velocity.x = -velocity.x;
            else
                velocity.y = -velocity.y;

            BrickData bd = hitBrick.GetComponent<BrickData>();
            if (bd != null && bd.TakeDamage())
            {
                BrickPowerUp drop = bd.powerUp;
                Vector3 brickPos = hitBrick.transform.position;
                BrickManager.Instance.BrickDestroyed(bd.pointValue);
                if (drop != BrickPowerUp.None)
                    BrickManager.Instance.DropPowerUp(brickPos, drop);
                Destroy(hitBrick);
            }
        }

        transform.position = new Vector3(pos.x, pos.y, 0);
    }

    Bounds GetBounds(GameObject obj)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null) return sr.bounds;
        return new Bounds(obj.transform.position, obj.transform.localScale);
    }

    bool BallOverlapsBounds(Vector2 ballPos, Bounds bounds)
    {
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
