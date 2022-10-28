using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using GasSim.SimCore.DataStructures;
using Puzzles;
using UniRx;
using UnityEngine;

public class StartPoint : CellHelper
{

    public bool IsConnected
    {
        get;
        set;
    }


    private LinkedList<Vector3Int> pipePath = new LinkedList<Vector3Int>();

    public Gradient connectedColor = new Gradient()
    {
        alphaKeys = new[] { new GradientAlphaKey(1, 1), new GradientAlphaKey(0, 1) },
        colorKeys = new[] { new GradientColorKey(Color.black, 0), new GradientColorKey(Color.black, 1) }
    };
    public Gradient disconnectedColor = new Gradient()
    {
        alphaKeys = new[] { new GradientAlphaKey(1, 1), new GradientAlphaKey(0, 1) },
        colorKeys = new[] { new GradientColorKey(Color.black, 0), new GradientColorKey(Color.black, 1) }
    };
    public LineRenderer lineRenderer;
    public Vector2 gridScale = new Vector2(2, 2);
    private Graph<Vector3Int> _graph;
    private HashSet<Vector3Int> _relaventNeighbors = new HashSet<Vector3Int>();
    public IEnumerable<Vector3Int> GetPath() => pipePath;
    public bool IsFullyConnected()
    {
        return _connectedEndPoint != null;
    }

    private EndPoint _connectedEndPoint;

    private EndPoint connectedEndPoint
    {
        get => _connectedEndPoint;
        set
        {
            if (_connectedEndPoint != value)
            {
                if(_connectedEndPoint!=null)_connectedEndPoint.NotifyDisconnectedFrom(this);
                _connectedEndPoint = value;
                if(_connectedEndPoint != null)_connectedEndPoint.NotifyConnectedTo(this);
                
            }
        }
    }
    
    
    private void Start()
    {
        MessageBroker.Default.Receive<BuildActionInfo>().TakeUntilDestroy(this).Subscribe(OnConnect);
        MessageBroker.Default.Receive<DisconnectActionInfo>().TakeUntilDestroy(this).Subscribe(OnDisconnect);
       UpdatePath();
    }

    void UpdatePath()
    {
        var current = this.CellCoordinate;
        pipePath.Clear();
        pipePath.AddLast(current);
        List<Vector3Int> path = new List<Vector3Int>();
        _graph = CellHelper.BuildFrom(CellCoordinate, this.Tilemap, out path);
        bool hasConnectedEndpoint = false;
        
        foreach (var vector3Int in path)
        {
            pipePath.AddLast(vector3Int);
            if (EndPoint.registeredEndpoints.ContainsKey(vector3Int))
            {
                hasConnectedEndpoint = true;
                connectedEndPoint = EndPoint.registeredEndpoints[vector3Int];
            }
        }

        if (!hasConnectedEndpoint && connectedEndPoint != null)
        {
            connectedEndPoint = null;
        }
        
        DrawPath();
    }

    void DrawPath()
    {
        List<Vector3> points = new List<Vector3>();
        foreach (var vector3Int in pipePath)
        {
            points.Add(this.Tilemap.GetCellCenterWorld(vector3Int));
        }

        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
        lineRenderer.colorGradient = IsFullyConnected() ? connectedColor : disconnectedColor;
    }

    void OnDisconnect(DisconnectActionInfo disconnectActionInfo)
    {
        UpdatePath();
    }

    void OnConnect(BuildActionInfo buildActionInfo)
    {
        UpdatePath();
    }
}
