using UnityEngine;

public class PipeMover : MonoBehaviour
{
    public float speed = 3.5f;
    private bool stopped = false;

    void Update()
    {
        if (stopped) return;
        transform.position += Vector3.left * speed * Time.deltaTime;

        // Ekran dışına çıkınca sil
        if (transform.position.x < -10f)
            Destroy(gameObject);
    }

    public void Stop() => stopped = true;
}