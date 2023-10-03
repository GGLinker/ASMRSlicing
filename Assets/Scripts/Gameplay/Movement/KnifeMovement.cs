using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(TranslateMovement))]
[RequireComponent(typeof(SliceExecutor))]
public class KnifeMovement : MonoBehaviour
{
    public event EventHandler OnTargetAchieved;
    [HideInInspector] public bool bAllowedToSplitObject = true;
    public static string SLICING_OBJECTS_TAG = "SlicingObject";
    
    [SerializeField] private Transform cutTarget;
    [SerializeField] private float cutMotionSpeed;
    [SerializeField] private float reverseMotionSpeed;
    
    private TranslateMovement.TargetInfo _cutTargetInfo;
    private TranslateMovement.TargetInfo _reverseTargetInfo;

    private TranslateMovement _movementComponent;
    private SliceExecutor _sliceExecutorComponent;

    private bool _bMotionStopped;
    private float _totalDistance, _minReachedDistance;

    private void Awake()
    {
        _cutTargetInfo.targetPosition = cutTarget.position;
        _cutTargetInfo.movementSpeed = cutMotionSpeed;
        
        _reverseTargetInfo.targetPosition = transform.position;
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
        if (!bAllowedToSplitObject || !other.gameObject.CompareTag(SLICING_OBJECTS_TAG)) return;
        
        bAllowedToSplitObject = false;
        _sliceExecutorComponent.Slice();
        _minReachedDistance = _totalDistance = (transform.position - _cutTargetInfo.targetPosition).magnitude;
    }
    IEnumerator UpdateSliceProgressInBendingMaterial()
    {
        while (true)
        {
            var reachedDistance = (transform.position - _cutTargetInfo.targetPosition).magnitude;
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
        _movementComponent.SetupMovement(gameObject.transform, bReverse ? _reverseTargetInfo : _cutTargetInfo);
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
        _movementComponent.Move(bMove);
    }
    private void TargetAchieved(object sender, EventArgs args)
    {
        _movementComponent.OnTargetAchieved -= TargetAchieved;
        Debug.Log("Movement end achieved");
        if(!_bMotionStopped) this.OnTargetAchieved?.Invoke(this, EventArgs.Empty);
    }
}
