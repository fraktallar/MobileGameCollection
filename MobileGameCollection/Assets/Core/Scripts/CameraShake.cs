using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 originalPos;
    private bool    isShaking = false;

    void Awake()
    {
        Instance = this;
        originalPos = transform.localPosition;
    }

    // Kolay çağrı — her yerden kullanılabilir
    public static void Shake(float duration = 0.2f,
                              float magnitude = 0.15f)
    {
        if (Instance != null && !Instance.isShaking)
            Instance.StartCoroutine(
                Instance.DoShake(duration, magnitude));
    }

    IEnumerator DoShake(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float strength = Mathf.Lerp(magnitude, 0f,
                                        elapsed / duration);
            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;

            transform.localPosition = originalPos +
                                      new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        isShaking = false;
    }

    // Kamera hareket ettiğinde originalPos'u güncelle
    public void UpdateOrigin()
    {
        if (!isShaking)
            originalPos = transform.localPosition;
    }
}