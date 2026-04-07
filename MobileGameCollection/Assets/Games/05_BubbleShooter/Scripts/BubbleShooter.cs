using UnityEngine;

public class BubbleShooter : MonoBehaviour
{
    private int   currentColor = 0;
    private int   nextColor    = 0;

    private GameObject firedBubble = null;
    private Vector2    velocity    = Vector2.zero;
    private bool       inFlight    = false;
    private float      speed       = 14f;
    private float      bubbleR;

    private LineRenderer aimLine;
    private GameObject  currentVisual;
    private GameObject  nextVisual;

    private Vector3 shootPos;
    private float   camW;

    void Start()
{
    bubbleR  = BubbleGrid.Instance.bubbleR;
    camW     = Camera.main.orthographicSize * Camera.main.aspect;
    shootPos = new Vector3(0, -7.2f, 0);

    currentColor = GetSmartColor();  // aktif top
    nextColor    = GetSmartColor();  // yedek top (değişmeyecek ta atışa kadar)

    SetupAimLine();
    CreateFloor();
    RefreshVisuals();
}

    void Update()
    {
        if (BubbleGameManager.Instance.IsEnded()) return;

        if (!inFlight)
        {
            UpdateAimLine();

            bool fired = Input.GetMouseButtonDown(0) ||
                         (Input.touchCount > 0 &&
                          Input.GetTouch(0).phase == TouchPhase.Began);
            if (fired) Shoot();
        }
        else
        {
            MoveFiredBubble();
        }
    }

    void Shoot()
    {
        // Nişan yönü
        Vector3 mw = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mw.z = 0;
        Vector2 dir = ((Vector2)mw - (Vector2)shootPos).normalized;

        // Aşağı atmayı engelle
        if (dir.y < 0.15f)
        {
            dir.y = 0.15f;
            dir.Normalize();
        }

        // Balonu oluştur
        firedBubble = BubbleGrid.Instance.CreateBubble(shootPos, currentColor);
        velocity    = dir * speed;
        inFlight    = true;

        // Mevcut görseli gizle
        if (currentVisual != null)
            currentVisual.SetActive(false);

        aimLine.enabled = false;
    }

    void MoveFiredBubble()
    {
        if (firedBubble == null) { inFlight = false; return; }

        Vector3 pos = firedBubble.transform.position;
        pos += (Vector3)(velocity * Time.deltaTime);

        // Sol / sağ duvar
        if (pos.x - bubbleR <= -camW)
        {
            velocity.x = Mathf.Abs(velocity.x);
            pos.x = -camW + bubbleR;
        }
        else if (pos.x + bubbleR >= camW)
        {
            velocity.x = -Mathf.Abs(velocity.x);
            pos.x = camW - bubbleR;
        }

        // Üst duvar — yapış
        if (pos.y + bubbleR >= BubbleGrid.Instance.startY + bubbleR)
        {
            firedBubble.transform.position = pos;
            Land(pos); return;
        }

        firedBubble.transform.position = pos;

        // Grid balonlarına çarpma
        foreach (var bd in BubbleGrid.Instance.GetAllBubbles())
        {
            if (bd == null) continue;
            float dist = Vector3.Distance(pos, bd.transform.position);
            if (dist < bubbleR * 2f + 0.05f)
            {
                Land(pos); return;
            }
        }
    }

    void Land(Vector3 pos)
{
    Destroy(firedBubble);
    firedBubble = null;
    inFlight    = false;

    bool popped = BubbleGrid.Instance.BubbleLanded(pos, currentColor);

    if (!popped)
        BubbleGameManager.Instance.AddScore(-5);

    // Sıradaki gelir, yeni next üretilir
    currentColor = nextColor;           // yedek şimdi aktif oldu
    nextColor    = GetSmartColor();     // yeni yedek seç

    // Aktif renk grid'de yoksa değiştir
    var available = BubbleGrid.Instance.GetAvailableColors();
    if (available.Count > 0 && !available.Contains(currentColor))
        currentColor = GetSmartColor();
    if (available.Count > 0 && !available.Contains(nextColor))
        nextColor = GetSmartColor();

    aimLine.enabled = true;
    RefreshVisuals();
}

// Eldeki renk grid'de kalmadıysa değiştir
void ValidateCurrentColor()
{
    var available = BubbleGrid.Instance.GetAvailableColors();
    if (available.Count == 0) return;

    // Mevcut renk grid'de yoksa değiştir
    if (!available.Contains(currentColor))
    {
        currentColor = GetSmartColor();
        // Görsel güncelle ama listeyi tetikleme (RefreshVisuals zaten çağrılacak)
    }

    // Sonraki renk grid'de yoksa değiştir
    if (!available.Contains(nextColor))
        nextColor = GetSmartColor();
}

// Sadece grid'deki renklerden rastgele seç
int GetSmartColor()
{
    var available = BubbleGrid.Instance.GetAvailableColors();

    if (available.Count == 0)
        return Random.Range(0, BubbleGameManager.BubbleColors.Length);

    var list = new System.Collections.Generic.List<int>(available);
    return list[Random.Range(0, list.Count)];
}

    void UpdateAimLine()
    {
        Vector3 mw = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mw.z = 0;
        Vector2 dir = ((Vector2)mw - (Vector2)shootPos).normalized;
        if (dir.y < 0.15f) { dir.y = 0.15f; dir.Normalize(); }

        aimLine.SetPosition(0, shootPos);
        aimLine.SetPosition(1, shootPos + (Vector3)(dir * 6f));
    }

    void RefreshVisuals()
    {
        if (currentVisual != null) Destroy(currentVisual);
        if (nextVisual    != null) Destroy(nextVisual);

        currentVisual = BubbleGrid.Instance.CreateBubble(
            shootPos + Vector3.up * (bubbleR + 0.1f), currentColor);

        nextVisual = BubbleGrid.Instance.CreateBubble(
            shootPos + new Vector3(-1.8f, 0.2f, 0), nextColor);
        nextVisual.transform.localScale = Vector3.one * 0.65f;
    }

    void SetupAimLine()
    {
        GameObject go = new GameObject("AimLine");
        aimLine = go.AddComponent<LineRenderer>();
        aimLine.startWidth = 0.05f;
        aimLine.endWidth   = 0.01f;
        aimLine.material   = new Material(Shader.Find("Sprites/Default"));
        aimLine.startColor = new Color(1, 1, 1, 0.7f);
        aimLine.endColor   = new Color(1, 1, 1, 0f);
        aimLine.positionCount = 2;
        aimLine.sortingOrder  = 10;
    }

    void CreateFloor()
    {
        GameObject go = new GameObject("Floor");
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.startWidth = 0.06f; lr.endWidth = 0.06f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = new Color(1, 1, 1, 0.25f);
        lr.positionCount = 2;
        lr.SetPosition(0, new Vector3(-camW, -6.8f, 0));
        lr.SetPosition(1, new Vector3( camW, -6.8f, 0));
        lr.sortingOrder = 2;
    }
}