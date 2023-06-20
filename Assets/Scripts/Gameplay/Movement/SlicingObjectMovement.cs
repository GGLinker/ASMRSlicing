using UnityEngine;

[RequireComponent(typeof(TranslateMovement))]
public class SlicingObjectMovement : MonoBehaviour
{
    [SerializeField] private TranslateMovement.TargetInfo targetInfo;
    private TranslateMovement movementComponent;
 
    public delegate void MotionEnded();
    public event MotionEnded OnMotionEnded;
    
    private void Start()
    {
        Setup();
    }
    private void Setup()
    {
        movementComponent = gameObject.GetComponent<TranslateMovement>();
        movementComponent.SetupMovement(gameObject.transform, targetInfo);
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
