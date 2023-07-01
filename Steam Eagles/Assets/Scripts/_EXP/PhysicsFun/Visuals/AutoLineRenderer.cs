using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public abstract class AutoLineRenderer : MonoBehaviour
{
    private LineRenderer _lr;
    private LineRenderer lr => _lr ? _lr : _lr = GetComponent<LineRenderer>();
    public abstract Vector3 GetEndPoint();
    public abstract Vector3 GetStartPoint();


    private void Update()
    {
        var endPoint = GetEndPoint();
        var startPoint = GetStartPoint();
        lr.positionCount = 2;
        lr.SetPosition(0, startPoint);
        lr.SetPosition(1, endPoint);
    }
}