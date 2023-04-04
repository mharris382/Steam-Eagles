using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapMaterial : MonoBehaviour
{
    public Material MATTTERAIL;
    private TilemapRenderer __TRRENDER;
    private Material _matINSTYACNCE;
    private static readonly int Offswt = Shader.PropertyToID("_OFFSWT");

    // Start is called before the first frame update
    void Start()
    {
        __TRRENDER = GetComponent<TilemapRenderer>();
        _matINSTYACNCE = new Material(MATTTERAIL);
        __TRRENDER.material = _matINSTYACNCE;
    }

    // Update is called once per frame
    void Update()
    {
        _matINSTYACNCE.SetVector(Offswt, transform.position);
    }
}
