using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ConstrainRotation : MonoBehaviour
{

    public float angle = 0;


    private void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
