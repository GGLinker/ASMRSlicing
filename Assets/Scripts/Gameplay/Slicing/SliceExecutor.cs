using UnityEngine;
using EzySlice;

public class SliceExecutor : MonoBehaviour
{
    [SerializeField] private Transform slicePlane;
    [SerializeField] private GameObject slicedObject;
    [SerializeField] private Material rolledSliceMaterial;
    [SerializeField] private Material slicedFaceMaterial;
    [SerializeField] private Material remainPartSliceMaterial;

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
        OnSliceComplete?.Invoke(_lastSlicedPart, slicedObject);
        
        CopyAndHideSlicedObjectInstance();
    }
    
    public GameObject Slice()
    {
        SlicedHull hull = slicedObject.Slice(slicePlane.position, slicePlane.up);
        GameObject slicedPart = slicedObject, remainPart = slicedObject;
        if (hull != null)
        {
            slicedPart = hull.CreateUpperHull(slicedObject);
            ApplyMaterials(
                slicedPart.GetComponent<MeshRenderer>(), 
                rolledSliceMaterial,
                slicedFaceMaterial);
            remainPart = hull.CreateLowerHull(slicedObject);
            ApplyMaterials(
                remainPart.GetComponent<MeshRenderer>(), 
                slicedObject.GetComponent<MeshRenderer>().material,
                remainPartSliceMaterial);

            Slice_Recursive(slicedObject, slicedPart.transform, remainPart.transform);
            CopyComponents(remainPart, slicedPart);

            Destroy(slicedObject);
            slicedObject = remainPart;
        }
        else if (slicedObject.transform.position.z < slicePlane.transform.position.z)
        {
            slicedPart = slicedObject;
            bFullySliced = true;
            Debug.Log("Fully sliced");
        }
        else return slicedObject;
        
        _lastSlicedPart = slicedPart;
        OnSliceComplete?.Invoke(slicedPart, remainPart);

        return slicedPart;
    }
    
    public void UpdateBendMaterialValue(float floatValue)
    {
        rolledSliceMaterial.SetFloat("_BendProgress", floatValue);
        slicedFaceMaterial.SetFloat("_BendProgress", floatValue);
    }
    private void CopyComponents(GameObject remainPart, GameObject slicedPart)
    {
        //Remain
        remainPart.tag = "SlicingObject";
        
        var translateMovement = remainPart.AddComponent<TranslateMovement>();
        translateMovement.SetupMovementComponent(slicedObject.GetComponent<TranslateMovement>());
        var slicingObjectMovement = remainPart.AddComponent<SlicingObjectMovement>();
        slicingObjectMovement.SetupComponent(slicedObject.GetComponent<SlicingObjectMovement>());

        var rigidBody = remainPart.AddComponent<Rigidbody>();
        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;

        var boxCollider = remainPart.AddComponent<BoxCollider>();
        var oldCollider = slicedObject.GetComponent<BoxCollider>();
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
        var oldCollider = slicedObject.GetComponent<BoxCollider>();
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
            SlicedHull hull = currentSubObjectToSlice.Slice(slicePlane.position, slicePlane.up);
            if (hull != null)
            {
                GameObject subObjectSlicedPart = hull.CreateUpperHull(currentSubObjectToSlice);
                GameObject subObjectRemainPart = hull.CreateLowerHull(currentSubObjectToSlice);
                
                subObjectSlicedPart.transform.parent = slicedRoot;
                subObjectSlicedPart.transform.localPosition = localPosition;
                subObjectSlicedPart.transform.localRotation = localRotation;
                ApplyMaterials(subObjectSlicedPart.GetComponent<MeshRenderer>(), rolledSliceMaterial, slicedFaceMaterial);
                
                subObjectRemainPart.transform.parent = remainRoot;
                subObjectRemainPart.transform.localPosition = localPosition;
                subObjectRemainPart.transform.localRotation = localRotation;
                ApplyMaterials(subObjectRemainPart.GetComponent<MeshRenderer>(), initialMaterial, remainPartSliceMaterial);

                Slice_Recursive(currentSubObjectToSlice, subObjectSlicedPart.transform,
                    subObjectRemainPart.transform);
            }
            else
            {
                currentSubObjectToSlice.transform.parent = currentSubObjectToSlice.transform.position.z >= slicePlane.transform.position.z ? remainRoot : slicedRoot;
                currentSubObjectToSlice.transform.localPosition = localPosition;
                currentSubObjectToSlice.transform.localRotation = localRotation;
            }
        }
    }

    private void ApplyMaterials(MeshRenderer meshRenderer, Material mainMaterial, Material slicedMaterial)
    {
        var newMaterialsArray = meshRenderer.materials;
        newMaterialsArray[0] = mainMaterial;
        for (int i = 1; i < meshRenderer.materials.Length; i++)
        {
            Debug.Log(meshRenderer.gameObject.name + ": " + mainMaterial.name + ";;; " + slicedMaterial.name);
            newMaterialsArray[i] = slicedMaterial;
        }

        meshRenderer.materials = newMaterialsArray;
    }
}