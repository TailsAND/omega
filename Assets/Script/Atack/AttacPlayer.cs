using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttacPlayer : MonoBehaviour
{

    [System.Serializable]
    public class AttackOption
    {
        public KeyCode attackKey; // Клавиша атаки
        public int attackPower; // Сила атаки
        public int manaCost; // Затраты маны
        public GameObject attackEffectPrefab; // Префаб визуального эффекта
        public bool effectTriggered = false; // Флаг активации эффекта
    }

    public AttackOption[] attackOptions; // Массив вариантов атаки
    public string[] enemyTags; // Массив тегов врагов
    private PlayerStats playerStats; // Ссылка на PlayerStats

    void Start()
    {
        playerStats = GetComponent<PlayerStats>(); // Получаем компонент PlayerStats из того же объекта
    }

    void Update()
    {
        foreach (var attackOption in attackOptions)
        {
            if (Input.GetKeyDown(attackOption.attackKey) && !attackOption.effectTriggered)
            {
                Attack(attackOption); // Атакуем с использованием текущего варианта
                attackOption.effectTriggered = true; // Установим флаг, что эффект уже сработал
            }

            // Сбрасываем флаг при отпускании клавиши
            if (Input.GetKeyUp(attackOption.attackKey))
            {
                attackOption.effectTriggered = false;
            }
        }
    }

    void Attack(AttackOption attackOption)
    {
        // Проверяем, достаточно ли маны для использования навыка
        if (!playerStats.HasEnoughMana(attackOption.manaCost))
        {
            Debug.Log("Not enough mana!");
            return; // Прерываем выполнение, если маны недостаточно
        }

        // Уменьшаем ману
        playerStats.ConsumeMana(attackOption.manaCost);

        // Создаем визуальный эффект по позиции курсора
        GameObject effectInstance = CreateAttackEffect(attackOption.attackEffectPrefab);

        if (effectInstance != null)
        {
            // Запускаем корутину для удаления эффекта после определенного времени
            StartCoroutine(DestroyEffectAfterTime(effectInstance, 1.0f)); // Замените 1.0f на нужное время в секундах
        }

        // Определяем врага под курсором
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null)
        {
            // Проверяем, что тег коллайдера находится в массиве enemyTags
            foreach (var tag in enemyTags)
            {
                if (hit.collider.CompareTag(tag)) // Если тег совпадает
                {
                    EnemyStats targetEnemy = hit.collider.GetComponent<EnemyStats>(); // Получаем EnemyStats из коллайдера

                    if (targetEnemy != null && targetEnemy.IsAlive())
                    {
                        // Реализуем атаку
                        int damage = CalculateDamage(attackOption.attackPower, targetEnemy.attack_enemy);
                        targetEnemy.TakeHit(damage);
                    }
                    break; // Выходим из цикла, если нашли врага
                }
            }
        }
    }

    GameObject CreateAttackEffect(GameObject effectPrefab)
    {
        if (effectPrefab != null)
        {
            // Получаем позицию курсора на экране и преобразуем её в мировые координаты
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Устанавливаем Z-координату в 0, если у вас 2D игра

            return Instantiate(effectPrefab, mousePosition, Quaternion.identity);
        }
        return null; // Возвращаем null, если объект не был создан
    }

    IEnumerator DestroyEffectAfterTime(GameObject effectInstance, float duration)
    {
        // Ждем указанное время перед уничтожением
        yield return new WaitForSeconds(duration);

        // Уничтожаем эффект после ожидания
        Destroy(effectInstance);
    }

    int CalculateDamage(int attackPower, int enemyArmor)
    {
        int damage = attackPower - enemyArmor;
        return damage > 0 ? damage : 0; // Если урон больше 0, возвращаем его, иначе 0
    }
}
