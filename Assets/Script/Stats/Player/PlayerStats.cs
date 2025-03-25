using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System.IO;
using Mirror;
public class PlayerStats : NetworkBehaviour
{
    [Header("Player Stats texts")]
    public TMP_Text hp_text;
    public TMP_Text hp_text_inv;

    public TMP_Text mana_text;
    public TMP_Text mana_text_inv;

    public TMP_Text armor_text;
    public TMP_Text strength_text;
    public TMP_Text sanity_text;
    public TMP_Text agility_text;
    public TMP_Text luck_text;
    public TMP_Text speed_text;
    public TMP_Text level_text;
    public TMP_Text level_text_percent_for_new_level;
    public TMP_Text ability_points_text;
    public TMP_Text exp_text;

    [Header("Player Stats UI buttons")]
    public GameObject strength_up;
    public GameObject sanity_up;
    public GameObject agility_up;
    public GameObject luck_up;
    public GameObject speed_up;

    [SyncVar]
    private int max_hp = 300;
    [SyncVar]
    private int currently_hp = 300;
    [SyncVar]
    private int max_mana = 65;
    [SyncVar]
    private int currently_mana = 65;

    [SyncVar]
    private int armor = 35;
    [SyncVar]
    private int xp_needed = 1000;
    [SyncVar]
    private int xp_needed_per_lvl = 1000;
    [SyncVar]
    private int xp_currently = 0;
    [SyncVar]
    private int lvl = 1;
    [SyncVar]
    private int ability_points;

    [SyncVar]
    private int strength = 1;
    [SyncVar]
    private int sanity = 1;
    [SyncVar]
    private int agility = 1;
    [SyncVar]
    private int luck = 1;
    [SyncVar]
    private int speed = 1;

    [SyncVar]
    private float timer = 0f;
    [SyncVar]
    private float interval = 1f;

    [SyncVar]
    private float hp_regeneration;
    [SyncVar]
    private float mana_regeneration;

    public Image healthBarFill;  //Добавил Денис для отображения hp на canvas
    public Image manahBarFill;  //Добавил Денис для отображения mana на canvas
    public Image expBarFill;  //Добавил Денис для отображения hp на персанаже на canvas

    private PlayerMovement playerMovement;
    private Find find;
    private void OnApplicationQuit()
    {
        //SavePlayerData(); // Сохраняем данные при выходе
    }
    void Start()
    {
        find = new();
        FindAllUI();
        //StartCoroutine(FindObjectsAsync());
        //Debug.Log(Application.persistentDataPath);

        UpdateEverything();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        AddExperience(2); //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!ДЛЯ ТЕСТА удалить потом
        CheckHpAndMana();
        UpdateUI();

        // Регенерация здоровья и маны раз в секунду, если не максимальное здоровье или мана
        // может быть переделать это в карутину в будущем
        timer += Time.deltaTime;
        if (timer >= interval && max_mana >= currently_mana && max_hp >= currently_hp)
        {
            Regeneration();
            timer = 0f; // Сброс времени на 1 секунду
        }
    }



    public void UseItem(Item item)
    {
        if (item.isHealing)
        {
            currently_hp += (int)item.HealingPower;
            if (currently_hp > max_hp)
            {
                currently_hp = max_hp; // Ограничиваем здоровье максимальным значением
            }
        }

        if (item.isMana)
        {
            currently_mana += (int)item.ManaPower;
            if (currently_mana > max_mana)
            {
                currently_mana = max_mana; // Ограничиваем ману максимальным значением
            }
        }

        UpdateUI(); // Обновляем интерфейс после использования предмета
    }




    private void UpdateHealthBar() //Добавил Денис для отображения hp на canvas
    {

        if (healthBarFill != null)
        {
            float healthPercentage = (float)currently_hp / max_hp;
            healthBarFill.fillAmount = healthPercentage;
        }
    }

    private void UpdateEXPBar() //Добавил Юра для отображения exp на canvas
    {
        if (expBarFill != null)
        {
            float expPercentage = (float)xp_currently / xp_needed;
            expBarFill.fillAmount = expPercentage;
            level_text_percent_for_new_level.text = expPercentage.ToString("0.00%");
        }
    }

    private void UpdateManaBar() //Добавил Денис для отображения mana на canvas
    {
        if (manahBarFill != null)
        {
            float manaPercentage = (float)currently_mana / max_mana;
            manahBarFill.fillAmount = manaPercentage;
        }
    }

    private void LevelUp()
    {

        // сохраняем избыток опыта, для перевода его в следующий уровень
        int extra_xp = xp_currently - xp_needed;

        //Повторная проверка на условие опыта и проверка на условный ограничитель в 100 уровней 
        if (lvl < 100 && xp_currently >= xp_needed)
        {
            lvl += 1;

            xp_currently = 0;
            ability_points += 1;
            SetStateOfAbilityUpdateButtons();
        }

        //если уровень выше 100, то просто прибавляем к нему 1, ничего не выдавая
        if (lvl >= 100 && xp_currently >= xp_needed)
        {
            lvl += 1;
            xp_currently = 0;

        }

        //выдаём остаток опыта, если таковой есть
        xp_currently += extra_xp;
        UpdateAllStats();

    }
    /// <summary>
    /// Обновляет характеристика игрока, выдавая им новые значения
    /// </summary>
    private void UpdateAllStats()
    {
        xp_needed = (int)(xp_needed_per_lvl + xp_needed_per_lvl * (lvl - 1));
        int strength_hp = (strength - 1) * 20;
        max_hp = (int)300 + strength_hp;
        int sanity_mana = (sanity - 1) * 10;
        max_mana = (int)65 + sanity_mana;
        UpdateUI();
    }

    public void AddExperience(int experience)
    {
        xp_currently += experience;
        if (xp_currently >= xp_needed)
        {
            LevelUp();
        }
        UpdateUI();
    }
    [Client]
    private void Regeneration()
    {
        hp_regeneration += 0.05f; // восстановление единицы хп раз в 20 секунд 
        mana_regeneration += 0.2f; // восстановление единицы маны раз в 5 секунд 

        if (hp_regeneration >= 1)
        {
            currently_hp += (int)hp_regeneration;
            hp_regeneration = 0f; // Сбросить значения регенерации
            UpdateUI();

        }
        if (mana_regeneration >= 1 && currently_mana > max_mana * 0.1f)
        {
            currently_mana += (int)mana_regeneration;
            mana_regeneration = 0f; // Сбросить значения регенерации
            UpdateUI();
        }
    }
    public void TakeHit(int damage)
    {
        currently_hp -= damage;
        UpdateEverything();
    }
    public void IncreaseStrength()
    {
        if (ability_points > 0)
        {
            strength++;
            ability_points -= 1;
            UpdateAllStats();
            SetStateOfAbilityUpdateButtons();
        }
    }
    public void IncreaseSanity()
    {
        if (ability_points > 0)
        {
            sanity++;
            ability_points -= 1;
            UpdateAllStats();
            SetStateOfAbilityUpdateButtons();


        }

    }
    public void IncreaseAgility()
    {
        if (ability_points > 0)
        {
            agility++;
            ability_points -= 1;
            UpdateAllStats();

            SetStateOfAbilityUpdateButtons();

        }
    }
    public void IncreaseLuck()
    {
        if (ability_points > 0)
        {
            luck++;
            ability_points -= 1;
            UpdateAllStats();
            SetStateOfAbilityUpdateButtons();

        }
    }
    public void IncreaseSpeed()
    {
        if (ability_points > 0)
        {
            speed++;
            ability_points -= 1;
            float speed_multiply = 0.05f;
            playerMovement.moveSpeed += speed_multiply;
            UpdateAllStats();
            SetStateOfAbilityUpdateButtons();

        }
    }
    /// <summary>
    /// Обновляет UI для игрока
    /// </summary>
    [Client]
    public void UpdateUI()
    {
        //Обновляет визуально картинку для игрока в процентном соотношении
        UpdateHealthBar();
        UpdateManaBar();
        UpdateEXPBar();

        //Обновляет текст
        hp_text.text = $"{currently_hp}/{max_hp}";
        hp_text_inv.text = $"{currently_hp}/{max_hp}";

        mana_text.text = $"{currently_mana} / {max_mana}";
        mana_text_inv.text = $"{currently_mana} / {max_mana}";

        armor_text.text = $"{armor}";

        level_text.text = $"{lvl}";
        exp_text.text = $"Опыт: {xp_currently}/{xp_needed}";

        strength_text.text = $"{strength}";
        sanity_text.text = $"{sanity}";
        agility_text.text = $"{agility}";
        luck_text.text = $"{luck}";
        speed_text.text = $"{speed}";
        if (ability_points > 0)
            ability_points_text.text = $"Очки характеристиков: {ability_points}";
        else
            ability_points_text.text = "";

    }
    [Client]
    private void CheckHpAndMana()
    {
        if (currently_hp <= 0)
        {
             //Здоровье закончилось, можно умереть
            Destroy(gameObject);
        }
        else if (currently_hp >= max_hp)
        {
            currently_hp = max_hp;
        }
        if (currently_mana < 0)
        {
            currently_mana = 0;
        }
        else if (currently_mana >= max_mana)
        {
            currently_mana = max_mana;
        }
    }
    public int GetCurretlyMana()
    {
        return currently_mana;
    }
    public int GetMaxMana()
    {
        return max_mana;
    }
    public void ConsumeMana(int mana)
    {
        currently_mana -= mana;
    }

    public bool HasEnoughMana(int cost)
    {
        return currently_mana >= cost; // Проверка достаточности маны
    }
    //public void SavePlayerData()
    //{
    //    Transform transform = GetComponent<Transform>();
    //    PlayerData data = new()
    //    {
    //        maxHp = max_hp,
    //        currentlyHp = currently_hp,
    //        maxMana = max_mana,
    //        currentlyMana = currently_mana,
    //        armor = armor,
    //        xpNeeded = xp_needed,
    //        xpNeededPerLvl = xp_needed_per_lvl,
    //        xpCurrently = xp_currently,
    //        lvl = lvl,
    //        abilityPoints = ability_points,
    //        strength = strength,
    //        sanity = sanity,
    //        agility = agility,
    //        luck = luck,
    //        speed = speed,
    //        //vector2 = transform.position,
    //    };

    //    string json = JsonUtility.ToJson(data);
    //    string path = Application.persistentDataPath + "/playerData.json";
    //    File.WriteAllText(path, json);
    //    Debug.Log("Данные игрока сохранены!");
    //}

    // Метод для загрузки данных игрока
    //public void LoadPlayerData()
    //{
    //    string path = Application.persistentDataPath + "/playerData.json";

    //    if (File.Exists(path))
    //    {
    //        string json = File.ReadAllText(path);
    //        PlayerData data = JsonUtility.FromJson<PlayerData>(json);

    //        max_hp = data.maxHp;
    //        currently_hp = data.currentlyHp;
    //        max_mana = data.maxMana;
    //        currently_mana = data.currentlyMana;
    //        armor = data.armor;
    //        xp_needed = data.xpNeeded;
    //        xp_needed_per_lvl = data.xpNeededPerLvl;
    //        xp_currently = data.xpCurrently;
    //        lvl = data.lvl;
    //        ability_points = data.abilityPoints;
    //        strength = data.strength;
    //        sanity = data.sanity;
    //        agility = data.agility;
    //        luck = data.luck;
    //        speed = data.speed;
    //        //GetComponent<Transform>().position = data.vector2;
    //        Debug.Log("Данные игрока загружены!");
    //    }
    //    else
    //    {
    //        Debug.Log("Файл данных игрока не найден!");
    //    }
    //}
    /// <summary>
    /// Обновляет все данные при запуске игры
    /// </summary>
    [Client]
    private void UpdateEverything()
    {
        UpdateUI();
        UpdateAllStats();
        //LoadPlayerData();

    }
    /// <summary>
    /// Корутина для поиска всех UI элементов
    /// </summary>
    [Client]
    private void FindAllUI()
    {
        hp_text = find.FindUIElement<TMP_Text>("hp text");
        hp_text_inv = find.FindUIElement<TMP_Text>("hp text inv");

        mana_text = find.FindUIElement<TMP_Text>("mana text");
        mana_text_inv = find.FindUIElement<TMP_Text>("mana text inv");

        armor_text = find.FindUIElement<TMP_Text>("armor text");

        strength_text = find.FindUIElement<TMP_Text>("strength text");
        sanity_text = find.FindUIElement<TMP_Text>("sanity text");
        agility_text = find.FindUIElement<TMP_Text>("agility text");
        luck_text = find.FindUIElement<TMP_Text>("luck text");
        speed_text = find.FindUIElement<TMP_Text>("speed text");
        level_text = find.FindUIElement<TMP_Text>("lvl full");
        level_text_percent_for_new_level = find.FindUIElement<TMP_Text>("lvl procent");

        ability_points_text = find.FindUIElement<TMP_Text>("ability points text");
        exp_text = find.FindUIElement<TMP_Text>("exp text");
        healthBarFill = find.FindUIElement<Image>("Healthbar");
        manahBarFill = find.FindUIElement<Image>("Manabar");
        expBarFill = find.FindUIElement<Image>("xp bar filled");


        playerMovement = gameObject.GetComponent<PlayerMovement>();

        strength_up = find.FindGameObject("StrengthUp");
        strength_up.GetComponent<Button>().onClick.AddListener(IncreaseStrength);
        sanity_up = find.FindGameObject("SanityUp");
        sanity_up.GetComponent<Button>().onClick.AddListener(IncreaseSanity);
        agility_up = find.FindGameObject("AgilityUp");
        agility_up.GetComponent<Button>().onClick.AddListener(IncreaseAgility);
        luck_up = find.FindGameObject("LuckUp");
        luck_up.GetComponent<Button>().onClick.AddListener(IncreaseLuck);
        speed_up = find.FindGameObject("SpeedUp");
        speed_up.GetComponent<Button>().onClick.AddListener(IncreaseSpeed);

        UpdateUI();
    }
    /// <summary>
    /// Выставляет состяние кнопок как активное/неактивное если есть/нет очки(-ов) улучшения
    /// </summary>
    private void SetStateOfAbilityUpdateButtons()
    {
        if (ability_points > 0)
        {
            strength_up.transform.localScale = new Vector3(0.3f, 1, 1); // Устанавливаем нормальный размер, кнопка активна
            sanity_up.transform.localScale = new Vector3(0.3f, 1, 1);
            agility_up.transform.localScale = new Vector3(0.3f, 1, 1);
            luck_up.transform.localScale = new Vector3(0.3f, 1, 1);
            speed_up.transform.localScale = new Vector3(0.3f, 1, 1);
            UpdateUI();
        }
        else
        {
            strength_up.transform.localScale = Vector3.zero; // Устанавливаем размер в ноль, кнопка неактивна
            sanity_up.transform.localScale = Vector3.zero;
            agility_up.transform.localScale = Vector3.zero;
            luck_up.transform.localScale = Vector3.zero;
            speed_up.transform.localScale = Vector3.zero;
            ability_points_text.text = "";
        }
    }
}






