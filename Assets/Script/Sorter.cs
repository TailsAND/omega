using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sorter : MonoBehaviour
{
    public bool isStatic = false;
    public float offset = 0;
    private int sortingOrderBase = 0;
    private new Renderer renderer;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
    }
    private void LateUpdate()
    {
        renderer.sortingOrder = (int)(sortingOrderBase - transform.position.y + offset);
    
        if (isStatic)
            Destroy(this);
    }
}
