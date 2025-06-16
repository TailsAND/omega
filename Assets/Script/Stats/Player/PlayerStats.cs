using Mirror;
using UnityEngine;

public class PlayerStats : NetworkBehaviour {
    [SyncVar] private int max_hp = 300;
    [SyncVar] private int currently_hp = 300;
    [SyncVar] private int max_mana = 65;
    [SyncVar] private int currently_mana = 65;

    [SyncVar] private int armor = 35;
    [SyncVar] private int xp_needed = 1000;
    [SyncVar] private int xp_needed_per_lvl = 1000;
    [SyncVar] private int xp_currently = 0;
    [SyncVar] private int lvl = 1;
    [SyncVar] private int ability_points;

    [SyncVar] private int strength = 1;
    [SyncVar] private int sanity = 1;
    [SyncVar] private int agility = 1;
    [SyncVar] private int luck = 1;
    [SyncVar] private int speed = 1;
    [SyncVar] 
    
    public bool greenZone = true;

    private Vector3 _spawnPosition;
    

    public int MaxHp {
        get { return max_hp; }
        set {  max_hp = value;
            if (playerUI != null) playerUI.UpdateUI();}
    }

    public int CurrentlyHp {
        get { return currently_hp; }
        set { currently_hp = value; }
    }

    public int MaxMana {
        get { return max_mana; }
        set {  max_mana = value;
            if (playerUI != null) playerUI.UpdateUI(); }
    }

    public int CurrentlyMana {
        get { return currently_mana; }
        set { currently_mana = value; }
    }

    public int Armor {
        get { return armor; }
        set { armor = value;
            if (playerUI != null) playerUI.UpdateUI();}
    }

    public int Lvl {
        get { return lvl; }
        set { lvl = value; }
    }

    public int XpNeeded {
        get { return xp_needed; }
        set { xp_needed = value; }
    }

    public int XpCurrently {
        get { return xp_currently; }
        set { xp_currently = value; }
    }

    public int AbilityPoints {
        get { return ability_points; }
        set { ability_points = value; }
    }

    public int Strength {
        get { return strength; }
        set { strength = value; }
    }

    public int Sanity {
        get { return sanity; }
        set { sanity = value; }
    }

    public int Agility {
        get { return agility; }
        set { agility = value; }
    }

    public int Luck {
        get { return luck; }
        set { luck = value; }
    }

    public int Speed {
        get { return speed; }
        set { speed = value; }
    }


    private PlayerMovement playerMovement;
    private PlayerUI playerUI;




    private void Awake() {

        _spawnPosition = transform.position;
    }

    private void OnApplicationQuit() {
        //SavePlayerData(); // Сохраняем данные при выходе
    }

    void Start() {
        FindPlayerComponents();
        currently_hp -= 150;
    }
    
    void Update() {
        if (!isLocalPlayer) {
            return;
        }

        AddExperience(2); //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!ДЛЯ ТЕСТА удалить потом
        CheckHpAndMana();
        playerUI.UpdateUI();
    }

    // public override void OnStartLocalPlayer()
    // {
    //     base.OnStartLocalPlayer();
    //     gameObject.layer = LayerMask.NameToLayer("Player");
    // }
    
    public void UseItem(ItemConfig itemConfig)
    {
        if (itemConfig.isHealing)
        {
            currently_hp += (int)itemConfig.HealingPower;
            if (currently_hp > max_hp)
            {
                currently_hp = max_hp; // Ограничиваем здоровье максимальным значением
            }
        }

        if (itemConfig.isMana)
        {
            currently_mana += (int)itemConfig.ManaPower;
            if (currently_mana > max_mana)
            {
                currently_mana = max_mana; // Ограничиваем ману максимальным значением
            }
        }

        playerUI.UpdateUI(); 
    }

    private void LevelUp() {
        // сохраняем избыток опыта, для перевода его в следующий уровень
        int extra_xp = xp_currently - xp_needed;


        if (lvl < 100 && xp_currently >= xp_needed) {
            lvl += 1;

            xp_currently = 0;
            ability_points += 1;
            PlayerUI.Instance.SetStateOfAbilityUpdateButtons();
        }


        if (lvl >= 100 && xp_currently >= xp_needed) {
            lvl += 1;
            xp_currently = 0;
        }

        //выдаём остаток опыта, если таковой есть
        xp_currently += extra_xp;
        UpdateAllStats();
    }
    
    public void UpdateAllStats() {
        xp_needed = (int)(xp_needed_per_lvl + xp_needed_per_lvl * (lvl - 1));
        int strength_hp = (strength - 1) * 20;
        max_hp = (int)300 + strength_hp;
        int sanity_mana = (sanity - 1) * 10;
        max_mana = (int)65 + sanity_mana;
        playerUI.UpdateUI();
    }

    public void AddExperience(int experience) {
        xp_currently += experience;
        if (xp_currently >= xp_needed) {
            LevelUp();
        }

        playerUI.UpdateUI();
    }

    [Server]
    public void TakeHit(int damage)
    {
        if (!isServer) 
        {
            Debug.Log("TakeHit called on client, ignoring");
            return;
        }
    
        currently_hp -= damage;
        Debug.Log($"Player took {damage} damage, health now: {currently_hp}");
    
        if (currently_hp <= 0)
        {
            currently_hp = 0;
            Die();
        }
    
        RpcUpdateHealth(currently_hp);
    }
    

    [ClientRpc]
    private void RpcUpdateHealth(int newHealth)
    {
        currently_hp = newHealth;
        if (playerUI != null)
        {
            playerUI.UpdateUI();
        }
    }

    [Client]
    private void CheckHpAndMana() {
        if (currently_hp <= 0) {
            Die();
        }
        else if (currently_hp >= max_hp) {
            currently_hp = max_hp;
        }

        if (currently_mana < 0) {
            currently_mana = 0;
        }
        else if (currently_mana >= max_mana) {
            currently_mana = max_mana;
        }
    }

    private void Die() {
        GetComponent<PlayerInventory>().DropOnDie();
        GetComponent<PlayerSkillController>().GreenZone = true;
        greenZone = true;
        currently_hp = max_hp / 2;
        transform.position = _spawnPosition;
    }

    [Client]
    private void UpdateEverything() {
        playerUI.UpdateUI();
        UpdateAllStats();
    }

    [Client]
    private void FindPlayerComponents() {
        playerMovement = GetComponent<PlayerMovement>();
        playerUI = GetComponent<PlayerUI>();
        
    }
    public void SetStateOfAbilityUpdateButtons() {
        if (InventoryManager.Instance.PlayerSkillController.PlayerStats.AbilityPoints > 0) {
            UiContainer.Instance.strength_up.transform.localScale = new Vector3(0.3f, 1, 1); // Устанавливаем нормальный размер, кнопка активна
            UiContainer.Instance.sanity_up.transform.localScale = new Vector3(0.3f, 1, 1);
            UiContainer.Instance.agility_up.transform.localScale = new Vector3(0.3f, 1, 1);
            UiContainer.Instance.luck_up.transform.localScale = new Vector3(0.3f, 1, 1);
            UiContainer.Instance.speed_up.transform.localScale = new Vector3(0.3f, 1, 1);
            PlayerUI.Instance.UpdateUI();
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