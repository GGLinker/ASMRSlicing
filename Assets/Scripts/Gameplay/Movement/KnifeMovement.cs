using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Analytics;

[RequireComponent(typeof(TranslateMovement))]
[RequireComponent(typeof(SliceExecutor))]
public class KnifeMovement : MonoBehaviour
{
    public delegate void TargetAchievedDelegate();
    public event TargetAchievedDelegate OnTargetAchieved;
    [HideInInspector] public bool bAllowedToSplitObject = true;

    [SerializeField] private String slicingObjectsTag;
    [SerializeField] private TranslateMovement.TargetInfo targetInfo;
    [SerializeField] private float reverseMotionTime;
    private TranslateMovement.TargetInfo reverseTargetInfo;

    private TranslateMovement movementComponent;
    private SliceExecutor sliceExecutorComponent;

    private bool bMotionStopped;
    private float absoluteDistance, minReachedDistance;

    private void Awake()
    {
        reverseTargetInfo.targetPosition = gameObject.transform.position;
        reverseTargetInfo.movementTime = reverseMotionTime;
        
        movementComponent = GetComponent<TranslateMovement>();
        sliceExecutorComponent = GetComponent<SliceExecutor>();
    }

    private void Start()
    {
        StartCoroutine(UpdateSliceProgressInBendingMaterial());
        SetupMovement(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!bAllowedToSplitObject || !other.gameObject.CompareTag(slicingObjectsTag)) return;
        
        bAllowedToSplitObject = false;
        sliceExecutorComponent.Slice();
        minReachedDistance = absoluteDistance;
    }
    IEnumerator UpdateSliceProgressInBendingMaterial()
    {
        absoluteDistance = Vector3.Distance(transform.position, targetInfo.targetPosition);
        minReachedDistance = absoluteDistance;
        while (true)
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
        bMotionStopped = !bMove;
        if (bMove)
        {
            movementComponent.OnTargetAchieved += TargetAchieved;
        }
        else
        {
            Debug.Log("Movement stopped before target has been achieved");
            movementComponent.OnTargetAchieved -= TargetAchieved;
        }
        movementComponent.ManageMovement(bMove);
    }
    private void TargetAchieved()
    {
        movementComponent.OnTargetAchieved -= TargetAchieved;
        Debug.Log("Movement end achieved");
        if(!bMotionStopped) this.OnTargetAchieved?.Invoke();
    }
}
