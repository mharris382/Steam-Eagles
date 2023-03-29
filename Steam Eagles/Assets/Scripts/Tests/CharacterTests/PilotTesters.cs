using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using CoreLib.Interactions;
using UniRx;
using UnityEngine;

public class PilotTesters : MonoBehaviour
{
    public string characterName = "Builder";
    public string characterName2 = "Transporter";
    public KeyCode gainControlKey = KeyCode.O;
    public KeyCode exitControlKey = KeyCode.K;
    public KeyCode takeControlKey = KeyCode.N;
    
    public AirshipControls controls;


    private bool hasControl = false;
    private void Awake()
    {
        Debug.Assert(controls != null, this);
        hasControl = false;
    }
    
    private void Update()
    {
        if ((controls.CurrentPilot == null || controls.CurrentPilot.tag != characterName) && Input.GetKeyDown(gainControlKey))
        {
            MessageBroker.Default.Publish(new AirshipPilotChangedInfo( characterName, controls));
        }

        if (controls.CurrentPilot == null) return;
        
        if (controls.CurrentPilot.tag == characterName && Input.GetKeyDown(exitControlKey))
        {
            MessageBroker.Default.Publish(new AirshipPilotChangedInfo("", controls));
        }
        if (controls.CurrentPilot.tag == characterName && Input.GetKeyDown(takeControlKey))
        {
            MessageBroker.Default.Publish(new AirshipPilotChangedInfo(characterName, characterName2, controls));
        }
    }
}
