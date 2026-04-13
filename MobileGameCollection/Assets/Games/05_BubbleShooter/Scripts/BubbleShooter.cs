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
    private float   ceilingY;
    private const float WallBounce = 0.97f;

    void Start()
    {
        bubbleR = BubbleGrid.Instance.bubbleR;
        shootPos = new Vector3(0, -7.2f, 0);
        ceilingY = BubbleGrid.Instance.startY + bubbleR * 0.35f;

        currentColor = GetSmartColor();
        nextColor    = GetSmartColor();

        SetupAimLine();
        CreateFloor();
        RefreshVisuals();
    }

    void Update()
    {
        if (BubbleGameManager.Instance.IsEnded()) return;
        if (PauseManager.Instance != null && PauseManager.Instance.IsInputBlocked) return;

        if (!inFlight)
        {
            UpdateAimLine();

            bool fired = Input.GetMouseButtonDown(0) ||
                         (Input.touchCount > 0 &&
                          Input.GetTouch(0).phase == TouchPhase.Began);
            if (fired) Shoot();
        }
        else
            MoveFiredBubble();
    }

    void Shoot()
    {
        Vector3 mw = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mw.z = 0;
        Vector2 dir = ((Vector2)mw - (Vector2)shootPos).normalized;

        if (dir.y < 0.15f)
        {
            dir.y = 0.15f;
            dir.Normalize();
        }

        firedBubble = BubbleGrid.Instance.CreateBubble(shootPos, currentColor);
        velocity    = dir * speed;
        inFlight    = true;

        if (currentVisual != null)
            currentVisual.SetActive(false);

        aimLine.enabled = false;
    }

    void MoveFiredBubble()
    {
        if (firedBubble == null) { inFlight = false; return; }

        float dt = Time.deltaTime;
        int substeps = Mathf.Clamp(
            Mathf.CeilToInt(velocity.magnitude * dt / (bubbleR * 0.45f)),
            1, 8);
        float sdt = dt / substeps;

        for (int s = 0; s < substeps; s++)
        {
            Vector3 pos = firedBubble.transform.position;
            pos += (Vector3)(velocity * sdt);

            if (pos.x - bubbleR < BubbleGrid.Instance.PlayfieldMinX)
            {
                pos.x = BubbleGrid.Instance.PlayfieldMinX + bubbleR;
                velocity.x = -velocity.x * WallBounce;
            }
            else if (pos.x + bubbleR > BubbleGrid.Instance.PlayfieldMaxX)
            {
                pos.x = BubbleGrid.Instance.PlayfieldMaxX - bubbleR;
                velocity.x = -velocity.x * WallBounce;
            }

            if (pos.y + bubbleR >= ceilingY)
            {
                firedBubble.transform.position = pos;
                Land(pos);
                return;
            }

            firedBubble.transform.position = pos;

            foreach (var bd in BubbleGrid.Instance.GetAllBubbles())
            {
                if (bd == null) continue;
                float dist = Vector3.Distance(pos, bd.transform.position);
                float hitR = bubbleR * 2f + 0.04f;
                if (dist < hitR)
                {
                    Vector2 away = ((Vector2)pos - (Vector2)bd.transform.position);
                    if (away.sqrMagnitude > 0.0001f)
                        pos += (Vector3)(away.normalized * (hitR - dist) * 0.35f);
                    firedBubble.transform.position = pos;
                    Land(pos);
                    return;
                }
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

        currentColor = nextColor;
        nextColor    = GetSmartColor();

        var available = BubbleGrid.Instance.GetAvailableColors();
        if (available.Count > 0 && !available.Contains(currentColor))
            currentColor = GetSmartColor();
        if (available.Count > 0 && !available.Contains(nextColor))
            nextColor = GetSmartColor();

        aimLine.enabled = true;
        RefreshVisuals();
    }

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
        currentVisual.transform.SetParent(transform);

        nextVisual = BubbleGrid.Instance.CreateBubble(
            shootPos + new Vector3(-1.8f, 0.2f, 0), nextColor);
        nextVisual.transform.localScale = Vector3.one * 0.65f;
        nextVisual.transform.SetParent(transform);
    }

    void SetupAimLine()
    {
        GameObject go = new GameObject("AimLine");
        go.transform.SetParent(transform);
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
        float camW = Camera.main.orthographicSize * Camera.main.aspect;
        GameObject go = new GameObject("Floor");
        go.transform.SetParent(transform);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.startWidth = 0.06f; lr.endWidth = 0.06f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = new Color(1, 1, 1, 0.25f);
        lr.positionCount = 2;
        lr.SetPosition(0, new Vector3(BubbleGrid.Instance.PlayfieldMinX, -6.8f, 0));
        lr.SetPosition(1, new Vector3(BubbleGrid.Instance.PlayfieldMaxX, -6.8f, 0));
        lr.sortingOrder = 2;
    }
}
