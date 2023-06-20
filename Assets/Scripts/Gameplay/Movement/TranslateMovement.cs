using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class TranslateMovement : MonoBehaviour
{
    [Serializable]
    public struct TargetInfo
    {
        public Vector3 targetPosition;
        public float movementTime;

        public TargetInfo(Vector3 _targetPosition, float _movementTime)
        {
            targetPosition = _targetPosition;
            movementTime = _movementTime;
        }
    }
    
    private Transform owningObject;
    private TargetInfo info;
    private Vector3 movementDirectionStep;

    public delegate void OnTargetPointAchieved();
    public event OnTargetPointAchieved OnTargetAchieved;
    
    private Coroutine movementHandler;

    public void SetupMovementComponent(TranslateMovement other)
    {
        owningObject = other.owningObject;
        info = other.info;
        movementDirectionStep = other.movementDirectionStep;
    }
    public void SetupMovement(Transform _owningObject, TargetInfo _targetInfo)
    {
        if (!_owningObject)
        {
            throw new Exception("Owning object passed in setup is invalid");
        }

        owningObject = _owningObject;
        info = _targetInfo;
        var position = owningObject.position;
        movementDirectionStep = (_targetInfo.targetPosition - position) / _targetInfo.movementTime / 30;
    }

    public void ManageMovement(bool bMove)
    {
        if (bMove)
        {
            movementHandler = StartCoroutine(MovementCoroutine());
        }
        else if (movementHandler != null)
        {
            StopCoroutine(movementHandler);
        }
    }

    IEnumerator MovementCoroutine()
    {
        for (;;)
        {
            if (Vector3.Distance(info.targetPosition, owningObject.position) < 0.1f)
            {
                OnTargetAchieved?.Invoke();
                yield break;
            }
            owningObject.Translate(movementDirectionStep, Space.World);
            yield return null;
        }
    }
}
