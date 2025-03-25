using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;
    private Vector2 direction;
    private Rigidbody2D rb;

    void Start()
    {
        instance = this;

        rb = GetComponent<Rigidbody2D>();
    }

}