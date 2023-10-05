using System;
using UnityEngine;
using EzySlice;

public class SliceExecutor : MonoBehaviour
{
    [SerializeField] private Transform slicePlane;
    [SerializeField] private GameObject slicedObject;
    [SerializeField] private Material bendMeshMaterial;
    [SerializeField] private Material slicedFaceMaterial;
    [SerializeField] private Material remainPartSliceMaterial;

    [SerializeField] private Vector3 throwOffForce;
    [SerializeField] private Vector3 throwOffTorque;

    private static readonly int BEND_PROGRESS_PROPERTY = Shader.PropertyToID("_BendProgress");
    private const string SLICED_SUBOBJECT_NAME_POSTFIX = "_SLICED"; 
    
    public class CompletedSliceEventArgs : EventArgs
    {
        public GameObject slicedPart;
        public GameObject remainPart;
    }
    public event EventHandler<CompletedSliceEventArgs> OnSliceComplete;

    public bool bFullySliced { get; private set; }

    private GameObject _hiddenSlicedObjectCopy;
    private GameObject _lastSlicedPart;

    private void Start()
    {
        CopyAndHideSlicedObjectInstance();
    }

    private void CopyAndHideSlicedObjectInstance()
    {
        _hiddenSlicedObjectCopy = Instantiate(slicedObject);
        _hiddenSlicedObjectCopy.transform.position = slicedObject.transform.position;
        _hiddenSlicedObjectCopy.transform.rotation = slicedObject.transform.rotation;
        
        _hiddenSlicedObjectCopy.SetActive(false);
    }
    public void RespawnSlicedObject()
    {
        bFullySliced = false;
        Destroy(slicedObject);
        _hiddenSlicedObjectCopy.SetActive(true);
        slicedObject = _hiddenSlicedObjectCopy;
        OnSliceComplete?.Invoke(this, new CompletedSliceEventArgs()
        {
            remainPart = slicedObject,
            slicedPart = _lastSlicedPart
        });
        
        CopyAndHideSlicedObjectInstance();
    }
    
    public void Slice()
    {
        SlicedHull hull = slicedObject.Slice(slicePlane.position, slicePlane.up);
        Transform slicedPart = slicedObject.transform, remainPart = slicedObject.transform;
        
        if (hull == null)
        {
            var slicedObjectBackBound = 
                slicedObject.transform.position.z +
                slicedObject.GetComponent<MeshCollider>().bounds.size.z / 2;
            
            if (slicedObjectBackBound < slicePlane.position.z)
            {
                //Knife cut after object -> slice cycle ended
                slicedPart = slicedObject.transform;
                bFullySliced = true;
                Debug.Log("Fully sliced");
            }
            
            //Knife cut before object
            else return;
        }
        else 
        {
            //Cut
            slicedPart = hull.CreateUpperHull(slicedObject).transform;
            ApplyMaterials(
                slicedPart.GetComponent<MeshRenderer>(), 
                bendMeshMaterial,
                slicedFaceMaterial);
            remainPart = hull.CreateLowerHull(slicedObject).transform;
            
            var oldName = slicedObject.gameObject.name;
            remainPart.name = oldName;
            slicedPart.name = oldName + SLICED_SUBOBJECT_NAME_POSTFIX;

            ApplyMaterials(
                remainPart.GetComponent<MeshRenderer>(), 
                slicedObject.GetComponent<MeshRenderer>().material,
                remainPartSliceMaterial);

            Slice_Recursive(slicedObject.transform, slicedPart, remainPart);
            CopyComponentsToRemainPart(remainPart);
            AddComponentsToSlicedPart(slicedPart);

            Destroy(slicedObject);
            slicedObject = remainPart.gameObject;
        }

        //Slice completed
        _lastSlicedPart = slicedPart.gameObject;
        OnSliceComplete?.Invoke(this, new CompletedSliceEventArgs()
        {
            remainPart = remainPart.gameObject,
            slicedPart = slicedPart.gameObject
        });
    }
    
    public void UpdateBendMaterialValue(float floatValue)
    {
        bendMeshMaterial.SetFloat(BEND_PROGRESS_PROPERTY, floatValue);
        slicedFaceMaterial.SetFloat(BEND_PROGRESS_PROPERTY, floatValue);
    }


    public void ThrowOffSlicedPart()
    {
        if (!_lastSlicedPart) return;
        var splitPartRigidbody = _lastSlicedPart.GetComponent<Rigidbody>();
        if (!splitPartRigidbody) return;

        splitPartRigidbody.useGravity = true;
        splitPartRigidbody.isKinematic = false;
        splitPartRigidbody.AddForce(throwOffForce);
        splitPartRigidbody.AddTorque(throwOffTorque);
    }

    private void Slice_Recursive(Transform operatingSubObject, Transform slicedRoot, Transform remainRoot)
    {
        for (int i = 0; i < operatingSubObject.childCount; i++)
        {
            Transform currentSubObjectToSlice = operatingSubObject.GetChild(i);
            
            Material initialMaterial = currentSubObjectToSlice.GetComponent<MeshRenderer>().materials[0];

            var localPosition = currentSubObjectToSlice.localPosition;
            var localRotation = currentSubObjectToSlice.localRotation;
            SlicedHull hull = currentSubObjectToSlice.gameObject.Slice(slicePlane.position, slicePlane.up);

            if (hull != null)
            {
                Transform subObjectSlicedPart = hull.CreateUpperHull(currentSubObjectToSlice.gameObject).transform;
                Transform subObjectRemainPart = hull.CreateLowerHull(currentSubObjectToSlice.gameObject).transform;
                var oldName = currentSubObjectToSlice.gameObject.name;
                subObjectRemainPart.name = oldName;
                subObjectSlicedPart.name = oldName + SLICED_SUBOBJECT_NAME_POSTFIX;
                
                //Sliced part
                subObjectSlicedPart.parent = slicedRoot;
                subObjectSlicedPart.localPosition = localPosition;
                subObjectSlicedPart.localRotation = localRotation;
                ApplyMaterials(subObjectSlicedPart.GetComponent<MeshRenderer>(), bendMeshMaterial, slicedFaceMaterial);
                
                AddComponentsToSlicedPart(subObjectSlicedPart);

                //Remain part
                subObjectRemainPart.parent = remainRoot;
                subObjectRemainPart.localPosition = localPosition;
                subObjectRemainPart.localRotation = localRotation;
                ApplyMaterials(subObjectRemainPart.GetComponent<MeshRenderer>(), initialMaterial, remainPartSliceMaterial);
                
                CopyComponentsToRemainPart(subObjectRemainPart);
                
                //Continue...
                Slice_Recursive(currentSubObjectToSlice, subObjectSlicedPart, subObjectRemainPart);
            }
            else
            { 
                currentSubObjectToSlice.parent = currentSubObjectToSlice.position.z >= slicePlane.position.z ? remainRoot : slicedRoot;
                currentSubObjectToSlice.localPosition = localPosition;
                currentSubObjectToSlice.localRotation = localRotation;
            }
        }
    }
    
    private void CopyComponentsToRemainPart(Transform remainPart)
    {
        remainPart.tag = KnifeMovement.SLICING_OBJECTS_TAG;
        
        var translateMovement = remainPart.gameObject.AddComponent<TranslateMovement>();
        translateMovement.SetupMovementComponent(slicedObject.GetComponent<TranslateMovement>());
        var slicingObjectMovement = remainPart.gameObject.AddComponent<SlicingObjectMovement>();
        slicingObjectMovement.SetupComponent(slicedObject.GetComponent<SlicingObjectMovement>());

        var rigidBody = remainPart.gameObject.AddComponent<Rigidbody>();
        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;

        remainPart.gameObject.AddComponent<MeshCollider>();
    }
    private void AddComponentsToSlicedPart(Transform slicedPart)
    {
        var rigidBody = slicedPart.gameObject.AddComponent<Rigidbody>();
        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;

        var collider = slicedPart.gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
    }

    private void ApplyMaterials(MeshRenderer meshRenderer, Material mainMaterial, Material slicedMaterial)
    {
        var newMaterialsArray = meshRenderer.materials;
        newMaterialsArray[0] = mainMaterial;
        for (int i = 1; i < meshRenderer.materials.Length; i++)
        {
            newMaterialsArray[i] = slicedMaterial;
        }

        meshRenderer.materials = newMaterialsArray;
    }
}