using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class NPCDialotueManager : MonoBehaviour
{
    public GameObject panel;
    public void Close()
    {
        panel.SetActive(false);
    }
}
