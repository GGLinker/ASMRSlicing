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
    }
    private void OnDisable()
    {
        playerTouchPressAction.performed -= PlayerInteracted;
    }

    private void PlayerInteracted(InputAction.CallbackContext context)
    {
        Debug.Log("++++ " + context.control.IsPressed());
        touchStateChanged?.Invoke(context.control.IsPressed());
    }
}
