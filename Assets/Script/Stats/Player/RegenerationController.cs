using Mirror;
using UnityEngine;

public class RegenerationController : NetworkBehaviour {
    [SyncVar] private float timer;
    [SyncVar] private float interval_of_one_second = 1f;

    [SyncVar] private float hp_regeneration;
    [SyncVar] private float mana_regeneration;

    [SerializeField][SyncVar] private float hp_regeneration_per_second = 0.05f;
    [SerializeField][SyncVar] private float mana_regeneration_per_second = 0.2f; 


    public float ManaRegeneration {
        get { return mana_regeneration; }
        set { mana_regeneration = value; }
    }

    public float HealthRegeneration {
        get { return hp_regeneration; }
        set { hp_regeneration = value; }
    }

    public float ManaRegenerationPerSecond {
        get { return mana_regeneration_per_second; }
        set { mana_regeneration_per_second = value; }
    }

    public float HealthRegenerationPerSecond {
        get { return hp_regeneration_per_second; }
        set { hp_regeneration_per_second = value; }
    }

    private PlayerStats playerStats;
    private PlayerUI playerUI;

    private void Start() {
        playerStats = GetComponent<PlayerStats>();
        playerUI = GetComponent<PlayerUI>();
    }

    private void Update() {
        timer += Time.deltaTime;
        Regeneration();
    }

    private void Regeneration() {
        if (!(timer >= interval_of_one_second && playerStats.MaxMana >= playerStats.CurrentlyMana &&
              playerStats.MaxHp >= playerStats.CurrentlyHp)) {
            return;
        }

        hp_regeneration += hp_regeneration_per_second;
        mana_regeneration += mana_regeneration_per_second;

        if (hp_regeneration >= 1) {
            playerStats.CurrentlyHp += (int)hp_regeneration;
            hp_regeneration = 0f;
            playerUI.UpdateUI();
        }

        if (mana_regeneration >= 1) {
            playerStats.CurrentlyMana += (int)mana_regeneration;
            mana_regeneration = 0f;
            playerUI.UpdateUI();
        }

        timer = 0f;
    }
}