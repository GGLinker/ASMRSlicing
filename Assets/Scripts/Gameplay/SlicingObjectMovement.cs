using UnityEngine;

[RequireComponent(typeof(TranslateMovement))]
public class SlicingObjectMovement : MonoBehaviour
{
    private TranslateMovement movementComponent;
    
    private void Start()
    {
        movementComponent = gameObject.GetComponent<TranslateMovement>();
        movementComponent.SetupMovement(gameObject.transform);
    }
    
    public void ManageMovement(bool bMove)
    {
        movementComponent.ManageMovement(bMove);
    }
}
