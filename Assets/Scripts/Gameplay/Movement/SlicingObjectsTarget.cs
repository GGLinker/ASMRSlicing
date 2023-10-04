using UnityEngine;

public class SlicingObjectsTarget : MonoBehaviour
{
    public static SlicingObjectsTarget Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Singleton violation: " + gameObject.name);
        }
        Instance = this;
    }
}
