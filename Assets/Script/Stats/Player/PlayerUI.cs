using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {
    private PlayerStats _playerStats;

    public static PlayerUI Instance;

    private void Awake() {
        Instance = this;
    }

    void Start() {
        _playerStats = GetComponent<PlayerStats>();
        UpdateUI();

        // UpdateEverything();
    }

    public void UpdateUI() {
        if (!_playerStats.isLocalPlayer) {
            return;
        }

        UpdateHealthBar();
        UpdateManaBar();
        UpdateEXPBar();

        UiContainer.Instance.hp_text.text = $"{_playerStats.CurrentlyHp}/{_playerStats.MaxHp}";

        UiContainer.Instance.hp_text_inv.text = $"{_playerStats.CurrentlyHp}/{_playerStats.MaxHp}";

        UiContainer.Instance.mana_text.text = $"{_playerStats.CurrentlyMana} / {_playerStats.MaxMana}";
        UiContainer.Instance.mana_text_inv.text = $"{_playerStats.CurrentlyMana} / {_playerStats.MaxMana}";

        UiContainer.Instance.armor_text.text = $"{_playerStats.Armor}";

        UiContainer.Instance.level_text.text = $"{_playerStats.Lvl}";
        UiContainer.Instance.exp_text.text = $"Опыт: {_playerStats.XpCurrently}/{_playerStats.XpNeeded}";

        UiContainer.Instance.strength_text.text = $"{_playerStats.Strength}";
        UiContainer.Instance.sanity_text.text = $"{_playerStats.Sanity}";
        UiContainer.Instance.agility_text.text = $"{_playerStats.Agility}";
        UiContainer.Instance.luck_text.text = $"{_playerStats.Luck}";
        UiContainer.Instance.speed_text.text = $"{_playerStats.Speed}";

        if (_playerStats.AbilityPoints > 0)
            UiContainer.Instance.ability_points_text.text = $"Очки характеристиков: {_playerStats.AbilityPoints}";
        else
            UiContainer.Instance.ability_points_text.text = "";
    }

    private void UpdateHealthBar() {
        if (UiContainer.Instance.healthBarFill != null) {
            float healthPercentage = (float)_playerStats.CurrentlyHp / _playerStats.MaxHp;
            UiContainer.Instance.healthBarFill.fillAmount = healthPercentage;
        }
    }

    private void UpdateEXPBar() {
        if (UiContainer.Instance.expBarFill != null) {
            float expPercentage = (float)_playerStats.XpCurrently / _playerStats.XpNeeded;
            UiContainer.Instance.expBarFill.fillAmount = expPercentage;
            UiContainer.Instance.level_text_percent_for_new_level.text = expPercentage.ToString("0.00%");
        }
    }

    private void UpdateManaBar() {
        if (UiContainer.Instance.manahBarFill != null) {
            float manaPercentage = (float)_playerStats.CurrentlyMana / _playerStats.MaxMana;
            UiContainer.Instance.manahBarFill.fillAmount = manaPercentage;
        }
    }

    public void SetStateOfAbilityUpdateButtons() {
        if (InventoryManager.Instance.PlayerSkillController.PlayerStats.AbilityPoints > 0) {
            UiContainer.Instance.strength_up.transform.localScale = new Vector3(0.3f, 1, 1); // Устанавливаем нормальный размер, кнопка активна
            UiContainer.Instance.sanity_up.transform.localScale = new Vector3(0.3f, 1, 1);
            UiContainer.Instance.agility_up.transform.localScale = new Vector3(0.3f, 1, 1);
            UiContainer.Instance.luck_up.transform.localScale = new Vector3(0.3f, 1, 1);
            UiContainer.Instance.speed_up.transform.localScale = new Vector3(0.3f, 1, 1);
            UpdateUI();
        } else {
            UiContainer.Instance.strength_up.transform.localScale = Vector3.zero; // Устанавливаем размер в ноль, кнопка неактивна
            UiContainer.Instance.sanity_up.transform.localScale = Vector3.zero;
            UiContainer.Instance.agility_up.transform.localScale = Vector3.zero;
            UiContainer.Instance.luck_up.transform.localScale = Vector3.zero;
            UiContainer.Instance.speed_up.transform.localScale = Vector3.zero;
            UiContainer.Instance.ability_points_text.text = "";
        }
    }
}