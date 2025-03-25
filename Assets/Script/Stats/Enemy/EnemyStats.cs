using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Не забудьте добавить этот using для работы с UI

public class EnemyStats : MonoBehaviour
{
    private int max_hp = 300;
    private int currently_hp;
    public int xp_enemy = 10;
    public int attack_enemy = 45;
    public int speed = 1;
    public int armor = 100;

    private PlayerStats playerStats;
    private Animator animator; // Переменная для аниматора
    private Coroutine hurtCoroutine; // Корутин для сброса анимации

    public Image healthBarFill;

    // Start is called before the first frame update
    void Start()
    {
        currently_hp = max_hp;
        playerStats = FindObjectOfType<PlayerStats>();
        animator = GetComponent<Animator>(); // Получаем компонент Animator
        UpdateHealthBar(); // Обновляем шкалу здоровья при старте
    }

    // Update is called once per frame
    void Update()
    {
        if (currently_hp <= 0)
        {
            Die(); // Вызываем метод смерти
        }
        else
        {
            UpdateHealthBar(); // Обновляем шкалу здоровья каждый кадр
        }
    }

    public int GetXp()
    {
        return xp_enemy;
    }

    public void TakeHit(int damage)
    {
        currently_hp -= damage;
        if (currently_hp < 0) currently_hp = 0;
        UpdateHealthBar(); // Обновляем шкалу здоровья после получения урона

        animator.SetTrigger("isHurt"); // Запускаем анимацию Hurt

        // Если корутина уже запущена, останавливаем ее
        if (hurtCoroutine != null)
        {
            StopCoroutine(hurtCoroutine);
        }

        // Запускаем новую корутину для сброса анимации
        hurtCoroutine = StartCoroutine(ResetHurtAnimation());
    }

    public bool IsAlive()
    {
        return currently_hp > 0; // Враг жив, если здоровье больше нуля
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            playerStats.TakeHit(attack_enemy);
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            float healthPercentage = (float)currently_hp / max_hp;
            healthBarFill.fillAmount = healthPercentage; // Устанавливаем значение заполнения
        }
    }

    private void Die()
    {
        animator.SetBool("isDead", true); // Запускаем анимацию Death
        // Здесь можно добавить задержку перед уничтожением объекта
        Destroy(gameObject, 1f); // Уничтожаем объект через 1 секунду после смерти
    }

    private IEnumerator ResetHurtAnimation()
    {
        yield return new WaitForSeconds(0.5f); // Задержка перед сбросом анимации
        animator.ResetTrigger("isHurt"); // Сбрасываем триггер Hurt
    }
}
