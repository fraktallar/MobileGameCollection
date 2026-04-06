using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    private int gridMin = -9;
    private int gridMax = 9;

    void Start()
    {
        SpawnFood();
    }

    public void SpawnFood()
    {
        int x = Random.Range(gridMin, gridMax + 1);
        int y = Random.Range(gridMin, gridMax + 1);

        GameObject food = new GameObject("Food");
        food.transform.position = new Vector3(x, y, 0);
        food.tag = "Food";

        // Görsel
        SpriteRenderer sr = food.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = new Color(0.97f, 0.44f, 0.44f); // kırmızı
        sr.sortingOrder = 1;

        // Collider
        BoxCollider2D col = food.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.9f, 0.9f);
        col.isTrigger = true;
    }

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
            new Vector2(0.5f, 0.5f), 16f);
    }
}