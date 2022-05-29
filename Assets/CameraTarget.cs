using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    public Camera camera;
    private Vector3 previousMousePos;
    private Vector3 currentMousePos;
    private bool mouseDown = false;
    Vector3 targetPos;
    [SerializeField] [Range(0, 10)] private float lerpSpeed=1;

    private void Start()
    {
        targetPos = transform.position;
    }

    private void Update()
    {
        currentMousePos = camera.ScreenToWorldPoint(Input.mousePosition);
        currentMousePos.z = 0;
        if (Input.GetMouseButtonDown(0))
        {
            mouseDown = true;
            previousMousePos = currentMousePos;
            targetPos = transform.position;
        }
        if (mouseDown)
        {
            targetPos = transform.position -currentMousePos + previousMousePos;
        }
        if (Input.GetMouseButtonUp(0))
        {
            mouseDown = false;
        }
        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            lerpSpeed * Mathf.Min(Time.deltaTime, 1f));
    }
}
