using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputHandler : MonoBehaviour
{
    public delegate void TouchInputFired(bool bBegan);
    public event TouchInputFired touchStateChanged;

    private PlayerInput playerInput;
    private InputAction playerTouchPressAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerTouchPressAction = playerInput.actions.FindAction("Interact");
    }

    private void OnEnable()
    {
        playerTouchPressAction.performed += PlayerInteracted;
        playerTouchPressAction.canceled += PlayerInteractionEnded;
    }

    private void OnDisable()
    {
        playerTouchPressAction.performed -= PlayerInteracted;
        playerTouchPressAction.canceled -= PlayerInteractionEnded;
    }

    private void PlayerInteracted(InputAction.CallbackContext context)
    {
        touchStateChanged?.Invoke(true);
    }
    private void PlayerInteractionEnded(InputAction.CallbackContext context)
    {
        touchStateChanged?.Invoke(false);
    }
}
