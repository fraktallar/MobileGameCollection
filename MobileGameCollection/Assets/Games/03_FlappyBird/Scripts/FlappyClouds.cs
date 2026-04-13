using UnityEngine;

/// <summary>Yumuşak bulutları yavaşça kaydırır, ekran dışına çıkınca sağa sarar.</summary>
public class FlappyClouds : MonoBehaviour
{
    void Awake()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float halfW = cam.orthographicSize * cam.aspect;
        for (int i = 0; i < 9; i++)
        {
            GameObject go = new GameObject("Cloud_" + i);
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3(
                UnityEngine.Random.Range(-halfW - 3f, halfW + 10f),
                UnityEngine.Random.Range(0.2f, 3.8f),
                1f);
            float sc = UnityEngine.Random.Range(0.85f, 1.55f);
            go.transform.localScale = new Vector3(sc, sc * UnityEngine.Random.Range(0.85f, 1.05f), 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = FlappyArtUtil.BuildCloudPuff(72);
            sr.color = new Color(1f, 1f, 1f, UnityEngine.Random.Range(0.82f, 0.96f));
            sr.sortingOrder = -14;

            var drift = go.AddComponent<FlappyCloudDrift>();
            drift.Init(
                UnityEngine.Random.Range(0.28f, 0.72f),
                -halfW - 5f,
                halfW + 8f);
        }
    }
}

public class FlappyCloudDrift : MonoBehaviour
{
    float speed;
    float leftX;
    float rightX;

    public void Init(float s, float left, float right)
    {
        speed = s;
        leftX = left;
        rightX = right;
    }

    void Update()
    {
        if (FlappyGameManager.Instance != null && FlappyGameManager.Instance.IsGameEnded())
            return;

        transform.position += Vector3.left * speed * Time.deltaTime;
        if (transform.position.x < leftX)
        {
            Vector3 p = transform.position;
            p.x = rightX + UnityEngine.Random.Range(0f, 4f);
            p.y += UnityEngine.Random.Range(-0.35f, 0.35f);
            p.y = Mathf.Clamp(p.y, -0.5f, 4.2f);
            transform.position = p;
        }
    }
}
