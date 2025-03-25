using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{ 
    private Vector3 targetPosition;
    public float speed = 5f; // �������� �����������
    public float maxMovementDistance = 5f; // �

    private void Start()
    {
        targetPosition = transform.position; // �������� ������� ������
    }

    private void Update()
    {
        // ��������� ����� ����
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; // ��������� Z = 0 ��� 2D

            // ��������� ���������� �� ������� �������
            float distance = Vector3.Distance(transform.position, mousePos);

            // ���� ���������� ������ ������������� �����������, ������������ ������� �������
            if (distance > maxMovementDistance)
            {
                Vector3 direction = (mousePos - transform.position).normalized; // �����������
                targetPosition = transform.position + direction * maxMovementDistance; // ����� ������� ������� � ������ �����������
            }
            else
            {
                targetPosition = mousePos; // ��������� ������� �������, ���� ��� � �������� ������������� ����������
            }
        }

        // ����������� ������
        Move();
    }

    private void Move()
    {
        // ���������� ����������� � �����������
        float step = speed * Time.deltaTime; // ������ ����
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step); // ����������� ������
    }
}
