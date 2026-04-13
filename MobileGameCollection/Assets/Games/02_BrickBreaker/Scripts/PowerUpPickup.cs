using TMPro;
using UnityEngine;

public class PowerUpPickup : MonoBehaviour
{
    private BrickPowerUp powerType;
    private float fallSpeed = 3.2f;
    private float camH;

    public void Init(BrickPowerUp type)
    {
        powerType = type;
        Camera cam = Camera.main;
        camH = cam != null ? cam.orthographicSize : 10f;

        transform.localScale = Vector3.one;

        GameObject back = new GameObject("Backdrop");
        back.transform.SetParent(transform, false);
        back.transform.localPosition = Vector3.zero;
        SpriteRenderer plate = back.AddComponent<SpriteRenderer>();
        plate.sprite = CreateCircleSprite();
        plate.color = new Color(0f, 0f, 0f, 0.55f);
        plate.sortingOrder = 4;
        back.transform.localScale = new Vector3(0.52f, 0.52f, 1f);

        TextMeshPro tmp = gameObject.AddComponent<TextMeshPro>();
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font == null)
            font = TMP_Settings.defaultFontAsset;
        if (font != null)
        {
            tmp.font = font;
            if (font.material != null)
                tmp.fontSharedMaterial = font.material;
        }

        tmp.text = SymbolFor(type);
        tmp.fontSize = type == BrickPowerUp.MultiBall ? 3.4f : 4.1f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = ColorForType(type);
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.fontStyle = FontStyles.Bold;
        tmp.outlineWidth = 0.22f;
        tmp.outlineColor = new Color32(0, 0, 0, 220);
        tmp.sortingOrder = 5;
        tmp.rectTransform.sizeDelta = new Vector2(3.2f, 1.25f);
        tmp.ForceMeshUpdate();
    }

    void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        if (transform.position.y < -camH - 1.5f)
        {
            Destroy(gameObject);
            return;
        }

        if (BrickManager.Instance == null) return;
        Bounds pb = BrickManager.Instance.GetPaddleWorldBounds();
        Vector2 p = transform.position;
        if (p.x >= pb.min.x && p.x <= pb.max.x && p.y >= pb.min.y && p.y <= pb.max.y)
        {
            BrickManager.Instance.ApplyPowerUp(powerType);
            Destroy(gameObject);
        }
    }

    /// <summary>Yalnızca temel Latin — SDF atlasında olmayan Unicode kare (tofu) üretmesin.</summary>
    static string SymbolFor(BrickPowerUp t)
    {
        switch (t)
        {
            case BrickPowerUp.WidePaddle: return "<->";
            case BrickPowerUp.MultiBall: return "x2";
            case BrickPowerUp.SlowBall: return "~~";
            default: return "?";
        }
    }

    static Color ColorForType(BrickPowerUp t)
    {
        switch (t)
        {
            case BrickPowerUp.WidePaddle: return new Color(0.55f, 0.95f, 1f);
            case BrickPowerUp.MultiBall: return new Color(1f, 0.72f, 0.35f);
            case BrickPowerUp.SlowBall: return new Color(0.78f, 0.62f, 1f);
            default: return Color.white;
        }
    }

    static Sprite CreateCircleSprite()
    {
        int size = 48;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 c = new Vector2(size / 2f, size / 2f);
        float r = size / 2f - 1f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y,
                    Vector2.Distance(new Vector2(x, y), c) <= r ? Color.white : Color.clear);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
