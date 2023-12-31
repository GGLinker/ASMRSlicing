using System;
using UnityEngine;
using EzySlice;
using UnityEngine.Serialization;

public class SliceExecutor : MonoBehaviour
{
    [SerializeField] private Transform SlicePlane;
    [SerializeField] private GameObject SlicedObject;
    [SerializeField] private Material RolledSliceMaterial;

    public delegate void SliceComplete(GameObject slicedPart, GameObject remainPart);
    public event SliceComplete OnSliceComplete;

    [HideInInspector] public bool bFullySliced;

    private GameObject hiddenSlicedObjectCopy;
    private GameObject LastSlicedPart;
    public GameObject GetLastSlicedPart()
    {
        return LastSlicedPart;
    }

    private void Start()
    {
        CopyAndHideSlicedObjectInstance();
    }

    private void CopyAndHideSlicedObjectInstance()
    {
        hiddenSlicedObjectCopy = Instantiate(SlicedObject);
        hiddenSlicedObjectCopy.transform.position = SlicedObject.transform.position;
        hiddenSlicedObjectCopy.transform.rotation = SlicedObject.transform.rotation;
        
        hiddenSlicedObjectCopy.SetActive(false);
    }
    public void RespawnSlicedObject()
    {
        bFullySliced = false;
        Destroy(SlicedObject);
        hiddenSlicedObjectCopy.SetActive(true);
        SlicedObject = hiddenSlicedObjectCopy;
        OnSliceComplete?.Invoke(LastSlicedPart, SlicedObject);
        
        CopyAndHideSlicedObjectInstance();
    }
    
    public GameObject Slice()
    {
        SlicedHull hull = SlicedObject.Slice(SlicePlane.position, SlicePlane.up);
        GameObject slicedPart = SlicedObject, remainPart = SlicedObject;
        if (hull != null)
        {
            slicedPart = hull.CreateUpperHull(SlicedObject);
            slicedPart.GetComponent<MeshRenderer>().material = RolledSliceMaterial;
            remainPart = hull.CreateLowerHull(SlicedObject);

            Slice_Recursive(SlicedObject, slicedPart.transform, remainPart.transform);
            CopyComponents(remainPart, slicedPart);

            Destroy(SlicedObject);
            SlicedObject = remainPart;
        }
        else if (SlicedObject.transform.position.z < SlicePlane.transform.position.z)
        {
            slicedPart = SlicedObject;
            bFullySliced = true;
            Debug.Log("Fully sliced");
        }
        else return SlicedObject;
        
        LastSlicedPart = slicedPart;
        OnSliceComplete?.Invoke(slicedPart, remainPart);

        return slicedPart;
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
                subObjectSlicedPart.GetComponent<MeshRenderer>().material = RolledSliceMaterial;
                
                subObjectRemainPart.transform.parent = remainRoot;
                subObjectRemainPart.transform.localPosition = localPosition;
                subObjectRemainPart.transform.localRotation = localRotation;

                Slice_Recursive(currentSubObjectToSlice, subObjectSlicedPart.transform,
                    subObjectRemainPart.transform);
            }
            else
            {
                currentSubObjectToSlice.transform.parent = currentSubObjectToSlice.transform.position.z >= SlicePlane.transform.position.z ? remainRoot : slicedRoot;
                currentSubObjectToSlice.transform.localPosition = localPosition;
                currentSubObjectToSlice.transform.localRotation = localRotation;
            }
        }
    }

    public void UpdateRollProgressMaterialValueY(float floatValue)
    {
        RolledSliceMaterial.SetFloat("_PointY", floatValue);
    }
}