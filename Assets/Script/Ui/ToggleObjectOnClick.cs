using UnityEngine;

public class ToggleObjectOnClick : MonoBehaviour
{
    public GameObject targetObject; 

    public void ToggleObject()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(!targetObject.activeSelf);  
        }
    }
}