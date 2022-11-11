using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPressureGauge : MonoBehaviour
{
    public Transform needle;
    public float maxAngle = 0f;
    public float minAngle = 180f;
    public int maxPressure = 100;

    public float speed = 10;
    private float _targetAngle = 0;

    private float _startAngle = 0;

    private float CurrentAngle
    {
        get=> needle.localEulerAngles.z;
        set=> needle.localEulerAngles = new Vector3(0, 0, value);
    }
    private IEnumerator _smoothingCoroutine;

    private void Start()
    {
        SetPressure(0);
    }

    public void SetPressure(int pressure)
    {
        
        _targetAngle = Mathf.Lerp(minAngle, maxAngle, (float)pressure / maxPressure);
        if (_smoothingCoroutine != null)
        {
            return;
        }
        _smoothingCoroutine = SmoothAngle();
        StartCoroutine(_smoothingCoroutine);
    }

    


    IEnumerator SmoothAngle()
    {
        while(Math.Abs(CurrentAngle - _targetAngle) > Mathf.Epsilon)
        {
            CurrentAngle = Mathf.MoveTowardsAngle(CurrentAngle, _targetAngle, Time.deltaTime * speed);
            yield return null;
        }
        _smoothingCoroutine = null;
    }


    public float AmountFilled
    {
        get
        {
            throw new NotImplementedException();
        }
    }
}
