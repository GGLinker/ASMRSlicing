using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerInput))]
public class InputHandler : MonoBehaviour
{
    public event EventHandler<bool> touchStateChanged;

    #region Singleton

    public static InputHandler Instance { get; private set; }    

    #endregion

    private PlayerInput _playerInput;
    private InputAction _playerTouchPressAction;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Singleton violation: " + gameObject.name);
        }
        Instance = this;
        
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
