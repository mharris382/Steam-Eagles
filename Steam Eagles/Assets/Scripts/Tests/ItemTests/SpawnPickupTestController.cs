using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib.Pickups;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class SpawnPickupTestController : MonoBehaviour
{
    public float moveSpeed = 5;
    public Pickup pickup;

    
    [AssetList(AutoPopulate = true)]
    public List<Pickup> pickups;


    [SerializeField]
    private bool showInlineEditors;

    public TextMeshProUGUI pickupLabel;


    private Pickup currentPickup => pickups[index];
    private int index = 0;
    void Next()
    {
        index++;
        if (index >= pickups.Count)
        {
            index = 0;
        }
        UpdateLabel();
    }

    void Prev()
    {
        index--;
        if (index < 0)
        {
            index = pickups.Count - 1;
        }
        UpdateLabel();
    }

    private void Awake()
    {
        if (pickups.Count == 0)
        {
            Debug.LogError("Need 1 or more pickups");
            enabled = false;
            return;
        }
        index = 0;
        pickup = pickups[index];
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        pickupLabel.text = currentPickup.name;
    }

    private void Update()
    {
        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");
        var move = new Vector3(x, y, 0) * (moveSpeed * Time.deltaTime);
        transform.position += move;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Prev();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Next();
        }
        
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        if (Input.GetMouseButtonUp(0))
        {
            var instance = pickup.SpawnPickup(mousePos);
        }
    }
}
