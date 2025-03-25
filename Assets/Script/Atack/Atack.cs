using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    public XpBar targetedEnemy; // Целевая цель для атаки
    public int skillDamage = 100; // Урон навыка
    public Button attackButton; // Кнопка для атаки
    public TMP_Text targetText; // Ссылка на текстовое поле для отображения цели

    private void Start()
    {
        attackButton.onClick.AddListener(Attack);
        UpdateTargetText();
    }

    // Метод для выбора врага, при клике на него
    private void OnMouseDown()
    {
        // Получаем позицию курсора в мировых координатах
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Проверяем, есть ли коллайдер в этой точке
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);
        if (hitCollider != null)
        {
            // Проверяем, является ли коллайдер врагом
            XpBar enemy = hitCollider.GetComponent<XpBar>();
            if (enemy != null)
            {
                SelectTarget(enemy);
            }
        }
    }

    // Метод для выбора цели
    public void SelectTarget(XpBar enemy)
    {
        targetedEnemy = enemy;
        UpdateTargetText();
    }

    // Метод для атаки
    private void Attack()
    {
        if (targetedEnemy != null)
        {
            targetedEnemy.TakeDamage(skillDamage);
        }
        else
        {
            Debug.Log("No target selected!");
        }
    }

    // Обновляем текст с информацией о целе
    private void UpdateTargetText()
    {
        if (targetedEnemy != null)
        {
            targetText.text = $"{targetedEnemy.name}"; // Отображаем имя выбранного врага
        }
        else
        {
            targetText.text = "Target: None";
        }
    }

}
