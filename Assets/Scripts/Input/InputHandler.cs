using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputHandler : MonoBehaviour
{
    public event EventHandler<bool> touchStateChanged;

    private PlayerInput _playerInput;
    private InputAction _playerTouchPressAction;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerTouchPressAction = _playerInput.actions.FindAction("Interact");
    }

    private void OnEnable()
    {
        _playerTouchPressAction.performed += PlayerInteracted;
    }
    private void OnDisable()
    {
        _playerTouchPressAction.performed -= PlayerInteracted;
    }

    private void PlayerInteracted(InputAction.CallbackContext context)
    {
        touchStateChanged?.Invoke( this, context.control.IsPressed());
    }
}
