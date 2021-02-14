using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 mousePosition;

    [Range(0.001f, 1f)] [SerializeField] private float speedMult = 0.2f;
    [SerializeField] private Camera camera;
    [Range(0.01f, 0.2f)] [SerializeField] private float screenDelta;
    [SerializeField] private bool dragEnabled = false;
    void Start()
    {
        if (camera == null)
            camera = Camera.main;
    }

    void Update()
    {
        mousePosition = Input.mousePosition;

        if (Input.GetKey(KeyCode.D) || (mousePosition.x >= Screen.width - Screen.width * screenDelta && dragEnabled))
            transform.position -= new Vector3(-speedMult, 0, 0);
        if (Input.GetKey(KeyCode.A) || (mousePosition.x <= 0 + Screen.width * screenDelta && dragEnabled))
            transform.position -= new Vector3(speedMult, 0, 0);
        if (Input.GetKey(KeyCode.W) || (mousePosition.y >= Screen.height - Screen.height * screenDelta && dragEnabled))
            transform.position -= new Vector3(0, -speedMult, 0);
        if (Input.GetKey(KeyCode.S) || (mousePosition.y <= 0 + Screen.height * screenDelta && dragEnabled))
            transform.position -= new Vector3(0, speedMult, 0);

        if (Input.GetAxis("Mouse ScrollWheel") > 0f && camera.orthographicSize > 2f)
            camera.orthographicSize -= 0.1f;
        if (Input.GetAxis("Mouse ScrollWheel") < 0f && camera.orthographicSize < 10f)
            camera.orthographicSize += 0.1f;


    }
}
