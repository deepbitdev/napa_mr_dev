using UnityEngine;

public class BrakeModel : MonoBehaviour
{
    // Reference to the source and target GameObjects
    public GameObject sourceObject; // The GameObject to copy from
    public GameObject targetObject; // The GameObject to copy to

    void Update()
    {
        if (sourceObject != null && targetObject != null)
        {
            // Copy position
            targetObject.transform.position = sourceObject.transform.position;

            // Copy rotation
            targetObject.transform.rotation = sourceObject.transform.rotation;
        }
    }
}
