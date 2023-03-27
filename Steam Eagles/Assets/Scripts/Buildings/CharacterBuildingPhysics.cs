using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using UnityEngine;

public class CharacterBuildingPhysics : MonoBehaviour
{
    Collider2D[] _colliders;
    private DynamicBodyState _dynamicBody;
    private Collider2D _buildingCollider;
    private Rigidbody2D _buildingRigidbody;
    private Collider2D _roomCollider;
    private Rigidbody2D _roomRigidbody;
    private void Start()
    {
        _dynamicBody = GetComponent<DynamicBodyState>();
        _colliders = new Collider2D[10];
    }

    // Update is called once per frame
    void Update()
    {
        int hits = Physics2D.OverlapPointNonAlloc(transform.position, _colliders);
        _buildingCollider = null;
        _buildingRigidbody = null;
        _roomCollider = null;
        _roomRigidbody = null;
        for (int i = 0; i < hits; i++)
        {
            if (_colliders[i].gameObject.CompareTag("Building"))
            {
                _buildingCollider = _colliders[i];
                _buildingRigidbody = _buildingCollider.attachedRigidbody;

            }
            else if (_colliders[i].gameObject.CompareTag("Room"))
            {
                
                _roomCollider = _colliders[i];
                _roomRigidbody = _roomCollider.attachedRigidbody;
            }
        }

        _dynamicBody.BuildingBody = _buildingRigidbody;
        _dynamicBody.RoomBody = _roomRigidbody;
        
    }
    
    
}
