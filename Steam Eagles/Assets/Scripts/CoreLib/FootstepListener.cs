using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FootstepListener : MonoBehaviour
{
    public UnityEvent footstepRunning;
    public UnityEvent footstepJump;
    public UnityEvent footstepLand;
    public void OnFootstepRunning()
    {
        footstepRunning?.Invoke();
    }

    public void OnFootstepJump()
    {
        footstepJump?.Invoke();
    }

    public void OnFootstepFall()
    {
        footstepLand?.Invoke();
    }
}
