using System;
using UnityEngine;

[RequireComponent(typeof(TranslateMovement))]
public class SlicingObjectMovement : MonoBehaviour
{
    [SerializeField] private float motionSpeed;
    
    private TranslateMovement _movementComponent;
    private TranslateMovement.TargetInfo _targetInfo;

    public event EventHandler OnMotionEnded;

    private void Start()
    {
        Setup();
    }
    
    private void Setup()
    {
        _targetInfo.movementSpeed = motionSpeed;
        _targetInfo.targetPosition = SlicingObjectsTarget.Instance.transform.position;
        _movementComponent = gameObject.GetComponent<TranslateMovement>();
        _movementComponent.SetupMovement(transform, _targetInfo);
    }

    public void SetupComponent(SlicingObjectMovement other)
    {
        motionSpeed = other.motionSpeed;
        Setup();
    }
    public void Move(bool bMove)
    {
        if (bMove)
        {
            _movementComponent.OnTargetAchieved += TargetAchieved;
        }
        else
        {
            _movementComponent.OnTargetAchieved -= TargetAchieved;
        }
        _movementComponent.Move(bMove);
    }
    private void TargetAchieved(object sender, EventArgs args)
    {
        _movementComponent.OnTargetAchieved -= TargetAchieved;
        OnMotionEnded?.Invoke(this, EventArgs.Empty);
    }
}
