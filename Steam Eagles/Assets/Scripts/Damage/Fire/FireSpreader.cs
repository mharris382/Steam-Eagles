using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Dest.Math;
using Sirenix.OdinInspector;
using UnityEngine.VFX;

public class FireSpreader  : MonoBehaviour
{
	public VisualEffect visualEffect;
	
	public FireSpreadConfig spreadConfig;
	
	private Coroutine _runningFire;
 
	[Serializable]
	public class FireSpreadConfig
	{
		public float spreadUpRate = 0.1f;
		public float spreadDownRate = 0.1f;
		public float spreadHorizontalRate = 0.1f;
	}
	
	public void StartFire(Vector3 startPoint, AAB2 maxBounds)
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
		StartFire(point, new AAB2(min, max));
	}
	private IEnumerator RunFire(Vector3 startPoint, AAB2 endBox)
	{
		AAB2 currentBox = AAB2.CreateFromPoint(startPoint);
		float xStart = startPoint.x;
		float yStart = startPoint.y;
		float xMax, yMax, xMin, yMin;
		xMax = xMin = xStart;
		yMin = yMax = yStart;
		visualEffect.Play();
	
		while(true)
		{
			yield return null;
			
			if(MoveTowardsMin(ref xMin, endBox.Min.x, spreadConfig.spreadHorizontalRate) &&
				MoveTowardsMax(ref xMax, endBox.Max.x, spreadConfig.spreadHorizontalRate) &&
				MoveTowardsMin(ref yMin, endBox.Min.y, spreadConfig.spreadUpRate) &&
				MoveTowardsMax(ref yMax, endBox.Max.y, spreadConfig.spreadDownRate))
			{
				break;
			}
			UpdateBox();
			
		}
		
		void UpdateBox()
		{
			currentBox.Include(new Vector2(xMin, yMin));
			currentBox.Include(new Vector2(xMax, yMax));	
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

	private AAB2? _current;
	
	[ShowInInspector, BoxGroup("Debug")]
	public Vector2 min
	{
		get => _current.HasValue ? _current.Value.Min  : Vector2.zero;
	}
	[ShowInInspector, BoxGroup("Debug")]
	public Vector2 max
	{
		get => _current.HasValue ? _current.Value.Max  : Vector2.zero;
	}
	void UpdateEffect(AAB2 box)
	{
		_current = box;
		Vector3 center = (box.Min + box.Max) / 2f;
		Vector3 size = box.Max - box.Min;
		center.z = 0;
		size.z = 1;
		visualEffect.SetVector3("boxCenter", center);
		visualEffect.SetVector3("boxSize", size);
	}
}
