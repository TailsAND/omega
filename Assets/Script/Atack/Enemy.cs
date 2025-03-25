using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject projectilePrefab; // ������ �������
    public Transform firePoint; // �����, �� ������� ����� ����������� ��������
    public float fireRate = 1f; // ������� ��������
    private float fireCountdown = 0f;
    public Transform player; // ������ �� ������
    public float shootingDistance = 5f; // ������������ ���������� ��� ��������
    public float chanceToShootAtPlayerMovement = 0.5f; // ����������� �������� � ����������� �������� ������

    void Update()
    {
        // ��������� ���������� �� ������
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // ���� ����� ��������� �� ����������, ��������� ��������
        if (distanceToPlayer <= shootingDistance)
        {
            fireCountdown -= Time.deltaTime;

            if (fireCountdown <= 0f)
            {
                Shoot();
                fireCountdown = 1f / fireRate; // ���������� ������
            }
        }
    }

    void Shoot()
    {
        if (projectilePrefab != null && player != null) // �������� �� null
        {
            // ����������, �������� �� � ����������� �������� ������
            Vector2 direction;
            if (Random.value < chanceToShootAtPlayerMovement)
            {
                // �������� ��������� Rigidbody2D ������ (��������������, ��� �� ����)
                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    // ���������� �������� ������ ��� �����������
                    direction = playerRb.linearVelocity.normalized;
                }
                else
                {
                    // ���� Rigidbody2D �����������, �������� � ����������� ������
                    direction = (player.position - firePoint.position).normalized;
                }
            }
            else
            {
                // �������� � ����������� ������
                direction = (player.position - firePoint.position).normalized;
            }

            // ������� ������
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            projectile.AddComponent<ProjectileDamage>();
            EnemyStats enemystats = gameObject.GetComponent<EnemyStats>();
            projectile.GetComponent<ProjectileDamage>().damage = enemystats.attack_enemy;

            // �������� ��������� ������� � ������������� �����������
            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.SetDirection(direction);
            }
        }
        else
        {
            Debug.LogWarning("Projectile prefab or player is missing!");
        }
    }
}
