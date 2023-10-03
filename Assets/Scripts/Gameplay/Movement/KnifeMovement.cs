using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(TranslateMovement))]
[RequireComponent(typeof(SliceExecutor))]
public class KnifeMovement : MonoBehaviour
{
    public delegate void TargetAchievedDelegate();
    public event TargetAchievedDelegate OnTargetAchieved;
    [HideInInspector] public bool bAllowedToSplitObject = true;

    [SerializeField] private String slicingObjectsTag;
    [SerializeField] private TranslateMovement.TargetInfo targetInfo;
    [SerializeField] private float reverseMotionSpeed;
    private TranslateMovement.TargetInfo _reverseTargetInfo;

    private TranslateMovement _movementComponent;
    private SliceExecutor _sliceExecutorComponent;

    private bool _bMotionStopped;
    private float _totalDistance, _minReachedDistance;

    private void Awake()
    {
        _reverseTargetInfo.targetPosition = gameObject.transform.position;
        _reverseTargetInfo.movementSpeed = reverseMotionSpeed;
        
        _movementComponent = GetComponent<TranslateMovement>();
        _sliceExecutorComponent = GetComponent<SliceExecutor>();
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
        _sliceExecutorComponent.Slice();
        _minReachedDistance = _totalDistance = (transform.position - targetInfo.targetPosition).magnitude;
    }
    IEnumerator UpdateSliceProgressInBendingMaterial()
    {
        while (true)
        {
            var reachedDistance = (transform.position - targetInfo.targetPosition).magnitude;
            if (reachedDistance >= _minReachedDistance)
            {
                yield return null;
                continue;
            }

            _minReachedDistance = reachedDistance;
            _sliceExecutorComponent.UpdateBendMaterialValue(Mathf.Lerp(0, 1, 1 - _minReachedDistance / _totalDistance));
            yield return null;
        }
    }

    public void SetupMovement(bool bReverse)
    {
        _movementComponent.SetupMovement(gameObject.transform, bReverse ? _reverseTargetInfo : targetInfo);
    }
    public void ManageMovement(bool bMove)
    {
        _bMotionStopped = !bMove;
        if (bMove)
        {
            _movementComponent.OnTargetAchieved += TargetAchieved;
        }
        else
        {
            Debug.Log("Movement stopped before target has been achieved");
            _movementComponent.OnTargetAchieved -= TargetAchieved;
        }
        _movementComponent.ManageMovement(bMove);
    }
    private void TargetAchieved(object sender, EventArgs args)
    {
        _movementComponent.OnTargetAchieved -= TargetAchieved;
        Debug.Log("Movement end achieved");
        if(!_bMotionStopped) this.OnTargetAchieved?.Invoke();
    }
}
