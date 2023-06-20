using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(TranslateMovement))]
public class KnifeMovement : MonoBehaviour
{
    public delegate void MotionEnded();
    public event MotionEnded OnMotionEnded;

    [SerializeField] private TranslateMovement.TargetInfo targetInfo;
    [SerializeField] private float reverseMotionSpeed;
    private TranslateMovement.TargetInfo reverseTargetInfo;

    private TranslateMovement movementComponent;

    private void Awake()
    {
        reverseTargetInfo.targetPosition = gameObject.transform.position;
        reverseTargetInfo.movementSpeed = reverseMotionSpeed;
    }

    private void Start()
    {
        movementComponent = gameObject.GetComponent<TranslateMovement>();
        SetupMovement(false);
    }

    public void SetupMovement(bool bReverse)
    {
        movementComponent.SetupMovement(gameObject.transform, bReverse ? reverseTargetInfo : targetInfo);
    }
    public void ManageMovement(bool bMove)
    {
        if (bMove)
        {
            movementComponent.OnTargetAchieved += TargetAchieved;
        }
        movementComponent.ManageMovement(bMove);
    }
    private void TargetAchieved()
    {
        movementComponent.OnTargetAchieved -= TargetAchieved;
        OnMotionEnded?.Invoke();
    }
}
