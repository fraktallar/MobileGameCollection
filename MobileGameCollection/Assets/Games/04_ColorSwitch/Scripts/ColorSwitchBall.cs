using UnityEngine;

public class ColorSwitchBall : MonoBehaviour
{
    public float jumpForce  = 7f;
    public float gravity    = -14f;
    public float maxFall    = -10f;

    private float velocityY = 0f;
    private bool  isDead    = false;
    private int   colorIndex = 0;
    private float radius    = 0.22f;

    private SpriteRenderer sr;

    void Start()
    {
        transform.localScale = Vector3.one * (radius * 2f);

        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = MakeCircle();
        sr.sortingOrder = 10;

        SetColor(0);
    }

    void Update()
    {
        if (isDead) return;

        bool tapped = Input.GetKeyDown(KeyCode.Space) ||
                      (Input.touchCount > 0 &&
                       Input.GetTouch(0).phase == TouchPhase.Began);

        if (tapped)
        {
            ColorSwitchManager.Instance.StartGame();
            velocityY = jumpForce;
        }

        if (!ColorSwitchManager.Instance.IsStarted()) return;

        // Yerçekimi
        velocityY += gravity * Time.deltaTime;
        velocityY  = Mathf.Max(velocityY, maxFall);
        transform.position += Vector3.up * velocityY * Time.deltaTime;

        // Ekran altına düşerse game over
        float bottomY = Camera.main.transform.position.y - 
                        Camera.main.orthographicSize - 1f;
        if (transform.position.y < bottomY)
        {
            Die(); return;
        }

        // Çember çarpışma kontrolü
        CheckWheelCollision();
    }

    void CheckWheelCollision()
    {
        Vector2 pos = transform.position;

        foreach (var wheel in FindObjectsOfType<ColorWheel>())
        {
            if (wheel == null) continue;

            float dist = Vector2.Distance(pos,
                         wheel.transform.position);
            float outerR = wheel.OuterRadius;
            float innerR = wheel.InnerRadius;

            // Top çemberin içinde mi?
            if (dist < outerR + radius && dist > innerR - radius)
            {
                // Hangi renk diliminde?
                Color segColor = wheel.GetColorAtAngle(pos);

                if (!ColorMatch(segColor, 
                    ColorSwitchManager.GameColors[colorIndex]))
                {
                    Die();
                    return;
                }
            }

            // Çemberin tam merkezini geçtiyse skor
            if (dist < innerR - radius && !wheel.Scored)
            {
                wheel.Scored = true;
                ColorSwitchManager.Instance.AddScore();
                // Rengi değiştir
                colorIndex = (colorIndex + 1) % 
                             ColorSwitchManager.GameColors.Length;
                SetColor(colorIndex);
            }
        }
    }

    bool ColorMatch(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.15f &&
               Mathf.Abs(a.g - b.g) < 0.15f &&
               Mathf.Abs(a.b - b.b) < 0.15f;
    }

    void SetColor(int idx)
    {
        sr.color = ColorSwitchManager.GameColors[idx];
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        ColorSwitchManager.Instance.GameOver();
        gameObject.SetActive(false);
    }

    Sprite MakeCircle()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 c = Vector2.one * (size / 2f);
        float r = size / 2f - 1f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y,
                    Vector2.Distance(new Vector2(x,y), c) <= r
                    ? Color.white : Color.clear);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,size,size),
                             new Vector2(0.5f,0.5f), size);
    }
}