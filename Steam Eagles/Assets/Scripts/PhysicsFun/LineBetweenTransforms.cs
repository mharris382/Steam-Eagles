using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class LineBetweenTransforms : MonoBehaviour
{
    private LineRenderer _lr;
    private LineRenderer lr => _lr ? _lr : _lr = GetComponent<LineRenderer>();

    public bool realtimeUpdates = true;
    public Transform start;
    public Transform end;
    
    bool HasPoints => start && end;
    public Vector3
        offset;

    private void Update()
    {
        if (!HasPoints)
            return;
        
        var points = new Vector3[2];
        points[0] = start.position + offset;
        points[1] = end.position + offset;
        lr.positionCount = points.Length;
        lr.SetPositions(points);
    }
}