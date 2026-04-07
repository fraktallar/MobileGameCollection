using UnityEngine;

public class PaddleController : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 12f;
    public float halfWidth = 1.5f;

    private float leftBound;
    private float rightBound;
    private SpriteRenderer sr;

    void Start()
{
    Camera cam = Camera.main;
    float camW = cam.orthographicSize * cam.aspect;
    leftBound  = -camW + halfWidth + 0.3f;
    rightBound =  camW - halfWidth - 0.3f;

    transform.position = new Vector3(0, -8f, 0);
    transform.localScale = new Vector3(halfWidth * 2, 0.4f, 1f);

    // Sadece görsel — collider'a gerek yok (top bounds kontrolü yapıyor)
    SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
    sr.sprite = CreateSprite();
    sr.color  = new Color(0.4f, 0.6f, 1f);
    sr.sortingOrder = 2;
}

    void Update()
    {
        float move = 0f;

        // Klavye
        if (Input.GetKey(KeyCode.LeftArrow))  move = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) move =  1f;

        // Dokunmatik — sol yarı sol, sağ yarı sağ
        if (Input.touchCount > 0)
        {
            float screenMid = Screen.width / 2f;
            foreach (Touch t in Input.touches)
            {
                if (t.phase == TouchPhase.Began ||
                    t.phase == TouchPhase.Moved ||
                    t.phase == TouchPhase.Stationary)
                {
                    move += t.position.x < screenMid ? -1f : 1f;
                }
            }
            move = Mathf.Clamp(move, -1f, 1f);
        }

        float newX = transform.position.x + move * speed * Time.deltaTime;
        newX = Mathf.Clamp(newX, leftBound, rightBound);
        transform.position = new Vector3(newX, transform.position.y, 0);
    }

    Sprite CreateSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), 1f);
    }
}