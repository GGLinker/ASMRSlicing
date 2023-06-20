using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(TranslateMovement))]
[RequireComponent(typeof(SliceExecutor))]
public class KnifeMovement : MonoBehaviour
{
    public delegate void MotionEnded();
    public event MotionEnded OnMotionEnded;
    [HideInInspector] public bool bAllowedToSplitObject = true;

    [SerializeField] private String slicingObjectsTag;
    [SerializeField] private TranslateMovement.TargetInfo targetInfo;
    [SerializeField] private float reverseMotionSpeed;
    private TranslateMovement.TargetInfo reverseTargetInfo;

    private TranslateMovement movementComponent;
    private SliceExecutor sliceExecutorComponent;

    private void Awake()
    {
        reverseTargetInfo.targetPosition = gameObject.transform.position;
        reverseTargetInfo.movementSpeed = reverseMotionSpeed;
        
        movementComponent = GetComponent<TranslateMovement>();
        sliceExecutorComponent = GetComponent<SliceExecutor>();
    }

    private void Start()
    {
        SetupMovement(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(slicingObjectsTag))
        {
            Debug.Log("Collide with: " + other.gameObject.name);
            bAllowedToSplitObject = false;
            Debug.Log(sliceExecutorComponent.Slice().name);
        }
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
