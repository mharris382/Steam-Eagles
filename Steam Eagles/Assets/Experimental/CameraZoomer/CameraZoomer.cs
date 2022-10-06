using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraZoomer : MonoBehaviour
{

    public float minZoom = 1;
    public float maxZoom = 10;
    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0,0,1,1);
    
    void Update()
    {
        var cameras = GetComponentsInChildren<Camera>();
        for (int i = 0; i < cameras.Length; i++)
        {
            var t = (i + 1) / (float)cameras.Length;
            cameras[i].orthographicSize = Mathf.Lerp(minZoom, maxZoom, zoomCurve.Evaluate(t));
        }
    }
}
