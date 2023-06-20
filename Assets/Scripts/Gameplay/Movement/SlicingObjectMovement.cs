using UnityEngine;

[RequireComponent(typeof(TranslateMovement))]
public class SlicingObjectMovement : MonoBehaviour
{
    [SerializeField] private TranslateMovement.TargetInfo targetInfo;
    private TranslateMovement movementComponent;
    
    private void Start()
    {
        movementComponent = gameObject.GetComponent<TranslateMovement>();
        movementComponent.SetupMovement(gameObject.transform, targetInfo);
    }
    
    public void ManageMovement(bool bMove)
    {
        movementComponent.ManageMovement(bMove);
    }
}
