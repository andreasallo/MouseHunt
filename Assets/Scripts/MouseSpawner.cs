using System.Collections.Generic;
using UnityEngine;

public class MouseSpawner : MonoBehaviour
{
    [Header("Mouse Prefab")]
    [SerializeField] private GameObject mousePrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Settings")]
    [SerializeField] private int maxMiceAlive = 3;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private bool spawnOnStart = true;

    [Header("Difficulty Scaling")]
    [SerializeField] private bool increaseDifficultyOverTime = true;
    [SerializeField] private float difficultyIncreaseInterval = 20f;
    [SerializeField] private int maxMiceLimit = 8;
    [SerializeField] private float minimumSpawnInterval = 1.5f;

    private readonly List<GameObject> aliveMice = new List<GameObject>();

    private float spawnTimer;
    private float difficultyTimer;

    private void Start()
    {
        spawnTimer = spawnInterval;
        difficultyTimer = difficultyIncreaseInterval;

        if (spawnOnStart)
        {
            SpawnMouse();
        }
    }

    private void Update()
    {
        RemoveDestroyedMice();

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            TrySpawnMouse();
            spawnTimer = spawnInterval;
        }

        if (increaseDifficultyOverTime)
        {
            UpdateDifficulty();
        }
    }

    private void TrySpawnMouse()
    {
        if (aliveMice.Count >= maxMiceAlive)
        {
            return;
        }

        SpawnMouse();
    }

    private void SpawnMouse()
    {
        if (mousePrefab == null)
        {
            Debug.LogWarning("MouseSpawner: Mouse Prefab is missing.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("MouseSpawner: No spawn points assigned.");
            return;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject newMouse = Instantiate(
            mousePrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        aliveMice.Add(newMouse);
    }

    private void RemoveDestroyedMice()
    {
        for (int i = aliveMice.Count - 1; i >= 0; i--)
        {
            if (aliveMice[i] == null)
            {
                aliveMice.RemoveAt(i);
            }
        }
    }

    private void UpdateDifficulty()
    {
        difficultyTimer -= Time.deltaTime;

        if (difficultyTimer > 0f)
        {
            return;
        }

        if (maxMiceAlive < maxMiceLimit)
        {
            maxMiceAlive++;
        }

        if (spawnInterval > minimumSpawnInterval)
        {
            spawnInterval -= 0.5f;
            spawnInterval = Mathf.Max(spawnInterval, minimumSpawnInterval);
        }

        difficultyTimer = difficultyIncreaseInterval;
    }
}