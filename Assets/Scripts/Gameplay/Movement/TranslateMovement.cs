using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class TranslateMovement : MonoBehaviour
{
    [Serializable]
    public struct TargetInfo
    {
        public Vector3 targetPosition;
        public float movementSpeed;

        public TargetInfo(Vector3 targetPosition, float movementSpeed)
        {
            this.targetPosition = targetPosition;
            this.movementSpeed = movementSpeed;
        }
    }
    
    public event EventHandler OnTargetAchieved;
    
    private Transform _owningObject;
    private TargetInfo _info;
    private Vector3 _movementDirection;

    private bool _bMoving;

    private void Start()
    {
        StartCoroutine(MovementCoroutine());
    }

    public void SetupMovementComponent(TranslateMovement other)
    {
        _owningObject = other._owningObject;
        _info = other._info;
        _movementDirection = other._movementDirection;
    }
    public void SetupMovement(Transform owningObject, TargetInfo targetInfo)
    {
        if (!owningObject)
        {
            throw new Exception("Owning object passed in setup is invalid");
        }
        
        _info = targetInfo;
        _owningObject = owningObject;
        _movementDirection = (targetInfo.targetPosition - _owningObject.position).normalized;
    }

    public void Move(bool bMove)
    {
        _bMoving = bMove;
    }

    IEnumerator MovementCoroutine()
    {
        for (;;)
        {
            if (_bMoving)
            {
                if ((_info.targetPosition - _owningObject.position).magnitude < 0.1f)
                {
                    Move(false);
                    OnTargetAchieved?.Invoke(this, EventArgs.Empty);
                }

                _owningObject.Translate(_movementDirection * (_info.movementSpeed * Time.deltaTime), Space.World);
            }

            yield return null;
        }
    }
}
