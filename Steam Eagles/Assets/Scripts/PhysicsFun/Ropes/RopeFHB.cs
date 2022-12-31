// Game.Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Rope
using System;
using System.Collections.Generic;
using UnityEngine;


public class RopeFHB : MonoBehaviour
{
	private struct RopeSegment
	{
		public Vector2 currentPosition;

		public Vector2 previousPosition;

		public RopeSegment(Vector2 position)
		{
			previousPosition = position;
			currentPosition = position;
		}
	}

	
	[SerializeField]private bool useFixedSegmentLength = false;
	[SerializeField]private float fixedDistancePerSegment = .1f;
	
	[SerializeField]
	private int _segmentCount = 35;

	[SerializeField]
	private Transform _startPoint;

	[SerializeField]
	private Transform _endPoint;

	[SerializeField, Range(0.01f, 1f)]
	private float _ropeSegmentLength = 0.25f;

	private float _lineWidth = 0.1f;

	private List<RopeSegment> _ropeSegments = new List<RopeSegment>();

	private LineRenderer _lineRenderer;

	private LayerMask _playerLayer;

	private bool _passengerDetected;

	private RopeSegment _pivotSegment;

	
	public Transform StartPoint
	{
		get
		{
			if (_startPoint == null)
			{
				_startPoint = new GameObject($"Rope Start point ({name}").transform;
			}
			return _startPoint;
		}
		set
		{
			_startPoint = value;
		}
	}

	public Transform EndPoint
	{
		get
		{
			if (_endPoint == null)
			{
				_endPoint = new GameObject($"Rope Endpoint ({name}").transform;
			}
			return _endPoint;
		}
		set
		{
			_endPoint = value;
		}
	}

	public LineRenderer LineRenderer => _lineRenderer;

	public void SetRopeDistance(float maxDistance)
	{
		_ropeSegmentLength = maxDistance / (float)_segmentCount;
	}

	private void DoAll()
	{
		Awake();
		Start();
		Update();
	}

	private void Awake()
	{
		_lineRenderer = GetComponent<LineRenderer>();
	}

	private void Start()
	{
		Vector3 localPosition = _startPoint.localPosition;
		for (int i = 0; i < _segmentCount; i++)
		{
			_ropeSegments.Add(new RopeSegment(localPosition));
			localPosition.y -= _ropeSegmentLength;
		}
	}

	private void Update()
	{
		DrawRope();
		Simulate();
	}

	private void DrawRope()
	{
		try
		{
			Vector3[] array = new Vector3[_segmentCount];
			for (int i = 0; i < _segmentCount; i++)
			{
				array[i] = _ropeSegments[i].currentPosition;
			}
			_lineRenderer.positionCount = array.Length;
			_lineRenderer.SetPositions(array);
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
		}
	}

	private void Simulate()
	{
		try
		{
			Vector2 vector = new Vector2(0f, -1f);
			for (int i = 0; i < _segmentCount; i++)
			{
				RopeSegment value = _ropeSegments[i];
				Vector2 vector2 = value.currentPosition - value.previousPosition;
				value.previousPosition = value.currentPosition;
				value.currentPosition += vector2;
				value.currentPosition += vector * Time.deltaTime;
				_ropeSegments[i] = value;
			}
			for (int j = 0; j < 200; j++)
			{
				ApplyConstraints();
			}
		}
		catch (Exception value2)
		{
			Console.WriteLine(value2);
		}
	}

	private void ApplyConstraints()
	{
		RopeSegment value = _ropeSegments[0];
		value.currentPosition = _startPoint.position;
		_ropeSegments[0] = value;
		RopeSegment value2 = _ropeSegments[_segmentCount - 1];
		value2.currentPosition = _endPoint.position;
		_ropeSegments[_segmentCount - 1] = value2;
		for (int i = 0; i < _segmentCount - 1; i++)
		{
			RopeSegment value3 = _ropeSegments[i];
			RopeSegment value4 = _ropeSegments[i + 1];
			float num = (value3.currentPosition - value4.currentPosition).magnitude - _ropeSegmentLength;
			Vector2 vector = (value3.currentPosition - value4.currentPosition).normalized * num;
			if (i != 0)
			{
				value3.currentPosition -= vector * 0.5f;
				_ropeSegments[i] = value3;
				value4.currentPosition += vector * 0.5f;
				_ropeSegments[i + 1] = value4;
			}
			else
			{
				value4.currentPosition += vector;
				_ropeSegments[i + 1] = value4;
			}
		}
	}
}