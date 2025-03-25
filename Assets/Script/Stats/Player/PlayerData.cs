using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // ���������, ��� ����� ����� ���� ������������
public class PlayerData
{
    public int maxHp; // ������������ ��������
    public int currentlyHp; // ������� ��������
    public int maxMana; // ������������ ����
    public int currentlyMana; // ������� ����

    public int armor; // �����
    public int xpNeeded; // ����������� ���� ��� ���������� ������
    public int xpNeededPerLvl; // ����, ����������� ��� ��������� ������
    public int xpCurrently; // ������� ����
    public int lvl; // ������� ������
    public int abilityPoints; // ���� ������

    public int strength; // ����
    public int sanity; // ����������� ���������
    public int agility; // ��������
    public int luck; // �����
    public int speed; // ��������

    public Vector2 vector2; // ��������������
}