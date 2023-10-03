using System;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SliceExecutor : MonoBehaviour
{
    [SerializeField] private Transform SlicePlane;
    [SerializeField] private GameObject SlicedObject;
    [SerializeField] private Material RolledSliceMaterial;
    [SerializeField] private Material SlicedFaceMaterial;
    [SerializeField] private Material RemainPartSliceMaterial;

    public delegate void SliceComplete(GameObject slicedPart, GameObject remainPart);
    public event SliceComplete OnSliceComplete;

    [HideInInspector] public bool bFullySliced;

    private GameObject _hiddenSlicedObjectCopy;
    private GameObject _lastSlicedPart;
    public GameObject GetLastSlicedPart()
    {
        return _lastSlicedPart;
    }

    private void Start()
    {
        CopyAndHideSlicedObjectInstance();
    }

    private void CopyAndHideSlicedObjectInstance()
    {
        _hiddenSlicedObjectCopy = Instantiate(SlicedObject);
        _hiddenSlicedObjectCopy.transform.position = SlicedObject.transform.position;
        _hiddenSlicedObjectCopy.transform.rotation = SlicedObject.transform.rotation;
        
        _hiddenSlicedObjectCopy.SetActive(false);
    }
    public void RespawnSlicedObject()
    {
        bFullySliced = false;
        Destroy(SlicedObject);
        _hiddenSlicedObjectCopy.SetActive(true);
        SlicedObject = _hiddenSlicedObjectCopy;
        OnSliceComplete?.Invoke(_lastSlicedPart, SlicedObject);
        
        CopyAndHideSlicedObjectInstance();
    }
    
    public GameObject Slice()
    {
        SlicedHull hull = SlicedObject.Slice(SlicePlane.position, SlicePlane.up);
        GameObject slicedPart = SlicedObject, remainPart = SlicedObject;
        if (hull != null)
        {
            slicedPart = hull.CreateUpperHull(SlicedObject);
            ApplyMaterials(
                slicedPart.GetComponent<MeshRenderer>(), 
                RolledSliceMaterial,
                SlicedFaceMaterial);
            remainPart = hull.CreateLowerHull(SlicedObject);
            ApplyMaterials(
                remainPart.GetComponent<MeshRenderer>(), 
                SlicedObject.GetComponent<MeshRenderer>().material,
                RemainPartSliceMaterial);

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
        
        _lastSlicedPart = slicedPart;
        OnSliceComplete?.Invoke(slicedPart, remainPart);

        return slicedPart;
    }
    
    public void UpdateBendMaterialValue(float floatValue)
    {
        RolledSliceMaterial.SetFloat("_BendProgress", floatValue);
        SlicedFaceMaterial.SetFloat("_BendProgress", floatValue);
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
            
            Material initialMaterial = currentSubObjectToSlice.GetComponent<MeshRenderer>().materials[0];

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
                ApplyMaterials(subObjectSlicedPart.GetComponent<MeshRenderer>(), RolledSliceMaterial, SlicedFaceMaterial);
                
                subObjectRemainPart.transform.parent = remainRoot;
                subObjectRemainPart.transform.localPosition = localPosition;
                subObjectRemainPart.transform.localRotation = localRotation;
                ApplyMaterials(subObjectRemainPart.GetComponent<MeshRenderer>(), initialMaterial, RemainPartSliceMaterial);

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

    private void ApplyMaterials(MeshRenderer renderer, Material mainMaterial, Material slicedFaceMaterial)
    {
        var newMaterialsArray = renderer.materials;
        newMaterialsArray[0] = mainMaterial;
        for (int i = 1; i < renderer.materials.Length; i++)
        {
            Debug.Log(renderer.gameObject.name + ": " + mainMaterial.name + ";;; " + slicedFaceMaterial.name);
            newMaterialsArray[i] = slicedFaceMaterial;
        }

        renderer.materials = newMaterialsArray;
    }
}