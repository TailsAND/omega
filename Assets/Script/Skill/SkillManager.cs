using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SkillManager : MonoBehaviour {
    public List<Skill> Skills = new List<Skill>();
    private List<Skill> SkillCooldowns = new List<Skill>();

    public void AddSkill(Skill skill) {
        foreach (var itemSkill in Skills) {
            if (skill == itemSkill) {
                return;
            }
        }

        Skills.Add(skill);
        if (skill.skillConfig.IsPassive) {
            skill.Activate(GetMouseWorldPosition());
        }
    }

    public void RemoveSkill(Skill skill) {
        Skills.RemoveAll(s => s.skillConfig.Name == skill.skillConfig.Name);
    }

    public void UseSkill(Skill playerskill) {
        Skill skill = Skills.Find(s => s == playerskill);

        if (skill == null || SkillCooldowns.Contains(skill)) {
            return;
        }

        if (GetComponent<PlayerStats>().CurrentlyMana - playerskill.skillConfig.ManaCost > 0) {
            Debug.Log(GetComponent<PlayerStats>().CurrentlyMana - playerskill.skillConfig.ManaCost);
            GetComponent<PlayerStats>().CurrentlyMana -= playerskill.skillConfig.ManaCost;
        }
        else {
            return;
        }
        skill.Activate(GetMouseWorldPosition());
        StartCoroutine(CooldownSkill(skill));
    }

    private IEnumerator CooldownSkill(Skill skill) {
        SkillCooldowns.Add(skill);
        yield return new WaitForSeconds(skill.skillConfig.Cooldown);
        SkillCooldowns.Remove(skill);
    }

    private Vector3 GetMouseWorldPosition() {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 0;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}