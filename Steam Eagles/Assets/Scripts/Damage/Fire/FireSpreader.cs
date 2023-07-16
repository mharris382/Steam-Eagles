using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Sirenix.OdinInspector;
using UnityEngine.VFX;

public class FireSpreader  : MonoBehaviour
{
	public VisualEffect visualEffect;
	private Bounds? _current;
	public FireSpreadConfig spreadConfig;
	
	private Coroutine _runningFire;
 
	
	[ShowInInspector, BoxGroup("Debug")]
	public Vector2 min
	{
		get => _current.HasValue ? _current.Value.min  : Vector2.zero;
	}
	
	[ShowInInspector, BoxGroup("Debug")]
	public Vector2 max
	{
		get => _current.HasValue ? _current.Value.max  : Vector2.zero;
	}
	
	[Serializable]
	public class FireSpreadConfig
	{
		public float spreadUpRate = 0.1f;
		public float spreadDownRate = 0.1f;
		public float spreadHorizontalRate = 0.1f;
	}
	
	public void StartFire(Vector3 startPoint, Bounds maxBounds)
	{
		if(_runningFire != null)
		{
			StopCoroutine(_runningFire);
		}
		_runningFire = StartCoroutine(RunFire(startPoint, maxBounds));
	}
	 public void StartFire(Vector3 point, SpriteRenderer sr)
	{
		var bounds = sr.bounds;
		var size = sr.size;
		var center = Vector2.zero;
		var min = center - size / 2f;
		var max = center + size / 2f;
		visualEffect.transform.SetParent(sr.transform, false);
		StartFire(point, new Bounds(min, max));
	}
	private IEnumerator RunFire(Vector3 startPoint, Bounds endBox)
	{
		Bounds currentBox = new Bounds(startPoint, Vector3.zero);
		float xStart = startPoint.x;
		float yStart = startPoint.y;
		float xMax, yMax, xMin, yMin;
		xMax = xMin = xStart;
		yMin = yMax = yStart;
		visualEffect.Play();
	
		while(true)
		{
			yield return null;
			
			if(MoveTowardsMin(ref xMin, endBox.max.x, spreadConfig.spreadHorizontalRate) &&
				MoveTowardsMax(ref xMax, endBox.max.x, spreadConfig.spreadHorizontalRate) &&
				MoveTowardsMin(ref yMin, endBox.min.y, spreadConfig.spreadUpRate) &&
				MoveTowardsMax(ref yMax, endBox.max.y, spreadConfig.spreadDownRate))
			{
				break;
			}
			UpdateBox();
			
		}
		
		void UpdateBox()
		{
			currentBox.Encapsulate(new Vector3(xMin, yMin, 0));
			currentBox.Encapsulate(new Vector3(xMax, yMax, 1));	
			UpdateEffect(currentBox);
		}
		
		bool MoveTowardsMin(ref float current, float end, float speed)
		{
			current -= speed * Time.deltaTime;
			return current <= end;
		}
		bool MoveTowardsMax(ref float current, float end, float speed)
		{
			current += speed * Time.deltaTime;
			return current >= end;
		}
	
	}

	
	

	void UpdateEffect(Bounds box)
	{
		_current = box;
		Vector3 center = (box.min + box.max) / 2f;
		Vector3 size = box.max - box.min;
		center.z = 0;
		size.z = 1;
		visualEffect.SetVector3("boxCenter", center);
		visualEffect.SetVector3("boxSize", size);
	}
}
