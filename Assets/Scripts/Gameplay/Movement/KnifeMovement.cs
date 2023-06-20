using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Analytics;

[RequireComponent(typeof(TranslateMovement))]
[RequireComponent(typeof(SliceExecutor))]
public class KnifeMovement : MonoBehaviour
{
    public delegate void MotionEnded();
    public event MotionEnded OnMotionEnded;
    [HideInInspector] public bool bAllowedToSplitObject = true;

    [SerializeField] private String slicingObjectsTag;
    [SerializeField] private TranslateMovement.TargetInfo targetInfo;
    [SerializeField] private float reverseMotionTime;
    private TranslateMovement.TargetInfo reverseTargetInfo;

    private TranslateMovement movementComponent;
    private SliceExecutor sliceExecutorComponent;

    private void Awake()
    {
        reverseTargetInfo.targetPosition = gameObject.transform.position;
        reverseTargetInfo.movementTime = reverseMotionTime;
        
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
            bAllowedToSplitObject = false;
            sliceExecutorComponent.Slice();
            StartCoroutine(UpdateSliceProgressInBendingMaterial());
        }
    }
    IEnumerator UpdateSliceProgressInBendingMaterial()
    {
        var absoluteDistance = Vector3.Distance(transform.position, targetInfo.targetPosition);
        float minReachedDistance = absoluteDistance;
        while (!bAllowedToSplitObject)
        {
            var reachedDistance = Vector3.Distance(transform.position, targetInfo.targetPosition);
            if (reachedDistance >= minReachedDistance)
            {
                yield return null;
                continue;
            }

            minReachedDistance = reachedDistance;
            sliceExecutorComponent.UpdateRollProgressMaterialValueY(Mathf.Lerp(0, 1, minReachedDistance / absoluteDistance));
            yield return null;
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
        else
        {
            Debug.Log("Movement stopped");
            movementComponent.OnTargetAchieved -= TargetAchieved;
        }
        movementComponent.ManageMovement(bMove);
    }
    private void TargetAchieved()
    {
        movementComponent.OnTargetAchieved -= TargetAchieved;
        Debug.Log("Movement end triggered");
        OnMotionEnded?.Invoke();
    }
}
