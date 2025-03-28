using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f; // �������� �������
    public float lifetime = 2f; // ����� ����� �������
    private Vector2 direction; // ����������� ������ �������

    void Start()
    {
        // ���������� ������ ����� �������� �����
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // ������� ������ � �������� �����������
        transform.Translate(direction * speed * Time.deltaTime);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection; // ������������� �����������
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // ������ ��� ������������ (��������, ���� ������)
        Destroy(gameObject); // ���������� ������ ��� ������������
    }
}
