using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
[AddComponentMenu("Steam Eagles/BlockMap")]
public class BlockMap : MonoBehaviour
{
    public BlockMapType type;
    

    private Tilemap _tilemap;
    public Tilemap Tilemap => _tilemap != null ? _tilemap : (_tilemap = GetComponent<Tilemap>());

    private void Awake()
    {
        type.ActivateMap(this);
    }
}


