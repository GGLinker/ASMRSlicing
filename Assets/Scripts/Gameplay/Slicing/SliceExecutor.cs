using UnityEngine;
using EzySlice;

public class SliceExecutor : MonoBehaviour
{
    [SerializeField] private Transform SlicePlane;
    [SerializeField] private GameObject SlicedObject;
    [SerializeField] private Material RolledSliceMaaterial;

    public delegate void SliceComplete(GameObject slicedPart, GameObject remainPart);
    public event SliceComplete OnSliceComplete;

    private GameObject LastSlicedPart;
    public GameObject GetLastSlicedPart()
    {
        return LastSlicedPart;
    }

    public GameObject Slice()
    {
        SlicedHull hull = SlicedObject.Slice(SlicePlane.position, SlicePlane.up);
        if (hull != null)
        {
            GameObject slicedPart = hull.CreateUpperHull(SlicedObject);
            slicedPart.GetComponent<MeshRenderer>().material = RolledSliceMaaterial;
            GameObject remainPart = hull.CreateLowerHull(SlicedObject);

            Slice_Recursive(SlicedObject, slicedPart.transform, remainPart.transform);
            LastSlicedPart = slicedPart;

            CopyComponents(remainPart, slicedPart);

            Destroy(SlicedObject);
            SlicedObject = remainPart;
            
            OnSliceComplete?.Invoke(slicedPart, remainPart);

            return slicedPart;
        }
        return SlicedObject;
    }
    private void CopyComponents(GameObject remainPart, GameObject slicedPart)
    {
        //Remain
        remainPart.tag = "SlicingObject";
        
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
        
        //Sliced
        AddPhysicalComponentsOnSlicedSubObjects(slicedPart);
    }
    private void AddPhysicalComponentsOnSlicedSubObjects(GameObject parent)
    {
        var rigidBody = parent.AddComponent<Rigidbody>();
        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;

        var boxCollider = parent.AddComponent<BoxCollider>();
        var oldCollider = SlicedObject.GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        boxCollider.center = oldCollider.center;
        boxCollider.size = oldCollider.size;

        /*for (int i = 0; i < parent.transform.childCount; i++)
        {
            AddPhysicalComponentsOnSlicedSubObjects(parent.transform.GetChild(i).gameObject);
        }*/
    }

    
    private void Slice_Recursive(GameObject operatingSubObject, Transform slicedRoot, Transform remainRoot)
    {
        for (int i = 0; i < operatingSubObject.transform.childCount; i++)
        {
            GameObject currentSubObjectToSlice = operatingSubObject.transform.GetChild(i).gameObject;
            
            var localPosition = currentSubObjectToSlice.transform.localPosition;
            var localRotation = currentSubObjectToSlice.transform.localRotation;
            SlicedHull hull = currentSubObjectToSlice.Slice(SlicePlane.position, SlicePlane.up);
            if (hull != null)
            {
                GameObject subObjectSlicedPart = hull.CreateUpperHull(currentSubObjectToSlice);
                GameObject subObjectRemainPart = hull.CreateLowerHull(currentSubObjectToSlice);

                subObjectSlicedPart.transform.parent = slicedRoot;
                subObjectSlicedPart.transform.localPosition = localPosition;
                subObjectSlicedPart.transform.localRotation = localRotation;
                subObjectSlicedPart.GetComponent<MeshRenderer>().material = RolledSliceMaaterial;
                
                subObjectRemainPart.transform.parent = remainRoot;
                subObjectRemainPart.transform.localPosition = localPosition;
                subObjectRemainPart.transform.localRotation = localRotation;

                Slice_Recursive(currentSubObjectToSlice, subObjectSlicedPart.transform,
                    subObjectRemainPart.transform);
            }
            else
            {
                currentSubObjectToSlice.transform.parent = remainRoot;
                currentSubObjectToSlice.transform.localPosition = localPosition;
                currentSubObjectToSlice.transform.localRotation = localRotation;
            }
        }
    }

    public void UpdateRollProgressMaterialValueY(float floatValue)
    {
        RolledSliceMaaterial.SetFloat("_PointY", floatValue);
    }
}