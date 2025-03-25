using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float timeDestroy = 3f;
    public float speed = 3f;
    public Rigidbody2D rb;
    public Transform player; // ������ �� ������
    public float detectionRange = 5f; // ���������� ��� ����������� ����

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // ���������, ��������� �� ����� � ���� ���������
        if (Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            // ���������� ���� � ������
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;
        }
        else
        {
            // ���� ����� ��� ����, ������������� ������� �����������
            rb.linearVelocity = transform.up * speed;
        }

        Invoke("DestroyBullet", timeDestroy);
    }

    void DestroyBullet()
    {
        Destroy(this.gameObject);
    }
}
