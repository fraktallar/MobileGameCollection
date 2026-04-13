using UnityEngine;

public enum BrickPowerUp
{
    None = 0,
    WidePaddle = 1,
    MultiBall = 2,
    SlowBall = 3,
}

public class BrickData : MonoBehaviour
{
    public int pointValue = 10;
    public int hp = 1;
    public BrickPowerUp powerUp = BrickPowerUp.None;

    private Color baseColor;
    private SpriteRenderer sr;

    public void Init(int health, Color color, BrickPowerUp drop = BrickPowerUp.None)
    {
        hp        = health;
        powerUp   = drop;
        baseColor = color;
        sr        = GetComponent<SpriteRenderer>();
        sr.color  = color;
    }

    // true = yok edildi, false = sadece hasar aldı
    public bool TakeDamage()
    {
        hp--;
        if (hp <= 0) return true;
        // Hasar aldı — rengi koyulaştır
        if (sr != null) sr.color = baseColor * 0.55f;
        return false;
    }
}
