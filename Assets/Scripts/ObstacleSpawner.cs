using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private float spawnZ = 20f;

    [Header("Spawn Timing")]
    [SerializeField] private float baseCenter = 1.2f;
    [SerializeField] private float baseHalfRange = 0.4f;
    [SerializeField] private float centerDecreasePerPoint = 0.005f;
    [SerializeField] private float rangeTightenPerPoint = 0.003f;
    [SerializeField] private float minimumCenter = 0.45f;
    [SerializeField] private float minimumHalfRange = 0.08f;

    [Header("Plane Baseline")]
    [SerializeField] private float planeBaselineY = 0f;

    [Header("Obstacle Height Offsets From Plane")]
    [SerializeField] private float lowHeightOffset = 1f;
    [SerializeField] private float highHeightOffset = 2f;
    [SerializeField] private float extraHighHeightOffset = 3f;

    private readonly float[] laneX = { -2f, 0f, 2f };

    private float timer = 0f;
    private float currentSpawnInterval;

    void Start()
    {
        SetNextSpawnInterval();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        timer += Time.deltaTime;

        if (timer >= currentSpawnInterval)
        {
            timer = 0f;
            SpawnRow();
            SetNextSpawnInterval();
        }
    }

    void SetNextSpawnInterval()
    {
        int score = 0;

        if (GameManager.Instance != null)
        {
            score = GameManager.Instance.Score;
        }

        float currentCenter = Mathf.Max(baseCenter - score * centerDecreasePerPoint, minimumCenter);
        float currentHalfRange = Mathf.Max(baseHalfRange - score * rangeTightenPerPoint, minimumHalfRange);

        float minInterval = currentCenter - currentHalfRange;
        float maxInterval = currentCenter + currentHalfRange;

        currentSpawnInterval = Random.Range(minInterval, maxInterval);
    }

    void SpawnRow()
    {
        int obstacleCount = GetObstacleCountFromScore();
        Vector2Int[] chosenSlots = GetRandomUniqueSlots(obstacleCount);

        float[] spawnHeights =
        {
            planeBaselineY + lowHeightOffset,
            planeBaselineY + highHeightOffset,
            planeBaselineY + extraHighHeightOffset
        };

        for (int i = 0; i < chosenSlots.Length; i++)
        {
            int laneIndex = chosenSlots[i].x;    // 0,1,2
            int heightIndex = chosenSlots[i].y;  // 0,1,2

            Vector3 spawnPos = new Vector3(
                laneX[laneIndex],
                spawnHeights[heightIndex],
                spawnZ
            );

            Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
        }
    }

    int GetObstacleCountFromScore()
    {
        int score = 0;

        if (GameManager.Instance != null)
        {
            score = GameManager.Instance.Score;
        }

        // Early: 1
        if (score < 20)
            return 1;

        // Mid: 1 or 2
        if (score < 40)
            return Random.Range(1, 3); // 1, 2

        // Upper-mid: 2 or 3
        if (score < 70)
            return Random.Range(2, 4); // 2, 3

        // Late: 3, 4, or 5
        return Random.Range(3, 6); // 3, 4, 5
    }

    Vector2Int[] GetRandomUniqueSlots(int count)
    {
        // 9 total slots in the 3x3 grid:
        // x = lane index (0..2)
        // y = height index (0..2)
        Vector2Int[] allSlots = new Vector2Int[]
        {
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0),
            new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1),
            new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2)
        };

        // Shuffle the 9 slots
        for (int i = 0; i < allSlots.Length; i++)
        {
            int swapIndex = Random.Range(i, allSlots.Length);
            Vector2Int temp = allSlots[i];
            allSlots[i] = allSlots[swapIndex];
            allSlots[swapIndex] = temp;
        }

        count = Mathf.Clamp(count, 1, 5);

        Vector2Int[] result = new Vector2Int[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = allSlots[i];
        }

        return result;
    }
}