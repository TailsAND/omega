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
        [HideInInspector] public float lastDeathTime;
        [HideInInspector] public bool isOccupied => spawnedEnemy != null;
    }

    [Header("Spawn Settings")]
    [SerializeField] private SpawnPoint[] spawnPoints;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float spawnCheckInterval = 5f;
    [SerializeField] private int maxActiveEnemies = 10;
    [SerializeField] private float spawnRadius = 1f;
    [SerializeField] private float respawnCooldown = 30f;

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
            if (!point.isOccupied && Time.time - point.lastDeathTime >= respawnCooldown)
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

        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = spawnPoint.point.position + new Vector3(randomOffset.x, randomOffset.y, 0);

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        // Устанавливаем точку спавна как центр патрулирования для врага
        SetupEnemyPatrolCenter(enemy, spawnPosition);
        
        NetworkServer.Spawn(enemy);

        TestenemyHealth health = enemy.GetComponent<TestenemyHealth>();
        if (health != null)
        {
            health.OnDeath += () => OnEnemyDeath(spawnPoint, enemy);
        }

        spawnPoint.spawnedEnemy = enemy;
        activeEnemies.Add(enemy);
    }

    [Server]
    private void SetupEnemyPatrolCenter(GameObject enemy, Vector3 spawnPosition)
    {
        // Для ShadowConjurer
        ShadowConjurer shadowConjurer = enemy.GetComponent<ShadowConjurer>();
        if (shadowConjurer != null)
        {
            // Используем рефлексию для установки приватного поля _initialPosition
            System.Reflection.FieldInfo fieldInfo = typeof(ShadowConjurer).GetField("_initialPosition", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(shadowConjurer, spawnPosition);
            }
            
            // Вызываем SetupPatrolPoints для пересчета точек патрулирования
            shadowConjurer.Invoke("SetupPatrolPoints", 0);
            return;
        }

        // Для EnemyShadowStalker
        EnemyShadowStalker shadowStalker = enemy.GetComponent<EnemyShadowStalker>();
        if (shadowStalker != null)
        {
            System.Reflection.FieldInfo patrolCenterField = typeof(EnemyShadowStalker).GetField("patrolCenter", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (patrolCenterField != null)
            {
                patrolCenterField.SetValue(shadowStalker, (Vector2)spawnPosition);
            }
            return;
        }

        // Для DarkCultistController
        DarkCultistController darkCultist = enemy.GetComponent<DarkCultistController>();
        if (darkCultist != null)
        {
            System.Reflection.FieldInfo initialPositionField = typeof(DarkCultistController).GetField("_initialPosition", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (initialPositionField != null)
            {
                initialPositionField.SetValue(darkCultist, spawnPosition);
            }
            return;
        }
    }

    [Server]
    private void OnEnemyDeath(SpawnPoint spawnPoint, GameObject enemy)
    {
        if (spawnPoint.spawnedEnemy == enemy)
        {
            spawnPoint.spawnedEnemy = null;
            spawnPoint.lastDeathTime = Time.time;
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