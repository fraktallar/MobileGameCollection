using UnityEngine;

/// <summary>Flappy Bird için procedural sprite üretimi (tek sefer, runtime).</summary>
public static class FlappyArtUtil
{
    public static readonly Color SkyTop = new Color(0.42f, 0.78f, 1f);
    public static readonly Color SkyBottom = new Color(0.58f, 0.88f, 1f);
    public static readonly Color HorizonTint = new Color(0.72f, 0.9f, 1f);

    public static Sprite BuildSkyGradient(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < h; y++)
        {
            float t = y / (float)(h - 1);
            Color row = Color.Lerp(SkyBottom, SkyTop, t);
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, row);
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 1f);
    }

    public static Sprite BuildGrassStrip(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Bilinear;
        var rng = new System.Random(42);
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                float ty = y / (float)h;
                Color baseC = Color.Lerp(
                    new Color(0.12f, 0.38f, 0.12f),
                    new Color(0.28f, 0.72f, 0.22f),
                    Mathf.Pow(ty, 0.65f));
                if (ty > 0.55f && rng.NextDouble() < 0.35)
                    baseC = Color.Lerp(baseC, new Color(0.35f, 0.82f, 0.18f), 0.45f);
                if (ty > 0.75f && rng.NextDouble() < 0.2f)
                    baseC *= 1.08f;
                tex.SetPixel(x, y, baseC);
            }
        }
        for (int i = 0; i < w * 2; i++)
        {
            int x = rng.Next(0, w);
            int gy = rng.Next(h * 6 / 10, h);
            int blade = rng.Next(2, 5);
            for (int k = 0; k < blade && gy + k < h; k++)
            {
                Color c = tex.GetPixel(x, gy + k);
                tex.SetPixel(x, gy + k, Color.Lerp(c, new Color(0.45f, 0.9f, 0.25f), 0.35f));
            }
        }
        int dirtH = Mathf.Max(2, h / 10);
        for (int x = 0; x < w; x++)
            for (int y = 0; y < dirtH; y++)
            {
                float t = y / (float)dirtH;
                tex.SetPixel(x, y, Color.Lerp(new Color(0.22f, 0.14f, 0.08f),
                    new Color(0.32f, 0.22f, 0.12f), t));
            }
        tex.Apply();
        float grassWorldH = 2.28f;
        float ppu = h / grassWorldH;
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 1f), ppu);
    }

    public static Sprite BuildCloudPuff(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, Color.clear);

        Vector2 c = new Vector2(size / 2f, size / 2f);
        void Blob(Vector2 o, float rad, float peak)
        {
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), c + o);
                    if (d >= rad) continue;
                    float wgt = peak * (1f - d / rad) * (1f - d / rad);
                    Color cur = tex.GetPixel(x, y);
                    float a = Mathf.Clamp01(cur.a + wgt);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
        }
        Blob(Vector2.zero, size * 0.24f, 0.7f);
        Blob(new Vector2(-size * 0.14f, 0.02f * size), size * 0.17f, 0.55f);
        Blob(new Vector2(size * 0.15f, 0.02f * size), size * 0.16f, 0.52f);
        Blob(new Vector2(0f, -0.07f * size), size * 0.15f, 0.48f);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / 2.2f);
    }

    /// <summary>Dünya boyutu ~1.2 x 5, köşeleri yuvarlatılmış boru gövdesi.</summary>
    public static Sprite BuildRoundedPipeBody()
    {
        const int pw = 48;
        const int ph = 200;
        const int r = 11;
        Texture2D tex = new Texture2D(pw, ph);
        tex.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < ph; y++)
            for (int x = 0; x < pw; x++)
                tex.SetPixel(x, y, InRoundedRect(x, y, pw, ph, r) ? Color.white : Color.clear);
        tex.Apply();
        const float ppu = ph / 5f;
        return Sprite.Create(tex, new Rect(0, 0, pw, ph), new Vector2(0.5f, 0.5f), ppu);
    }

    static bool InRoundedRect(int x, int y, int w, int h, int rad)
    {
        if (x < 0 || x >= w || y < 0 || y >= h) return false;
        if (x >= rad && x < w - rad) return true;
        if (y >= rad && y < h - rad) return true;
        Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
        float rr = rad + 0.35f;
        if (x < rad && y < rad)
            return Vector2.Distance(p, new Vector2(rad - 0.5f, rad - 0.5f)) <= rr;
        if (x >= w - rad && y < rad)
            return Vector2.Distance(p, new Vector2(w - rad - 0.5f, rad - 0.5f)) <= rr;
        if (x < rad && y >= h - rad)
            return Vector2.Distance(p, new Vector2(rad - 0.5f, h - rad - 0.5f)) <= rr;
        if (x >= w - rad && y >= h - rad)
            return Vector2.Distance(p, new Vector2(w - rad - 0.5f, h - rad - 0.5f)) <= rr;
        return false;
    }

    public static Sprite BuildPipeCapSprite()
    {
        const int w = 66;
        const int h = 16;
        const int r = 6;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, InRoundedRect(x, y, w, h, r) ? Color.white : Color.clear);
        tex.Apply();
        const float capWorldH = 0.36f;
        float ppu = h / capWorldH;
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
    }
}
