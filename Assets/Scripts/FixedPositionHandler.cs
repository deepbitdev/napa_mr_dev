using UnityEngine;
using Anaglyph.DisplayCapture.Barcodes;

public class FixedPositionHandler : MonoBehaviour
{
    public static FixedPositionHandler Instance { get; private set; }

    [SerializeField] private GameObject fixedGameObject; // GameObject to position
    [SerializeField] private GameObject emptyReferenceObject;

    private bool isPositionSet = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple FixedPositionHandler instances detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (fixedGameObject == null) Debug.LogWarning("fixedGameObject is not assigned in the Inspector!");
        if (emptyReferenceObject == null) Debug.LogWarning("emptyReferenceObject is not assigned in the Inspector!");
        if (Indicator.Instance == null) Debug.LogWarning("Indicator.Instance is null!");

        if (emptyReferenceObject != null && !emptyReferenceObject.activeSelf)
        {
            emptyReferenceObject.SetActive(true);
            Debug.Log("Activated emptyReferenceObject.");
        }

        if (fixedGameObject != null && fixedGameObject.activeSelf)
        {
            fixedGameObject.SetActive(false);
            Debug.Log("Disabled fixedGameObject at start.");
        }
    }

    public void SetFixedPosition(Vector3 worldPosition, Quaternion worldRotation)
    {
        if (fixedGameObject != null)
        {
            fixedGameObject.SetActive(true);
            fixedGameObject.transform.position = worldPosition;
            fixedGameObject.transform.rotation = worldRotation;
            Debug.Log($"Set fixedGameObject to world position: {worldPosition}, rotation: {worldRotation.eulerAngles}");

            if (emptyReferenceObject != null)
            {
                emptyReferenceObject.transform.position = worldPosition;
                emptyReferenceObject.transform.rotation = worldRotation;
            }

            isPositionSet = true;
        }
        else
        {
            Debug.LogWarning("fixedGameObject is null; cannot set position!");
        }
    }

    void Update()
    {
        if (!isPositionSet)
        {
            UpdatePosition();
        }
    }

    public void UpdatePosition()
    {
        if (Indicator.Instance == null || fixedGameObject == null || !fixedGameObject.activeSelf) return;

        if (emptyReferenceObject != null && emptyReferenceObject.activeSelf)
        {
            emptyReferenceObject.transform.position = Indicator.Instance.EventOnePosition;
            emptyReferenceObject.transform.rotation = Indicator.Instance.EventOneRotation;
            fixedGameObject.transform.position = emptyReferenceObject.transform.position;
            fixedGameObject.transform.rotation = emptyReferenceObject.transform.rotation;
            Debug.Log($"Updated fixedGameObject to position: {fixedGameObject.transform.position}, rotation: {fixedGameObject.transform.rotation.eulerAngles}");
        }
    }


    public void ResetFixedPosition()
    {
        if (fixedGameObject != null && fixedGameObject.activeSelf)
        {
            fixedGameObject.SetActive(false);
            Debug.Log("Disabled fixedGameObject.");
        }
        isPositionSet = false;
        Debug.Log("Reset fixedGameObject state.");
    }

        public void LockFixedPosition()
    {
        isPositionSet = true;
        Debug.Log("Locked fixedGameObject position.");
    }

    public void UnlockedFixedPosition()
    {
        isPositionSet = false;
        Debug.Log("Unlocked fixedGameObject position.");
    }
}