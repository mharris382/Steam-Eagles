using System;
using System.Collections;
using System.Collections.Generic;
using SteamEagles.Characters;
using UnityEngine;
using UnityEngine.Events;
using UniRx;
using UnityEngine.Serialization;

public class LandingEffect : MonoBehaviour
{
    
    public UnityEvent onCharacterLanded;

    [FormerlySerializedAs("character")] public CharacterState characterState;
    
    [Tooltip("How long the character needs to remain in the air for the effect to trigger on character becoming grounded")]
    public float fallSpeedToTriggerEffect = 0.2f;

    private float _timeEnteredAir = float.MaxValue;
    public float timeInAirToTrigger = 1;
    
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
        characterState.IsGroundedEventStream
            .Select(t => t ? Observable.EveryUpdate().AsUnitObservable() : Observable.Never<Unit>())
            .Switch()
            .TakeUntilDestroy(this)
            .Subscribe(
                _ =>
                {
                    bool falling = characterState.VelocityY < 0;
                    if (!falling) return;
                    float fallSpeed = Mathf.Abs(characterState.VelocityY);
                    if (maxRecordedFallSpeed < fallSpeed)
                    {
                        maxRecordedFallSpeed = fallSpeed;
                    }
                });
        characterState.IsGroundedEventStream
            .Where(t => t)
            .Select(t => Observable.EveryUpdate().AsUnitObservable())
            .Take(TimeSpan.FromSeconds(timeInAirToTrigger))
            .TakeUntil(characterState.IsGroundedEventStream)
            .Subscribe(_ =>
            {
                Debug.Log("Updating stuffs.0..");
            }, () =>
            {
                if(characterState.IsGrounded)
                    Debug.Log($"Character Spent {timeInAirToTrigger} time in air");
                return;
            });

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
