using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Kdr : MonoBehaviour
{
    public Button skillButton; // Ссылка на кнопку навыка
    public Image cooldownOverlayImage; // Ссылка на изображение с визуальным эффектом охлаждения

    private bool _isCooldown = false; // Флаг доступности навыка
    private float lastActivationTime; // Время последнего использования

    private void Start()
    {
        // Подключаем методы к кнопке навыка
        skillButton.onClick.AddListener(ActivateSkill);
    }

    private void Update()
    {
        // Если навык находится на кулдауне, то меняем прозрачность
        if (_isCooldown)
        {
            float cooldown = 10f; // Время КД в секундах
            float timeLeft = cooldown - (Time.time - lastActivationTime);

            if (timeLeft <= 0)
            {
                _isCooldown = false;
                cooldownOverlayImage.fillAmount = 0f;
                cooldownOverlayImage.gameObject.SetActive(false);
                skillButton.interactable = true; // Включаем кнопку снова
                return;
            }

            cooldownOverlayImage.fillAmount = timeLeft / cooldown;
        }
    }

    private void ActivateSkill()
    {
        if (_isCooldown) return; // Если навык на КД - то мы не можем его использовать
        _isCooldown = true; // Навык в КД
        lastActivationTime = Time.time; // Запоминаем время последнего использования
        cooldownOverlayImage.gameObject.SetActive(true); // Отображаем визуальный эффект КД

        skillButton.interactable = false; // Отключаем кнопку навыка
    }
}
