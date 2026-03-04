using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    [SerializeField] private float baseMoveSpeed = 18f;
    [SerializeField] private float destroyZ = -20f;

    void Update()
    {
        float speed = baseMoveSpeed;

        if (GameManager.Instance != null)
        {
            speed *= GameManager.Instance.SpeedMultiplier;
        }

        transform.position += Vector3.back * speed * Time.deltaTime;

        if (transform.position.z < destroyZ)
        {
            Destroy(gameObject);
        }
    }
}