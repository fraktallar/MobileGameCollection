using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    private float spawnInterval = 2.2f;
    private float timer = 1.5f;
    private float pipeSpeed = 3.5f;
    private float gapSize = 2.38f;
    private float gapMin = -2f;
    private float gapMax = 2f;

    static Sprite s_pipeBody;
    static Sprite s_pipeCap;

    static void EnsurePipeSprites()
    {
        if (s_pipeBody == null) s_pipeBody = FlappyArtUtil.BuildRoundedPipeBody();
        if (s_pipeCap == null) s_pipeCap = FlappyArtUtil.BuildPipeCapSprite();
    }

    void Update()
    {
        if (!FlappyGameManager.Instance.IsGameStarted()) return;
        if (FlappyGameManager.Instance.IsGameEnded()) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnPipePair();
        }
    }

    void SpawnPipePair()
    {
        float gapY = Random.Range(gapMin, gapMax);
        float halfGap = gapSize / 2f;
        const float bodyHalfH = 2.5f;

        SpawnPipe(new Vector3(7f, gapY - halfGap - bodyHalfH, 0), false);
        SpawnPipe(new Vector3(7f, gapY + halfGap + bodyHalfH, 0), true);
        SpawnScoreZone(new Vector3(7f, gapY, 0));
    }

    void SpawnPipe(Vector3 pos, bool isTop)
    {
        EnsurePipeSprites();

        GameObject pipe = new GameObject("Pipe");
        pipe.tag = "Pipe";
        pipe.transform.position = pos;
        pipe.transform.localScale = Vector3.one;

        Color body = new Color(0.16f, 0.62f, 0.2f);
        Color capC = new Color(0.22f, 0.78f, 0.26f);
        Color rim = new Color(0.32f, 0.9f, 0.34f);

        SpriteRenderer sr = pipe.AddComponent<SpriteRenderer>();
        sr.sprite = s_pipeBody;
        sr.color = body;
        sr.sortingOrder = 3;

        GameObject cap = new GameObject("Cap");
        cap.transform.SetParent(pipe.transform, false);
        const float bodyHalf = 2.5f;
        const float capWorldH = 0.36f;
        float capHalf = capWorldH * 0.5f;
        cap.transform.localPosition = new Vector3(0, isTop ? -bodyHalf - capHalf : bodyHalf + capHalf, 0);
        cap.transform.localScale = new Vector3(1f, isTop ? -1f : 1f, 1f);

        SpriteRenderer capSR = cap.AddComponent<SpriteRenderer>();
        capSR.sprite = s_pipeCap;
        capSR.color = Color.Lerp(capC, rim, 0.35f);
        capSR.sortingOrder = 4;

        pipe.AddComponent<PipeMover>().speed = pipeSpeed;
    }

    void SpawnScoreZone(Vector3 pos)
    {
        GameObject zone = new GameObject("ScoreZone");
        zone.tag = "ScoreZone";
        zone.transform.position = pos;
        zone.transform.localScale = new Vector3(0.5f, gapSize, 1f);
        zone.AddComponent<PipeMover>().speed = pipeSpeed;
    }
}
