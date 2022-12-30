using System;
using UnityEngine;

public class PlayerCameras : MonoBehaviour
{
    [SerializeField]
    private Camera[] cameras;

    private void OnEnable()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].enabled = true;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].enabled = false;
        }
    }

    public Camera GetCamera(int playerNumber)
    {
        const string ERROR_MSG = "Player number must be 0 or 1";
        switch( playerNumber)
        {
            case 0:
                return cameras[0];
            case 1:
                return cameras[1];
            default:
                Debug.LogError(ERROR_MSG, this);
                throw new IndexOutOfRangeException(ERROR_MSG + " " + playerNumber);
        }
    }
}