using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyPatrol : MonoBehaviour
{
    public Transform[] patrolPoints; // ������ ����� ��������������
    public float speed = 2f; // �������� �������� �����
    public float waitTime = 5f; // ����� �������� �� ������ �����
    public float detectionRange = 5f; // ��������� ����������� ������
    public Transform player; // ������ �� ������
    public float chaseDuration = 5f; // ����� ������������� ����� ��������� �� ����� ���
    private int currentPointIndex = 0; // ������ ������� ����� ��������������
    private bool isChasing = false; // ���� ��� ������������ ��������� �������������
    private Animator animator; // ������ �� ��������� Animator

    private void Start()
    {
        animator = GetComponent<Animator>(); // �������� ��������� Animator
        StartCoroutine(Patrol());
    }

    private void Update()
    {
        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            DetectPlayer();
        }

        // ��������� �������� � ����������� �� ��������
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        float currentSpeed = 0f;
        float horizontal = 0f;
        float vertical = 0f;

        if (!isChasing)
        {
            // ���� ���� �����������, ���������� ����������� � ������� ����� ��������������
            if (patrolPoints.Length > 0)
            {
                Transform targetPoint = patrolPoints[currentPointIndex];
                Vector2 direction = (targetPoint.position - transform.position).normalized;
                currentSpeed = (targetPoint.position - transform.position).magnitude > 0.1f ? speed : 0f;

                // ������������� �������� horizontal � vertical
                horizontal = direction.x;
                vertical = direction.y;

                transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, currentSpeed * Time.deltaTime);
            }
        }
        else
        {
            // ���� ���� ���������� ������
            currentSpeed = speed;
            Vector2 direction = (player.position - transform.position).normalized;

            // ������������� �������� horizontal � vertical
            horizontal = direction.x;
            vertical = direction.y;

            transform.position = Vector2.MoveTowards(transform.position, player.position, currentSpeed * Time.deltaTime);
        }

        // ������������� ��������� Speed, Horizontal � Vertical � Animator
        animator.SetFloat("Speed", currentSpeed);
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
    }


    private void DetectPlayer()
    {
        // ��������� ���������� �� ������
        if (Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            isChasing = true; // �������� �������������
            StartCoroutine(ChaseTimer()); // ��������� ������ �������������
        }
    }

    private void ChasePlayer()
    {
        // ��������� � ������
        if (Vector2.Distance(transform.position, player.position) < 0.1f)
        {
            // ������� �� ����� ���
            SceneManager.LoadScene("Battle scene"); // �������� "BattleScene" �� ��� ����� ����� ���
        }
    }

    private IEnumerator Patrol()
    {
        while (!isChasing)
        {
            if (patrolPoints.Length == 0) yield break;

            Transform targetPoint = patrolPoints[currentPointIndex];
            while (Vector2.Distance(transform.position, targetPoint.position) > 0.1f)
            {
                yield return null; // ���� ���������� �����
            }

            // ������������� ��������
            yield return new WaitForSeconds(waitTime); // ���� ����� ��������
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length; // ����������� �������
        }
    }

    private IEnumerator ChaseTimer()
    {
        yield return new WaitForSeconds(chaseDuration);
        isChasing = false; // ������������� ������������� ����� �������
        StartCoroutine(Patrol()); // ������������ � ��������������
    }
}
