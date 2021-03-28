using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLooking : MonoBehaviour
{
    // Looking Variables
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private float lookSensitivity;
    [SerializeField] private float minXLook, maxXLook;
    [SerializeField] private bool invertCameraXRot;
    private float curXRot;

    // Global enable mouse movement rotate screen bool
    public static bool LOOKING_ENABLED = true;

    private void LateUpdate()
    {
        if (!LOOKING_ENABLED)
            return;

        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        transform.eulerAngles += Vector3.up * x * lookSensitivity;

        if (invertCameraXRot)
            curXRot += y * lookSensitivity;
        else
            curXRot -= y * lookSensitivity;

        curXRot = Mathf.Clamp(curXRot, minXLook, maxXLook);

        Vector3 clampedAngle = cameraAnchor.eulerAngles;
        clampedAngle.x = curXRot;
        cameraAnchor.eulerAngles = clampedAngle;
    }
}
