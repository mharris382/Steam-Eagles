using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class SimIORegistry : MonoBehaviour
{
    private BoxCollider2D _collider;

    private BoxCollider2D Collider => _collider ? _collider : _collider = GetComponent<BoxCollider2D>();


    public List<SimIOPoint> dynamicSources = new List<SimIOPoint>();
    private BoundsInt _simBounds;
    private Grid _grid;

    public int testSourceAmount = 1;
    public List<Vector2Int> testSources = new List<Vector2Int>();

    public IEnumerable<(Vector2Int cellSpacePos, int gasDelta)> GetSimIOSources()
    {
        foreach (var testSource in testSources)
        {
            if (_simBounds.Contains(new Vector3Int(testSource.x, testSource.y, 0)))
            {
                yield return (testSource, testSourceAmount);
            }
        }  
    }

    public IEnumerable<(Vector2Int cellSpacePos, int gasDelta)> GetSimIOSinks()
    {
        var bounds = _simBounds;
        var maxY = bounds.max.y-1;
        for (int x = 0; x < bounds.xMax; x++)
        {
            var cellPos = new Vector2Int(x, maxY);
            yield return (cellPos, 15);
        }
    }
    public void InitializeIOTracking(BoxCollider2D boundingArea, Grid grid, BoundsInt simBounds)
    {
        boundingArea.OnTriggerEnter2DAsObservable()
            .Select(t => t.GetComponent<SimIOPoint>())
            .Where(t => t != null)
            .Subscribe(OnIOEntered).AddTo(this);
        
        boundingArea.OnTriggerExit2DAsObservable()
            .Select(t => t.GetComponent<SimIOPoint>())
            .Where(t => t != null)
            .Subscribe(OnIOExited).AddTo(this);

        this._grid = grid;
        this._simBounds = simBounds;
    }

    void OnIOEntered(SimIOPoint simIOPoint)
    {
        Debug.Log($"SimIOPoint {simIOPoint.name} Entered", this);
    }

    void OnIOExited(SimIOPoint simIOPoint)
    {
        Debug.Log($"SimIOPoint {simIOPoint.name} Exited", this);
    }


    private void OnDrawGizmos()
    {
        if (_grid == null)
        {
            _grid = GetComponent<Grid>();
        }
        foreach (var testSource in testSources)
        {
            Gizmos.color = Color.blue;
            var wsPos = _grid.GetCellCenterWorld(new Vector3Int(testSource.x, testSource.y, 0));
            Gizmos.DrawCube(wsPos, _grid.cellSize);
        }
    }
}

public interface ISimIO
{
    Vector3 Position { get; }
    bool isActive { get; }
}
public interface IGasSink : ISimIO
{
    int TryRemoveGas(int amount);
}
public interface IGasSource : ISimIO
{
    int TryAddGas(int amount);
} 