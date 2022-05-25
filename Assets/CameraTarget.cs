using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    public Camera camera;
    private Vector3 previousMousePos;
    private Vector3 currentMousePos;
    private bool mouseDown = false;

    private void Update()
    {
        currentMousePos = camera.ScreenToWorldPoint(Input.mousePosition);
        currentMousePos.z = 0;
        if (Input.GetMouseButtonDown(0))
        {
            mouseDown = true;
            previousMousePos = currentMousePos;
        }
        if (mouseDown)
        {
            transform.position -= currentMousePos - previousMousePos;
        }
        if (Input.GetMouseButtonUp(0))
        {
            mouseDown = false;
        }
    }
}
