using System;
using UnityEngine;

public class PlayerCameras : MonoBehaviour, IPlayerDependencyResolver<Camera>
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

    public Camera GetAny()
    {
        foreach (Camera cam in cameras)
        {
            if (cam.gameObject.activeSelf)
            {
                return cam;
            }
        }

        var first = cameras[0];
        first.gameObject.SetActive(true);
        return first;
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
    
    public Camera GetDependency(int playerNumber)
    {
        return GetCamera(playerNumber);
    }
}


