using System;
using UnityEngine;

[RequireComponent(typeof(TranslateMovement))]
public class SlicingObjectMovement : MonoBehaviour
{
    [SerializeField] private TranslateMovement.TargetInfo targetInfo;
    private TranslateMovement _movementComponent;

    public event EventHandler OnMotionEnded;
    
    private void Start()
    {
        Setup();
    }
    private void Setup()
    {
        _movementComponent = gameObject.GetComponent<TranslateMovement>();
        _movementComponent.SetupMovement(transform, targetInfo);
    }

    public void SetupComponent(SlicingObjectMovement other)
    {
        targetInfo = other.targetInfo;
        Setup();
    }
    public void ManageMovement(bool bMove)
    {
        if (bMove)
        {
            _movementComponent.OnTargetAchieved += TargetAchieved;
        }
        _movementComponent.ManageMovement(bMove);
    }
    private void TargetAchieved(object sender, EventArgs args)
    {
        _movementComponent.OnTargetAchieved -= TargetAchieved;
        OnMotionEnded?.Invoke(this, EventArgs.Empty);
    }
}
