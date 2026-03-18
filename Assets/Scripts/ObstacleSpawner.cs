using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bottomRowObstaclePrefab;
    [SerializeField] private GameObject middleRowObstaclePrefab;
    [SerializeField] private GameObject topRowObstaclePrefab;
    [SerializeField] private GameObject verticalShiftObstaclePrefab;
    [SerializeField] private GameObject sideShiftObstaclePrefab;

    [SerializeField] private float baseSpawnZ = 20f;

    [Header("Spawn Distance Scaling")]
    [SerializeField] private float extraSpawnZPerSpeedMultiplier = 8f;
    [SerializeField] private float maxExtraSpawnZ = 20f;

    [Header("Spawn Timing")]
    [SerializeField] private float baseCenter = 1.2f;
    [SerializeField] private float baseHalfRange = 0.4f;
    [SerializeField] private float centerDecreasePerPoint = 0.005f;
    [SerializeField] private float rangeTightenPerPoint = 0.003f;
    [SerializeField] private float minimumCenter = 0.45f;
    [SerializeField] private float minimumHalfRange = 0.08f;

    [Header("Plane Baseline")]
    [SerializeField] private float planeBaselineY = 0f;

    [Header("Obstacle Height Layout")]
    [SerializeField] private float firstRowCenterOffset = 0.5f;
    [SerializeField] private float rowSpacing = 1.5f;

    [Header("Fairness")]
    [SerializeField] private bool alwaysLeaveOneLaneOpen = true;

    private readonly float[] laneX = { -2f, 0f, 2f };

    private float timer = 0f;
    private float currentSpawnInterval;

    private enum ObstacleType
    {
        Normal,
        VerticalShift,
        SideShift
    }

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
        float rowSpawnZ = GetCurrentSpawnZ();

        float lowY = planeBaselineY + firstRowCenterOffset;

        float[] spawnHeights =
        {
            lowY,
            lowY + rowSpacing,
            lowY + (rowSpacing * 2f)
        };

        bool[,] reserved = new bool[3, 3];

        int safeLane = -1;
        if (alwaysLeaveOneLaneOpen)
        {
            safeLane = Random.Range(0, 3);
            for (int height = 0; height < 3; height++)
            {
                reserved[safeLane, height] = true;
            }
        }

        List<Vector2Int> candidateSlots = new List<Vector2Int>();

        for (int lane = 0; lane < 3; lane++)
        {
            for (int height = 0; height < 3; height++)
            {
                if (!reserved[lane, height])
                {
                    candidateSlots.Add(new Vector2Int(lane, height));
                }
            }
        }

        ShuffleSlots(candidateSlots);

        int spawned = 0;

        for (int i = 0; i < candidateSlots.Count && spawned < obstacleCount; i++)
        {
            int laneIndex = candidateSlots[i].x;
            int heightIndex = candidateSlots[i].y;

            ObstacleType chosenType;
            if (!TryChooseObstacleType(laneIndex, heightIndex, reserved, out chosenType))
            {
                continue;
            }

            ReserveSlotsForObstacle(chosenType, laneIndex, heightIndex, reserved);

            Vector3 spawnPos = new Vector3(
                laneX[laneIndex],
                spawnHeights[heightIndex],
                rowSpawnZ
            );

            GameObject prefabToSpawn = GetPrefabForType(chosenType, heightIndex);
            if (prefabToSpawn != null)
            {
                Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
                spawned++;
            }
        }
    }

    int GetObstacleCountFromScore()
    {
        int score = 0;

        if (GameManager.Instance != null)
        {
            score = GameManager.Instance.Score;
        }

        if (score < 20)
            return 1;

        if (score < 40)
            return Random.Range(1, 3);

        if (score < 70)
            return Random.Range(2, 4);

        return Random.Range(3, 6);
    }

    bool TryChooseObstacleType(int laneIndex, int heightIndex, bool[,] reserved, out ObstacleType chosenType)
    {
        List<ObstacleType> types = new List<ObstacleType>
        {
            ObstacleType.Normal,
            ObstacleType.VerticalShift,
            ObstacleType.SideShift
        };

        ShuffleTypes(types);

        for (int i = 0; i < types.Count; i++)
        {
            if (CanPlaceObstacle(types[i], laneIndex, heightIndex, reserved))
            {
                chosenType = types[i];
                return true;
            }
        }

        chosenType = ObstacleType.Normal;
        return false;
    }

    bool CanPlaceObstacle(ObstacleType type, int laneIndex, int heightIndex, bool[,] reserved)
    {
        if (reserved[laneIndex, heightIndex])
            return false;

        if (type == ObstacleType.Normal)
        {
            return true;
        }

        if (type == ObstacleType.VerticalShift)
        {
            if (heightIndex + 1 >= 3)
                return false;

            return !reserved[laneIndex, heightIndex + 1];
        }

        if (type == ObstacleType.SideShift)
        {
            if (laneIndex + 1 >= 3)
                return false;

            return !reserved[laneIndex + 1, heightIndex];
        }

        return false;
    }

    void ReserveSlotsForObstacle(ObstacleType type, int laneIndex, int heightIndex, bool[,] reserved)
    {
        reserved[laneIndex, heightIndex] = true;

        if (type == ObstacleType.VerticalShift)
        {
            reserved[laneIndex, heightIndex + 1] = true;
        }
        else if (type == ObstacleType.SideShift)
        {
            reserved[laneIndex + 1, heightIndex] = true;
        }
    }

    GameObject GetPrefabForType(ObstacleType type, int heightIndex)
    {
        if (type == ObstacleType.VerticalShift)
            return verticalShiftObstaclePrefab;

        if (type == ObstacleType.SideShift)
            return sideShiftObstaclePrefab;

        return GetNormalObstaclePrefabForRow(heightIndex);
    }

    GameObject GetNormalObstaclePrefabForRow(int heightIndex)
    {
        if (heightIndex == 0)
            return bottomRowObstaclePrefab;

        if (heightIndex == 1)
            return middleRowObstaclePrefab;

        return topRowObstaclePrefab;
    }

    void ShuffleSlots(List<Vector2Int> slots)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            int swapIndex = Random.Range(i, slots.Count);
            Vector2Int temp = slots[i];
            slots[i] = slots[swapIndex];
            slots[swapIndex] = temp;
        }
    }

    void ShuffleTypes(List<ObstacleType> types)
    {
        for (int i = 0; i < types.Count; i++)
        {
            int swapIndex = Random.Range(i, types.Count);
            ObstacleType temp = types[i];
            types[i] = types[swapIndex];
            types[swapIndex] = temp;
        }
    }

    float GetCurrentSpawnZ()
    {
        float speedMultiplier = 1f;

        if (GameManager.Instance != null)
        {
            speedMultiplier = GameManager.Instance.SpeedMultiplier;
        }

        float extraSpawnZ = (speedMultiplier - 1f) * extraSpawnZPerSpeedMultiplier;
        extraSpawnZ = Mathf.Min(extraSpawnZ, maxExtraSpawnZ);

        return baseSpawnZ + extraSpawnZ;
    }
}