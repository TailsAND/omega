using UnityEngine;

public class StatsUpgrade : MonoBehaviour{
    public void IncreaseStrength() {
        if (InventoryManager.Instance.PlayerSkillController.PlayerStats.AbilityPoints > 0) {
            InventoryManager.Instance.PlayerSkillController.PlayerStats.Strength++;
            InventoryManager.Instance.PlayerSkillController.PlayerStats.AbilityPoints -= 1;
            InventoryManager.Instance.PlayerSkillController.PlayerStats.UpdateAllStats();
            PlayerUI.Instance.SetStateOfAbilityUpdateButtons();
        }
    }

    public void IncreaseSanity() {
        if (InventoryManager.Instance.PlayerSkillController.PlayerStats.AbilityPoints > 0) {
            InventoryManager.Instance.PlayerSkillController.PlayerStats.Sanity++;
            InventoryManager.Instance.PlayerSkillController.PlayerStats.AbilityPoints -= 1;
            InventoryManager.Instance.PlayerSkillController.PlayerStats.UpdateAllStats();
            PlayerUI.Instance.SetStateOfAbilityUpdateButtons();
        }
    }

    public void IncreaseAgility() {
        if (InventoryManager.Instance.PlayerSkillController.PlayerStats.AbilityPoints > 0) {
            InventoryManager.Instance.PlayerSkillController.PlayerStats.Agility++;
            InventoryManager.Instance.PlayerSkillController.PlayerStats.AbilityPoints -= 1;
            InventoryManager.Instance.PlayerSkillController.PlayerStats.UpdateAllStats();

            PlayerUI.Instance.SetStateOfAbilityUpdateButtons();
        }
    }

    public void IncreaseLuck() {
        if (InventoryManager.Instance.PlayerSkillController.PlayerStats.AbilityPoints > 0) {
            InventoryManager.Instance.PlayerSkillController.PlayerStats.Luck++;
            InventoryManager.Instance.PlayerSkillController.PlayerStats.AbilityPoints -= 1;
            InventoryManager.Instance.PlayerSkillController.PlayerStats.UpdateAllStats();
            PlayerUI.Instance.SetStateOfAbilityUpdateButtons();
        }
    }

    public void IncreaseSpeed() {
        if (InventoryManager.Instance.PlayerSkillController.PlayerStats.AbilityPoints > 0) {
            InventoryManager.Instance.PlayerSkillController.PlayerStats.Speed++;
            InventoryManager.Instance.PlayerSkillController.PlayerStats.AbilityPoints -= 1;
            float speedMultiply = 0.05f;
            InventoryManager.Instance.PlayerSkillController.PlayerStats.gameObject.GetComponent<PlayerMovement>().moveSpeed += speedMultiply;
            InventoryManager.Instance.PlayerSkillController.PlayerStats.UpdateAllStats();
            PlayerUI.Instance.SetStateOfAbilityUpdateButtons();
        }
    }

}