using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image fill;
    
    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
    }
    
    public void SetHealth(int health)
    {
        slider.value = health;
        
        // Изменение цвета в зависимости от уровня здоровья
        fill.color = Color.Lerp(Color.red, Color.green, slider.normalizedValue);
    }
}