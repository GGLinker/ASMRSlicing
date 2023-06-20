using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(TranslateMovement))]
public class KnifeMovement : MonoBehaviour
{
    public delegate void ReverseMotionEnded();
    public event ReverseMotionEnded OnReverseMotionEnded;

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
        SetupMovement();
    }

    public void SetupMovement()
    {
        movementComponent.SetupMovement(gameObject.transform, targetInfo);
    }
    public void ManageMovement(bool bMove)
    {
        movementComponent.ManageMovement(bMove);
    }

    public void MoveToInitialPose()
    {
        movementComponent.SetupMovement(gameObject.transform, reverseTargetInfo);
        movementComponent.OnTargetAchieved += ReverseTargetAchieved;
        ManageMovement(true);
    }
    private void ReverseTargetAchieved()
    {
       movementComponent.OnTargetAchieved += ReverseTargetAchieved;
       OnReverseMotionEnded?.Invoke();
    }
}
