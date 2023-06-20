using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class TranslateMovement : MonoBehaviour
{
    [SerializeField] private Transform owningObject;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private float movementSpeed;
    private Vector3 movementDirectionStep;

    public delegate void OnTargetPointAchieved();
    public event OnTargetPointAchieved OnTargetAchieved;
    
    private Coroutine movementHandler;
    private bool initialPositionDifferenceSign;
    

    public void SetupMovement(Transform _owningObject)
    {
        if (_owningObject == null)
        {
            throw new Exception("Owning object passed in setup is invalid");
        }

        owningObject = _owningObject;
        var position = owningObject.position;
        movementDirectionStep = (targetPosition - position) / movementSpeed / 30;
        initialPositionDifferenceSign = Vector3.Magnitude(targetPosition - position) > 0;
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
            if (Vector3.Magnitude(targetPosition - owningObject.position) > 0 != initialPositionDifferenceSign)
            {
                OnTargetAchieved?.Invoke();
                yield break;
            }
            owningObject.Translate(movementDirectionStep, Space.World);
            yield return null;
        }
    }
}
