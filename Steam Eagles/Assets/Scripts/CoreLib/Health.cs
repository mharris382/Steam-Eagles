using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{

    public bool IsDead
    {
        get => false;
        set
        {
            Debug.LogWarning("Health is not implemented yet!",this);
        }
    }
}
