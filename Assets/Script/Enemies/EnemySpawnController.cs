using System.Collections;
using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class EnemySpawnController : NetworkBehaviour
{
    [System.Serializable]
    public class SpawnPoint
    {
        public Transform point;
        [HideInInspector] public GameObject spawnedEnemy;
        [HideInInspector] public bool isOccupied => spawnedEnemy != null;
    }

    [Header("Spawn Settings")]
    [SerializeField] private SpawnPoint[] spawnPoints;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float spawnCheckInterval = 5f;
    [SerializeField] private int maxActiveEnemies = 10;
    [SerializeField] private float spawnRadius = 1f;

    [Header("Wave Settings")]
    [SerializeField] private int enemiesPerWave = 3;
    [SerializeField] private float waveCooldown = 20f;
    [SerializeField] private float initialDelay = 5f;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private float nextWaveTime;

    public override void OnStartServer()
    {
        base.OnStartServer();
        nextWaveTime = Time.time + initialDelay;
        StartCoroutine(SpawnRoutine());
    }

    [Server]
    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (Time.time >= nextWaveTime && activeEnemies.Count < maxActiveEnemies)
            {
                SpawnWave();
                nextWaveTime = Time.time + waveCooldown;
            }

            CleanupDeadEnemies();
            yield return new WaitForSeconds(spawnCheckInterval);
        }
    }

    [Server]
    private void SpawnWave()
    {
        int enemiesToSpawn = Mathf.Min(enemiesPerWave, maxActiveEnemies - activeEnemies.Count);
        List<SpawnPoint> availablePoints = GetAvailableSpawnPoints();

        for (int i = 0; i < enemiesToSpawn && i < availablePoints.Count; i++)
        {
            SpawnEnemyAtPoint(availablePoints[i]);
        }
    }

    [Server]
    private List<SpawnPoint> GetAvailableSpawnPoints()
    {
        List<SpawnPoint> available = new List<SpawnPoint>();
        foreach (var point in spawnPoints)
        {
            if (!point.isOccupied)
            {
                available.Add(point);
            }
        }
        return available;
    }

    [Server]
    private void SpawnEnemyAtPoint(SpawnPoint spawnPoint)
    {
        if (enemyPrefabs.Length == 0) return;

        // Выбираем случайного врага из доступных префабов
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        
        // Случайное смещение в пределах spawnRadius
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = spawnPoint.point.position + new Vector3(randomOffset.x, randomOffset.y, 0);

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        NetworkServer.Spawn(enemy);

        // Назначаем обработчик смерти врага
        TestenemyHealth health = enemy.GetComponent<TestenemyHealth>();
        if (health != null)
        {
            health.OnDeath += () => OnEnemyDeath(spawnPoint, enemy);
        }

        spawnPoint.spawnedEnemy = enemy;
        activeEnemies.Add(enemy);
    }

    [Server]
    private void OnEnemyDeath(SpawnPoint spawnPoint, GameObject enemy)
    {
        if (spawnPoint.spawnedEnemy == enemy)
        {
            spawnPoint.spawnedEnemy = null;
        }

        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }

    [Server]
    private void CleanupDeadEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.RemoveAt(i);
            }
            else
            {
                TestenemyHealth health = activeEnemies[i].GetComponent<TestenemyHealth>();
                if (health != null && health.IsDead)
                {
                    activeEnemies.RemoveAt(i);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (var point in spawnPoints)
            {
                if (point.point != null)
                {
                    Gizmos.DrawWireSphere(point.point.position, 0.5f);
                    Gizmos.DrawWireSphere(point.point.position, spawnRadius);
                }
            }
        }
    }
}