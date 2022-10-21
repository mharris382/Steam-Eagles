using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInputManager : MonoBehaviour
{
    private PlayerInputManager inputManager;
    public PlayerInput transporter;
    public PlayerInput builder;

    // Start is called before the first frame update
    void Start()
    {
        this.inputManager = GetComponent<PlayerInputManager>();
        inputManager.playerPrefab = transporter.gameObject;
        var p1  = inputManager.JoinPlayer(controlScheme: "Keyboard");
        inputManager.playerPrefab = builder.gameObject;
        var p2 = inputManager.JoinPlayer(controlScheme: "Keyboard 2");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
