using System;
using System.Collections;
using System.Collections.Generic;
using CharactersInZone = CharacterTriggerArea.CharactersInZone ;
using UnityEngine;

[RequireComponent(typeof(CharacterTriggerArea))]
public class CharacterCameraZoneTrigger : MonoBehaviour
{
    private static CharacterCameraSwitcher _cameraController;
    private static int _zoneTriggerCount = 0;
    
    private CharacterCameraSwitcher cameraController
    {
        get
        {
            if (_cameraController == null) _cameraController = FindObjectOfType<CharacterCameraSwitcher>();
            return _cameraController;
        }
    }

    private CharacterTriggerArea _triggerArea;
    private bool _bothCharacterInZone;
    
    public void OnCharactersInZoneChanged(CharactersInZone charactersInZone)
    {
        bool hasTP = (charactersInZone & CharactersInZone.TRANSPORTER) > 0;
        bool hasBD = (charactersInZone & CharactersInZone.BUILDER) > 0;
        _bothCharacterInZone = hasTP && hasBD;
        if(enabled) cameraController.forceSingleCameraMode = _bothCharacterInZone;
    }

    private void Awake()
    {
        _bothCharacterInZone = false;
        _triggerArea = GetComponent<CharacterTriggerArea>();
        _triggerArea.onCharactersInAreaChanged.AddListener(OnCharactersInZoneChanged);
    }

    private void OnEnable()
    {
        cameraController.forceSingleCameraMode = _bothCharacterInZone;
        _zoneTriggerCount++;
    }

    private void OnDisable()
    {
        _zoneTriggerCount--;
    }

    private void OnDestroy()
    {
        _triggerArea.onCharactersInAreaChanged.AddListener(OnCharactersInZoneChanged);
        if (_zoneTriggerCount == 0) _cameraController = null;
    }
}
