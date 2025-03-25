using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]

public class Item : ScriptableObject
{
    [Header("Базовые характеристики")]
    public string Name = " ";
    public string Description = "Описание предмета";
    public Sprite icon = null;

    public bool isHealing;
    public float HealingPower;

    public bool isMana;
    public float ManaPower;

    [Header("Игровые характеристики")]
    public int time;

    [Header("Характеристики для торговли")]
    public int Coins;

    public string PlayerPrefsName;
    
}