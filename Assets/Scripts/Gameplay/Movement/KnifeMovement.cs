using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(TranslateMovement))]
[RequireComponent(typeof(SliceExecutor))]
public class KnifeMovement : MonoBehaviour
{
    public event EventHandler<KnifeMovementState> OnTargetAchieved;
    
    public static string SLICING_OBJECTS_TAG = "SlicingObject";
    
    [SerializeField] private Transform cutTarget;
    [SerializeField] private float cutMotionSpeed;
    [SerializeField] private float reverseMotionSpeed;
    
    [SerializeField] private float initialBendingStrength;
    [SerializeField] private float initialSlicedFaceBendingStrength;
    [SerializeField] private float cutWidthBendingSensitivity;
    
    [SerializeField] private Material bendMeshMaterial;
    [SerializeField] private Material slicedFaceMaterial;

    private static readonly int BEND_STRENGTH_PROPERTY = Shader.PropertyToID("_BendStrength");
    
    private TranslateMovement.TargetInfo _cutTargetInfo;
    private TranslateMovement.TargetInfo _releaseTargetInfo;

    private TranslateMovement _movementComponent;
    private SliceExecutor _sliceExecutorComponent;
    
    private float _totalDistance, _minReachedDistance;
    private float _lastCutPositionZ;
    private bool _bCutAtLeastOnce;

    public enum KnifeMovementState
    {
        Idle,
        Descent,
        Cut,
        Release,
    }
    private KnifeMovementState _state;
    private EventHandler _knifeMovementCallback;
    
    private void Awake()
    {
        _cutTargetInfo.targetPosition = cutTarget.position;
        _cutTargetInfo.movementSpeed = cutMotionSpeed;
        
        _releaseTargetInfo.targetPosition = transform.position;
        _releaseTargetInfo.movementSpeed = reverseMotionSpeed;

        _movementComponent = GetComponent<TranslateMovement>();
        _sliceExecutorComponent = GetComponent<SliceExecutor>();
        
        Reset();
    }

    private void Start()
    {
        StartCoroutine(UpdateSliceProgressInBendingMaterial());
        SetupMovement(false);
    }

    public void Cut()
    {
        if (_state != KnifeMovementState.Descent)
        {
            _state = KnifeMovementState.Cut;
        }

        Debug.Log("Cut movement started");
        
        SetupMovement(false);
        Move(true);
    }
    public void ReleaseKnife()
    {
        Move(false);
        Debug.Log("Reverse movement started");
        
        SetupMovement(true);
        Move(true);
    }

    public void Reset()
    {
        _bCutAtLeastOnce = false;
        _state = KnifeMovementState.Idle;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (_state != KnifeMovementState.Cut || !other.gameObject.CompareTag(SLICING_OBJECTS_TAG)) return;
        _state = KnifeMovementState.Descent;
        
        UpdateBendingStrength(other.transform);
        
        _sliceExecutorComponent.Slice();
        _minReachedDistance = _totalDistance = (transform.position - _cutTargetInfo.targetPosition).magnitude;
    }

    #region MaterialParametersUpdate

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

    void UpdateBendingStrength(Transform slicingObjectTransform)
    {
        var newCutPosition = slicingObjectTransform.position.z;
        if (!_bCutAtLeastOnce)
        {
            _lastCutPositionZ = newCutPosition + slicingObjectTransform.gameObject.GetComponent<MeshCollider>().bounds.size.z / 2;
            _bCutAtLeastOnce = true;
        }
        bendMeshMaterial.SetFloat(BEND_STRENGTH_PROPERTY, initialBendingStrength - Mathf.Abs(newCutPosition - _lastCutPositionZ) * cutWidthBendingSensitivity);
        slicedFaceMaterial.SetFloat(BEND_STRENGTH_PROPERTY, initialSlicedFaceBendingStrength - Mathf.Abs(newCutPosition - _lastCutPositionZ) * cutWidthBendingSensitivity);
        
        _lastCutPositionZ = newCutPosition;
    }

    #endregion
    
    #region Movement

    private void SetupMovement(bool bRelease)
    {
        switch (_state)
        {
            case KnifeMovementState.Cut:
            case KnifeMovementState.Descent: 
                _knifeMovementCallback = bRelease ? ReachedTopPoint : ReachedBottomPoint;
                break;
            case KnifeMovementState.Release:
                _knifeMovementCallback = ReachedTopPoint;
                break;
        }
        
        if (bRelease)
        {
            _movementComponent.SetupMovement(gameObject.transform, _releaseTargetInfo);
        }
        else
        {
            _movementComponent.SetupMovement(gameObject.transform, _cutTargetInfo);
        }
    }
    
    private void Move(bool bMove)
    {
        if (bMove)
        {
            _movementComponent.OnTargetAchieved += _knifeMovementCallback;
        }
        else
        {
            _movementComponent.OnTargetAchieved -= _knifeMovementCallback;
        }

        _movementComponent.Move(bMove);
    }

    private void ReachedBottomPoint(object sender, EventArgs args)
    {
        _movementComponent.OnTargetAchieved -= _knifeMovementCallback;
        
        _state = KnifeMovementState.Release;
        
        OnTargetAchieved?.Invoke(this, _state);
    }
    private void ReachedTopPoint(object sender, EventArgs args)
    {
        _movementComponent.OnTargetAchieved -= _knifeMovementCallback;
        
        if (_state != KnifeMovementState.Descent)
        {
            _state = KnifeMovementState.Idle;
        }

        OnTargetAchieved?.Invoke(this, _state);
    }

    #endregion
}
