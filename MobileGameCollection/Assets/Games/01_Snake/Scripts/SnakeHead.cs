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

    // Swipe
    private Vector2 touchStartPos;
    private const float SWIPE_THRESHOLD = 50f;

    void Start()
    {
        moveInterval = SnakeGameManager.Instance.GetMoveInterval();
        transform.position = Vector3.zero;

        // Head'e görsel + collider ekle (Inspector'a gerek yok)
        SetupVisual(gameObject, new Color(0.29f, 0.87f, 0.5f)); // yeşil
        SetupCollider(gameObject, true);

        // Başlangıç body'leri
        for (int i = 0; i < initialSize - 1; i++)
            SpawnBodyPart();
    }

    // Sprite'ı koddan oluştur
    void SetupVisual(GameObject obj, Color color)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = color;
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

    // 16x16 beyaz kare sprite oluştur
    Sprite CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(16, 16);
        Color[] pixels = new Color[16 * 16];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex,
            new Rect(0, 0, 16, 16),
            new Vector2(0.5f, 0.5f),
            16f);
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
            touchStartPos = touch.position;

        if (touch.phase == TouchPhase.Ended)
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
        }
    }

    void Move()
    {
        direction = nextDirection;
        positionHistory.Insert(0, transform.position);

        Vector3 newPos = transform.position + (Vector3)direction;

        // Sınır kontrolü
        if (newPos.x > 10 || newPos.x < -10 || newPos.y > 10 || newPos.y < -10)
        {
            Die(); return;
        }

        // Kendine çarpma (ilk 2 parçayı atla)
        for (int i = 2; i < bodyParts.Count; i++)
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

    void SpawnBodyPart()
    {
        GameObject part = new GameObject("BodyPart_" + bodyParts.Count);

        // Renk: head koyu yeşil, geri kalanlar biraz daha açık
        Color bodyColor = bodyParts.Count == 0
            ? new Color(0.13f, 0.77f, 0.37f)
            : new Color(0.18f, 0.65f, 0.32f);

        SetupVisual(part, bodyColor);

        // Başlangıçta head'in soluna diz
        part.transform.position = new Vector3(
            transform.position.x - (bodyParts.Count + 1),
            transform.position.y, 0);

        bodyParts.Add(part);
    }

    public void GrowBody()
    {
        GameObject part = new GameObject("BodyPart_" + bodyParts.Count);
        SetupVisual(part, new Color(0.18f, 0.65f, 0.32f));

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
            SnakeGameManager.Instance.AddScore(10);
            Destroy(other.gameObject);
            FindObjectOfType<FoodSpawner>().SpawnFood();
        }
    }
}