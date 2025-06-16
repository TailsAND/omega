using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);
    
    [Header("Цвета")]
    [SerializeField] private Color normalColor = Color.red;
    [SerializeField] private Color blockingColor = Color.blue;
    [SerializeField] private float colorChangeSpeed = 5f;

    private GameObject healthBarInstance;
    private Slider healthSlider;
    private Image fillImage;
    private TestenemyHealth enemyHealth;
    private Color targetColor;
    private Color currentColor;

    private void Start()
    {
        enemyHealth = GetComponent<TestenemyHealth>();
        
        // Создаем полоску здоровья
        healthBarInstance = Instantiate(healthBarPrefab, transform.position + offset, Quaternion.identity);
        healthBarInstance.transform.SetParent(transform);
        
        healthSlider = healthBarInstance.GetComponentInChildren<Slider>();
        fillImage = healthSlider.fillRect.GetComponent<Image>();
        
        targetColor = normalColor;
        currentColor = normalColor;
        fillImage.color = currentColor;
        
        UpdateHealthBar();
        
        // Подписываемся на события
        enemyHealth.OnDamageTaken += _ => UpdateHealthBar();
        enemyHealth.OnDeath += OnEnemyDeath;
    }

    private void Update()
    {
        if (healthBarInstance != null)
        {
            // Обновляем позицию и поворот
            healthBarInstance.transform.position = transform.position + offset;
            healthBarInstance.transform.rotation = Camera.main.transform.rotation;
            
            // Плавное изменение цвета
            if (fillImage != null && currentColor != targetColor)
            {
                currentColor = Color.Lerp(currentColor, targetColor, colorChangeSpeed * Time.deltaTime);
                fillImage.color = currentColor;
            }
        }
    }

    public void SetBlockingState(bool isBlocking)
    {
        targetColor = isBlocking ? blockingColor : normalColor;
    }

    private void UpdateHealthBar()
    {
        if (healthSlider != null && enemyHealth != null)
        {
            healthSlider.maxValue = enemyHealth.MaxHp;
            healthSlider.value = enemyHealth.CurrentHealth;
        }
    }

    private void OnEnemyDeath()
    {
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
    }

    private void OnDestroy()
    {
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
    }
}