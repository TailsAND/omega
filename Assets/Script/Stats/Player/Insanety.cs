using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Insanety : MonoBehaviour
{
    [Header("Enemies that can spawn")]
    public GameObject[] enemyPrefabs;
    [Header("Zone size and number of enemies")]
    public float spawnRadius = 5f;
    public int numberOfEnemies = 10;

    private PlayerStats playerStats;
    private Transform player;

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private Coroutine spawnCoroutine;

    void Start()
    {
        player = GetComponent<Transform>();
        playerStats = FindObjectOfType<PlayerStats>();
        spawnCoroutine = StartCoroutine(SpawnEnemies());
    }

    void Update()
    {
        if (playerStats.GetCurretlyMana() >= playerStats.GetMaxMana() * 0.1f)
        {
            DestroyAllEnemies();
        }
        else if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnEnemies());
        }
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            if (playerStats.GetCurretlyMana() < playerStats.GetMaxMana() * 0.1f)
            {

                GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

                // Генерируем случайную позицию вокруг игрока
                Vector2 spawnPosition2D = Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPosition = new Vector3(spawnPosition2D.x, 0, spawnPosition2D.y) + player.position;

                // Спавним врага
                GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

                enemy.AddComponent<EnemyStats>();
                enemy.AddComponent<EnemyMovement>();


                enemy.GetComponent<EnemyMovement>().chaseRadius = 100f;
                enemy.GetComponent<EnemyMovement>().speed = Random.Range(1.5f, 3.5f);
                enemy.GetComponent<EnemyMovement>().attackRadius = Random.Range(1f, 3.5f);

                // Сохраняем созданного врага
                spawnedEnemies.Add(enemy);

                yield return new WaitForSeconds(Random.Range(.7f, 2f)); // Задержка между спавном врагов
            }
            else
            {
                yield return null; // Ждем следующего кадра, если мана достаточна
            }
        }
    }

    private void DestroyAllEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            Destroy(enemy);
        }
        spawnedEnemies.Clear(); // Очищаем список
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null; // Сбрасываем корутину
        }
    }
}