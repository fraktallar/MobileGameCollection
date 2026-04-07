using UnityEngine;

public class BrickData : MonoBehaviour
{
    public int pointValue = 10;

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.GetComponent<BallController>() != null)
        {
            BrickManager.Instance.BrickDestroyed(pointValue);
            Destroy(gameObject);
        }
    }
}