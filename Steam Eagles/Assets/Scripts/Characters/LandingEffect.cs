using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UniRx;

public class LandingEffect : MonoBehaviour
{
    
    public UnityEvent onCharacterLanded;

    public CharacterState characterState;
    
    [Tooltip("How long the character needs to remain in the air for the effect to trigger on character becoming grounded")]
    public float fallSpeedToTriggerEffect = 0.2f;

    private float _timeEnteredAir = float.MaxValue;

    
    private float maxRecordedFallSpeed;
    
    private void Update()
    {
        if (!characterState.IsGrounded)
        {
            bool falling = characterState.VelocityY < 0;
            if (!falling) return;
            float fallSpeed = Mathf.Abs(characterState.VelocityY);
            if (maxRecordedFallSpeed < fallSpeed)
            {
                maxRecordedFallSpeed = fallSpeed;
            }
        }
    }

    private void Awake()
    {
        _timeEnteredAir = Time.time;
        characterState.IsGroundedEventStream.Subscribe(isGrounded =>
        {
            if (isGrounded)
            {
                if (maxRecordedFallSpeed > fallSpeedToTriggerEffect)
                {
                    onCharacterLanded?.Invoke();
                    maxRecordedFallSpeed = 0;
                }
            }
            else
            {
                _timeEnteredAir = Time.time;
            }
        });
    }
}
