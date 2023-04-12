using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UniRx;
using UnityEngine.Serialization;

public class LandingEffect : MonoBehaviour
{
    
    public UnityEvent onCharacterLanded;

    [FormerlySerializedAs("characterState")] public Character character;
    
    [Tooltip("How long the character needs to remain in the air for the effect to trigger on character becoming grounded")]
    public float fallSpeedToTriggerEffect = 0.2f;

    private float _timeEnteredAir = float.MaxValue;
    public float timeInAirToTrigger = 1;
    
    private float maxRecordedFallSpeed;
    
    private void Update()
    {
        if (!character.IsGrounded)
        {
            bool falling = character.VelocityY < 0;
            if (!falling) return;
            float fallSpeed = Mathf.Abs(character.VelocityY);
            if (maxRecordedFallSpeed < fallSpeed)
            {
                maxRecordedFallSpeed = fallSpeed;
            }
        }
    }

    private void Awake()
    {
        _timeEnteredAir = Time.time;
        character.IsGroundedEventStream
            .Select(t => t ? Observable.EveryUpdate().AsUnitObservable() : Observable.Never<Unit>())
            .Switch()
            .TakeUntilDestroy(this)
            .Subscribe(
                _ =>
                {
                    bool falling = character.VelocityY < 0;
                    if (!falling) return;
                    float fallSpeed = Mathf.Abs(character.VelocityY);
                    if (maxRecordedFallSpeed < fallSpeed)
                    {
                        maxRecordedFallSpeed = fallSpeed;
                    }
                });
        character.IsGroundedEventStream
            .Where(t => t)
            .Select(t => Observable.EveryUpdate().AsUnitObservable())
            .Take(TimeSpan.FromSeconds(timeInAirToTrigger))
            .TakeUntil(character.IsGroundedEventStream)
            .Subscribe(_ =>
            {
                Debug.Log("Updating stuffs.0..");
            }, () =>
            {
                if(character.IsGrounded)
                    Debug.Log($"Character Spent {timeInAirToTrigger} time in air");
                return;
            });

    character.IsGroundedEventStream.Subscribe(isGrounded =>
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
