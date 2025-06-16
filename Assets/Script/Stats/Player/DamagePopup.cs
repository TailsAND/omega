using UnityEngine;
using UnityEngine.UI;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private Font _font;
    [SerializeField] private int _fontSize = 24;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _criticalColor = Color.yellow;
    [SerializeField] private float _moveSpeed = 50f;
    [SerializeField] private float _lifeTime = 1f;
    [SerializeField] private float _fadeOutTime = 0.5f;
    
    private Text _text;
    private float _timer;
    private Color _originalColor;
    private RectTransform _rectTransform;
    private Canvas _canvas;

    private void Awake()
    {
        // Создаем Canvas если его нет
        CreateCanvasIfNeeded();
        
        // Создаем текстовый объект
        GameObject textObject = new GameObject("DamageText");
        textObject.transform.SetParent(_canvas.transform, false);
        
        // Добавляем компоненты
        _text = textObject.AddComponent<Text>();
        _rectTransform = textObject.GetComponent<RectTransform>();
        
        // Настраиваем текст
        _text.font = _font;
        _text.fontSize = _fontSize;
        _text.alignment = TextAnchor.MiddleCenter;
        _text.horizontalOverflow = HorizontalWrapMode.Overflow;
        _text.verticalOverflow = VerticalWrapMode.Overflow;
        
        _originalColor = _normalColor;
        _text.color = _originalColor;
        
        // Уничтожаем через заданное время
        Destroy(gameObject, _lifeTime);
    }

    private void CreateCanvasIfNeeded()
    {
        _canvas = FindObjectOfType<Canvas>();
        if (_canvas == null)
        {
            GameObject canvasGO = new GameObject("DamageCanvas");
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
    }

    public void SetDamage(int damage)
    {
        if (_text == null) return;
        
        _text.text = damage.ToString();
        
        // Критический урон (больше 20)
        if (damage > 20)
        {
            _text.color = _criticalColor;
            _text.fontSize = (int)(_fontSize * 1.5f);
        }
    }

    private void Update()
    {
        if (_text == null) return;
        
        // Плавное перемещение вверх
        _rectTransform.anchoredPosition += Vector2.up * _moveSpeed * Time.deltaTime;
        
        // Плавное исчезновение
        _timer += Time.deltaTime;
        if (_timer > _lifeTime - _fadeOutTime)
        {
            float alpha = 1 - ((_timer - (_lifeTime - _fadeOutTime)) / _fadeOutTime);
            _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, alpha);
        }
    }

    private void OnDestroy()
    {
        if (_text != null)
        {
            Destroy(_text.gameObject);
        }
    }
}