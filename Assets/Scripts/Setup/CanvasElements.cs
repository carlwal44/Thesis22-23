using UnityEngine;
using System.Collections;

public class CanvasElements : MonoBehaviour
{
    private Canvas CanvasObject; // Assign in inspector

    void Start()
    {
        CanvasObject = GetComponent<Canvas>();
        CanvasObject.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.T))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        CanvasObject.enabled = !CanvasObject.enabled;
    }
}
