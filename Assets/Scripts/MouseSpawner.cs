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
    [SerializeField] private bool spawnOnStart = false;

    [Header("Difficulty Scaling")]
    [SerializeField] private bool increaseDifficultyOverTime = false;
    [SerializeField] private float difficultyIncreaseInterval = 20f;
    [SerializeField] private int maxMiceLimit = 8;
    [SerializeField] private float minimumSpawnInterval = 1.5f;

    private readonly List<GameObject> aliveMice = new List<GameObject>();

    private float spawnTimer;
    private float difficultyTimer;
    private float currentMouseSpeed = 2.5f;

    private void Start()
    {
        spawnTimer = spawnInterval;
        difficultyTimer = difficultyIncreaseInterval;

        if (spawnOnStart)
        {
            SpawnStartingMice(1);
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

        MouseMovement mouseMovement = newMouse.GetComponent<MouseMovement>();

        if (mouseMovement != null)
        {
            mouseMovement.moveSpeed = currentMouseSpeed;
        }
        else
        {
            Debug.LogWarning("MouseSpawner: Spawned mouse does not have MouseMovement script.");
        }

        aliveMice.Add(newMouse);
    }

    private void SpawnStartingMice(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            SpawnMouse();
        }
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

    private void ClearExistingMice()
    {
        for (int i = aliveMice.Count - 1; i >= 0; i--)
        {
            if (aliveMice[i] != null)
            {
                Destroy(aliveMice[i]);
            }
        }

        aliveMice.Clear();

        GameObject[] sceneMice = GameObject.FindGameObjectsWithTag("Mouse");

        foreach (GameObject mouse in sceneMice)
        {
            Destroy(mouse);
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

    public void ApplyLevelSettings(LevelManager.LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("MouseSpawner: Level data is null.");
            return;
        }

        if (levelData.levelRoot == null)
        {
            Debug.LogError("MouseSpawner: Level root is missing for " + levelData.levelName);
            return;
        }

        Transform spawnParent = levelData.levelRoot.transform.Find("MouseSpawnPoints");

        if (spawnParent == null)
        {
            Debug.LogError("MouseSpawner: No MouseSpawnPoints found in " + levelData.levelName);
            return;
        }

        List<Transform> newSpawnPoints = new List<Transform>();

        foreach (Transform child in spawnParent)
        {
            newSpawnPoints.Add(child);
        }

        spawnPoints = newSpawnPoints.ToArray();

        currentMouseSpeed = levelData.mouseSpeed;

        maxMiceAlive = levelData.maxMice;
        maxMiceLimit = levelData.maxMice;

        spawnInterval = levelData.spawnInterval;
        spawnTimer = spawnInterval;

        increaseDifficultyOverTime = false;

        ClearExistingMice();
        SpawnStartingMice(levelData.startingMice);
    }

    public void ClearAllMice()
    {
        ClearExistingMice();
    }
}