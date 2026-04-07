using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    private float spawnInterval = 2.2f;
    private float timer = 1.5f; // ilk boruya kadar bekle
    private float pipeSpeed  = 3.5f;
    private float gapSize    = 2.8f; // borular arası boşluk
    private float gapMin     = -2f;
    private float gapMax     =  2f;

    void Update()
    {
        if (!FlappyGameManager.Instance.IsGameStarted()) return;
        if (FlappyGameManager.Instance.IsGameEnded())   return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnPipePair();
        }
    }

    void SpawnPipePair()
    {
        float gapY    = Random.Range(gapMin, gapMax);
        float halfGap = gapSize / 2f;

        // Alt boru
        SpawnPipe(new Vector3(7f, gapY - halfGap - 2.5f, 0),
                  new Vector3(1.2f, 5f, 1f), false);

        // Üst boru
        SpawnPipe(new Vector3(7f, gapY + halfGap + 2.5f, 0),
                  new Vector3(1.2f, 5f, 1f), true);

        // Skor bölgesi (görünmez, boruların ortasında)
        SpawnScoreZone(new Vector3(7f, gapY, 0));
    }

    void SpawnPipe(Vector3 pos, Vector3 scale, bool isTop)
{
    GameObject pipe = new GameObject("Pipe");
    pipe.tag = "Pipe";
    pipe.transform.position = pos;
    pipe.transform.localScale = scale;

    SpriteRenderer sr = pipe.AddComponent<SpriteRenderer>();
    sr.sprite = CreateSprite();
    sr.color  = new Color(0.2f, 0.75f, 0.3f);
    sr.sortingOrder = 3;

    // Collider YOK — BirdController bounds ile kontrol ediyor
    pipe.AddComponent<PipeMover>().speed = pipeSpeed;
}

    void SpawnScoreZone(Vector3 pos)
{
    GameObject zone = new GameObject("ScoreZone");
    zone.tag = "ScoreZone";
    zone.transform.position = pos;
    zone.transform.localScale = new Vector3(0.5f, gapSize, 1f);
    // Collider yok, PipeMover yeterli
    zone.AddComponent<PipeMover>().speed = pipeSpeed;
}

    Sprite CreateSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white); tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,1,1),
                             new Vector2(0.5f,0.5f), 1f);
    }
}