using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class FixedRotation : MonoBehaviour
{
  

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.identity;
    }
}
