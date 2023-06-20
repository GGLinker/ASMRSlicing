using UnityEngine;
using EzySlice;

public class SliceExecutor : MonoBehaviour
{
    [SerializeField] private Transform SlicePlane;
    [SerializeField] private GameObject SlicedObject;

    public GameObject Slice()
    {
        SlicedHull hull = SlicedObject.Slice(SlicePlane.position, SlicePlane.up);
        if (hull != null)
        {
            GameObject slicedPart = hull.CreateUpperHull(SlicedObject);
            GameObject remainPart = hull.CreateLowerHull(SlicedObject);

            Slice_Recursive(SlicedObject, slicedPart.transform, remainPart.transform);

            CopyComponents(remainPart);

            Destroy(SlicedObject);
            SlicedObject = remainPart;
            return slicedPart;
        }
        return SlicedObject;
    }
    private void CopyComponents(GameObject remainPart)
    {
        var translateMovement = remainPart.AddComponent<TranslateMovement>();
        translateMovement.SetupMovementComponent(SlicedObject.GetComponent<TranslateMovement>());
        var slicingObjectMovement = remainPart.AddComponent<SlicingObjectMovement>();
        slicingObjectMovement.SetupComponent(SlicedObject.GetComponent<SlicingObjectMovement>());

        var rigidBody = remainPart.AddComponent<Rigidbody>();
        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;

        var boxCollider = remainPart.AddComponent<BoxCollider>();
        var oldCollider = SlicedObject.GetComponent<BoxCollider>();
        boxCollider.isTrigger = oldCollider.isTrigger;
        boxCollider.center = oldCollider.center;
        boxCollider.size = oldCollider.size;
    }

    private void Slice_Recursive(GameObject operatingSubObject, Transform slicedRoot, Transform remainRoot)
    {
        for (int i = 0; i < operatingSubObject.transform.childCount; i++)
        {
            GameObject currentSubObjectToSlice = operatingSubObject.transform.GetChild(i).gameObject;
            
            SlicedHull hull = currentSubObjectToSlice.Slice(SlicePlane.position, SlicePlane.up);
            if (hull != null)
            {
                GameObject subObjectSlicedPart = hull.CreateUpperHull(currentSubObjectToSlice);
                GameObject subObjectRemainPart = hull.CreateLowerHull(currentSubObjectToSlice);

                var localPosition = currentSubObjectToSlice.transform.localPosition;
                var localRotation = currentSubObjectToSlice.transform.localRotation;
                
                subObjectSlicedPart.transform.parent = slicedRoot;
                subObjectSlicedPart.transform.localPosition = localPosition;
                subObjectSlicedPart.transform.localRotation = localRotation;
                
                subObjectRemainPart.transform.parent = remainRoot;
                subObjectRemainPart.transform.localPosition = localPosition;
                subObjectRemainPart.transform.localRotation = localRotation;

                Slice_Recursive(currentSubObjectToSlice, subObjectSlicedPart.transform,
                    subObjectRemainPart.transform);
            }
        }
    }
}