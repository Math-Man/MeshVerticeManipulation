using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSoftTrack : MonoBehaviour
{

    [SerializeField] private Camera camera;
    [Range(0, 1.0f)] [SerializeField] private float cameraSmoothingMult = 0.125f;
    [Range(0, -1000f)] [SerializeField] private float ortographicZPosition = -10f;

    private void Start()
    {
        if (camera == null)
            camera = Camera.main;
    }

    private void LateUpdate()
    {
        cameraDragFollow();
    }

    private void cameraDragFollow()
    {
        var mousePosition = Input.mousePosition;
        Vector3 smoothedMousePos = Vector3.Lerp(transform.position, mousePosition, cameraSmoothingMult);
        smoothedMousePos.z = ortographicZPosition;
        camera.transform.position = smoothedMousePos;

    }

}
