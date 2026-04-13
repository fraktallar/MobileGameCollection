using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    void Start()
    {
        SpawnFood();
    }

    public void SpawnFood()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        int xMax = Mathf.FloorToInt(cam.orthographicSize * cam.aspect) - 1;
        int yMax = Mathf.FloorToInt(cam.orthographicSize) - 1;

        // Yılanın kapladığı pozisyonları topla
        var occupied = new System.Collections.Generic.HashSet<Vector2Int>();
        var head = FindObjectOfType<SnakeHead>();
        if (head != null)
            occupied.Add(Vector2Int.RoundToInt(head.transform.position));
        foreach (var part in GameObject.FindGameObjectsWithTag("BodyPart"))
            occupied.Add(Vector2Int.RoundToInt(part.transform.position));

        // Boş hücre bulana kadar dene (max 200 deneme)
        int x = 0, y = 0;
        for (int attempt = 0; attempt < 200; attempt++)
        {
            x = Random.Range(-xMax, xMax + 1);
            y = Random.Range(-yMax, yMax + 1);
            if (!occupied.Contains(new Vector2Int(x, y))) break;
        }

        GameObject food = new GameObject("Food");
        food.transform.position = new Vector3(x, y, 0);
        food.tag = "Food";

        // Görsel
        SpriteRenderer sr = food.AddComponent<SpriteRenderer>();
        sr.sprite = CreateAppleSprite();
        sr.sortingOrder = 1;

        // Collider
        BoxCollider2D col = food.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.9f, 0.9f);
        col.isTrigger = true;
    }

    Sprite CreateAppleSprite()
    {
        int sz = 64;
        var tex = new Texture2D(sz, sz) { filterMode = FilterMode.Bilinear };
        for (int y = 0; y < sz; y++)
        for (int x = 0; x < sz; x++)
            tex.SetPixel(x, y, Color.clear);

        // Elma gövdesi: kırmızı oval (hafif yukarı kaymış)
        Color red      = new Color(0.88f, 0.10f, 0.10f);
        Color darkRed  = new Color(0.60f, 0.05f, 0.05f);
        Color highlight = new Color(1.00f, 0.60f, 0.60f);
        for (int y = 6; y < 56; y++)
        for (int x = 4; x < 60; x++)
        {
            float nx = (x - 32f) / 25f;
            float ny = (y - 28f) / 22f;
            if (nx * nx + ny * ny > 1f) continue;
            // Işık: sol üst → açık, sağ alt → koyu
            float t = Mathf.Clamp01(nx * 0.35f + ny * 0.25f + 0.5f);
            tex.SetPixel(x, y, Color.Lerp(highlight, darkRed, t));
        }

        // Üst girinti (elmanın karakteristik çukuru)
        for (int y = 36; y < 54; y++)
        for (int x = 24; x < 40; x++)
        {
            float nx = (x - 32f) / 7f, ny = (y - 47f) / 5f;
            if (nx * nx + ny * ny < 1f)
                tex.SetPixel(x, y, Color.clear);
        }

        // Sap: kahverengi dikdörtgen
        Color stem = new Color(0.38f, 0.22f, 0.08f);
        for (int y = 47; y < 58; y++)
        for (int x = 29; x < 33; x++)
            tex.SetPixel(x, y, stem);

        // Yaprak: yeşil oval
        Color leaf = new Color(0.18f, 0.68f, 0.18f);
        for (int y = 46; y < 58; y++)
        for (int x = 30; x < 50; x++)
        {
            float nx = (x - 40f) / 9f, ny = (y - 52f) / 5f;
            if (nx * nx + ny * ny < 1f)
                tex.SetPixel(x, y, leaf);
        }

        // Parlaklık noktası: sol üst
        Color shine = new Color(1f, 0.85f, 0.85f, 0.85f);
        for (int y = 38; y < 48; y++)
        for (int x = 14; x < 24; x++)
        {
            float nx = (x - 19f) / 5f, ny = (y - 43f) / 4f;
            if (nx * nx + ny * ny < 1f)
                tex.SetPixel(x, y, Color.Lerp(tex.GetPixel(x, y), shine,
                    1f - Mathf.Sqrt(nx * nx + ny * ny)));
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), sz);
    }
}