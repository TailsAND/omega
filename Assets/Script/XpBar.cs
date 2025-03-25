using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class XpBar : MonoBehaviour
{
    public int health = 100; // Текущее здоровье врага
    public int armor = 100; // Броня врага

    // Ссылки на UI элементы TextMeshPro
    public TMP_Text healthText; // Ссылка на TextMeshPro для здоровья
    public TMP_Text armorText; // Ссылка на TextMeshPro для брони

    private void Start()
    {
        // Устанавливаем начальные значения текста
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        // Вычисляем, сколько урона поглотит броня (20% от входящего урона)
        int damageToArmor = Mathf.FloorToInt(damage * 0.2f);
        int damageToHealth = damage - damageToArmor;

        // Обновляем броню и здоровье
        armor -= damageToArmor;
        health -= damageToHealth;

        // Обновляем UI
        UpdateUI();

        // Выводим информацию о damage
        Debug.Log($"Damage dealt: {damage}, Damage to armor: {damageToArmor}, Damage to health: {damageToHealth}");
        Debug.Log($"Remaining Armor: {armor}, Remaining Health: {health}");

        // Проверяем, если враг мёртв
        if (health <= 0)
        {
            Die();
        }
    }


    private void UpdateUI()
    {
        // Обновляем значения текстовых полей
        healthText.text = $"{health}";
        armorText.text = $"{armor}";
    }

    public void Die()
    {
        // Выводим сообщение о смерти
        Debug.Log("Enemy has died!");

        // Возвращаемся на предыдущую сцену
        // '1' - это количество сцен назад, на которое нужно вернуться
        // Например, если у вас была следующая сцена, и вы хотите вернуться на предыдущую
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}
