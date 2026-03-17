using UnityEngine;

public class ObstacleShift : MonoBehaviour
{
    public enum ShiftAxis
    {
        X,
        Y
    }

    [SerializeField] private ShiftAxis axis = ShiftAxis.Y;
    [SerializeField] private float shiftAmount = 1f;
    [SerializeField] private float shiftSpeed = 2f;

    private Vector3 startPosition;
    private float moveTimer = 0f;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        moveTimer += Time.deltaTime * shiftSpeed;
        float offset = Mathf.PingPong(moveTimer, shiftAmount);

        Vector3 pos = transform.position;

        if (axis == ShiftAxis.X)
        {
            pos.x = startPosition.x + offset;
        }
        else
        {
            pos.y = startPosition.y + offset;
        }

        transform.position = pos;
    }
}