using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LockCameraRotation : MonoBehaviour
{
    public Transform cameraTransform;

    private void Update()
    {
        if(cameraTransform != null)
            cameraTransform.rotation = Quaternion.identity;
    }
}
