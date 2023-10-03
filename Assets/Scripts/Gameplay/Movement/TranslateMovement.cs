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

    private Coroutine _movementHandler;

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

        this._owningObject = owningObject;
        _info = targetInfo;
        var position = this._owningObject.position;
        _movementDirection = (targetInfo.targetPosition - position).normalized;
    }

    public void Move(bool bMove)
    {
        if (bMove)
        {
            _movementHandler = StartCoroutine(MovementCoroutine());
        }
        else if (_movementHandler != null)
        {
            StopCoroutine(_movementHandler);
        }
    }

    IEnumerator MovementCoroutine()
    {
        for (;;)
        {
            if (Vector3.Distance(_info.targetPosition, _owningObject.position) < 0.1f)
            {
                OnTargetAchieved?.Invoke(this, EventArgs.Empty);
                yield break;
            }
            _owningObject.Translate(_movementDirection * (_info.movementSpeed * Time.deltaTime), Space.World);
            yield return null;
        }
    }
}
