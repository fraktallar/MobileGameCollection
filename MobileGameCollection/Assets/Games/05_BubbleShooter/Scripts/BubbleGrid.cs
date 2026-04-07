using UnityEngine;
using System.Collections.Generic;

public class BubbleGrid : MonoBehaviour
{
    public static BubbleGrid Instance;

    public  int   cols    = 9;
    public  int   rows    = 8;
    public  float bubbleR = 0.52f;
    public  float startY  = 5.5f;
    public  float startX  = -4.0f;

    // Tüm yerleşik balonlar (grid + fırlatılanlar dahil değil)
    private List<BubbleData> activeBubbles = new List<BubbleData>();

    void Awake() => Instance = this;

    void Start()
    {
    // Kameraya göre dinamik hesapla
    Camera cam = Camera.main;
    float camW = cam.orthographicSize * cam.aspect;

    cols   = 8;           // sabit sütun
    bubbleR = 0.48f;      // biraz küçült
    float diameter = bubbleR * 2f;

    // Grid'i kameraya ortala
    float totalW = cols * diameter;
    startX = -(totalW / 2f) + bubbleR;
    startY = cam.orthographicSize - 1.8f;

    SpawnGrid();
}
    void SpawnGrid()
    {
        float d = bubbleR * 2f;
        for (int r = 0; r < rows; r++)
        {
            float ox = (r % 2 == 0) ? 0 : bubbleR;
            for (int c = 0; c < cols; c++)
            {
                if (r >= 6 && c % 2 == 0) continue;
                float x = startX + ox + c * d;
                float y = startY - r * (d * 0.88f);
                int ci = Random.Range(0, BubbleGameManager.BubbleColors.Length);
                var go = CreateBubble(new Vector3(x, y, 0), ci);
                activeBubbles.Add(go.GetComponent<BubbleData>());
            }
        }
    }

    // Genel balon oluşturucu
    public GameObject CreateBubble(Vector3 pos, int colorIdx)
    {
        GameObject go = new GameObject("Bubble");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * (bubbleR * 2f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = MakeCircle();
        sr.color  = BubbleGameManager.BubbleColors[colorIdx];
        sr.sortingOrder = 3;

        BubbleData bd = go.AddComponent<BubbleData>();
        bd.colorIndex = colorIdx;
        return go;
    }

    // Fırlatılan balon bir yere çarptı
    public bool BubbleLanded(Vector3 landPos, int colorIdx)
{
    Vector3 snapped = SnapToGrid(landPos);

    var go = CreateBubble(snapped, colorIdx);
    BubbleData bd = go.GetComponent<BubbleData>();
    activeBubbles.Add(bd);

    var matches = FindMatches(bd);
    if (matches.Count >= 3)
    {
        GameAudio.PlayExplode();
        CameraShake.Shake(0.2f, 0.1f);
        foreach (var b in matches)
        {
            activeBubbles.Remove(b);
            Destroy(b.gameObject);
        }
        BubbleGameManager.Instance.AddScore(matches.Count * 10);
        BubbleGameManager.Instance.CheckWin();
        return true;
    }

    GameAudio.PlayBounce();

    if (snapped.y < -5f)
        BubbleGameManager.Instance.GameOver();

    return false;
}

    Vector3 SnapToGrid(Vector3 worldPos)
{
    float d  = bubbleR * 2f;
    float hy = d * 0.88f;

    // En yakın hücreyi bul
    int bestR = 0, bestC = 0;
    float bestDist = float.MaxValue;

    for (int r = 0; r < rows + 6; r++)
    {
        float ox = (r % 2 == 0) ? 0 : bubbleR;
        for (int c = 0; c < cols; c++)
        {
            float x = startX + ox + c * d;
            float y = startY - r * hy;
            float dist = Vector2.Distance(worldPos, new Vector2(x, y));

            if (dist < bestDist)
            {
                // Bu hücre boş mu?
                bool occupied = false;
                foreach (var bd in activeBubbles)
                {
                    if (bd == null) continue;
                    if (Vector3.Distance(bd.transform.position,
                        new Vector3(x, y, 0)) < d * 0.8f)
                    {
                        occupied = true; break;
                    }
                }
                if (!occupied)
                {
                    bestDist = dist;
                    bestR = r; bestC = c;
                }
            }
        }
    }

    float fx = startX + ((bestR % 2 == 0) ? 0 : bubbleR) + bestC * d;
    float fy = startY - bestR * hy;
    return new Vector3(fx, fy, 0);
}

    bool IsOccupied(Vector3 pos)
    {
        foreach (var b in activeBubbles)
        {
            if (b == null) continue;
            if (Vector3.Distance(b.transform.position, pos) < bubbleR * 1.2f)
                return true;
        }
        return false;
    }

    // BFS — aynı renk komşuları bul
    List<BubbleData> FindMatches(BubbleData start)
    {
        var result  = new List<BubbleData>();
        var queue   = new Queue<BubbleData>();
        var visited = new HashSet<BubbleData>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if (cur == null) continue;
            result.Add(cur);

            foreach (var nb in GetNeighbors(cur))
            {
                if (!visited.Contains(nb) &&
                    nb.colorIndex == start.colorIndex)
                {
                    visited.Add(nb);
                    queue.Enqueue(nb);
                }
            }
        }
        return result;
    }

    List<BubbleData> GetNeighbors(BubbleData bd)
    {
        var result = new List<BubbleData>();
        float threshold = bubbleR * 2.4f; // komşuluk mesafesi

        foreach (var other in activeBubbles)
        {
            if (other == null || other == bd) continue;
            float dist = Vector3.Distance(
                bd.transform.position,
                other.transform.position);
            if (dist < threshold)
                result.Add(other);
        }
        return result;
    }

    public List<BubbleData> GetAllBubbles() => activeBubbles;

    public bool IsEmpty()
    {
        activeBubbles.RemoveAll(b => b == null);
        return activeBubbles.Count == 0;
    }

    // Grid'de hangi renkler var?
public HashSet<int> GetAvailableColors()
{
    var colors = new HashSet<int>();
    foreach (var bd in activeBubbles)
    {
        if (bd != null)
            colors.Add(bd.colorIndex);
    }
    return colors;
}

    Sprite MakeCircle()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 c = Vector2.one * (size / 2f);
        float r   = size / 2f - 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y,
                    Vector2.Distance(new Vector2(x, y), c) <= r
                    ? Color.white : Color.clear);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size),
                             new Vector2(0.5f, 0.5f), size);
    }
}