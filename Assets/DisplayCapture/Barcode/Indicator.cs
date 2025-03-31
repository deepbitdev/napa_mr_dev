using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;

namespace Anaglyph.DisplayCapture.Barcodes
{
	public class Indicator : MonoBehaviour
{
    public static Indicator Instance;

    [SerializeField] private LineRenderer lineRenderer;
    public LineRenderer LineRenderer => lineRenderer;

    [SerializeField] private TMP_Text textMesh;
    public TMP_Text TextMesh => textMesh;

    [SerializeField] private Transform reassemblyPositionMarker;

    private Vector3[] offsetPositions = new Vector3[4];

    public GameObject eventOne;
    public GameObject reassemblyOne;
    public GameObject wrenchObjectToSpawn; // The prefab to spawn
    private GameObject spawnedWrenchObject; // Track the instantiated wrench object

    private HashSet<string> singleDetectionBarcodes = new HashSet<string>();
    private bool hasScannedMocap = false;
    private bool hasScannedWrench = false;

    public Vector3 EventOnePosition => reassemblyPositionMarker != null ? reassemblyPositionMarker.position : Vector3.zero;
    public Quaternion EventOneRotation => reassemblyPositionMarker != null ? reassemblyPositionMarker.rotation : Quaternion.identity;

    private void Start()
    {
        Instance = this;

        eventOne.SetActive(false);
        reassemblyOne.SetActive(false);

        singleDetectionBarcodes.Clear();
        hasScannedMocap = false;
        hasScannedWrench = false;
        spawnedWrenchObject = null; // Initialize as null

        if (eventOne == null) Debug.LogWarning("eventOne is not assigned in the Inspector!");
        if (reassemblyOne == null) Debug.LogWarning("reassemblyOne is not assigned in the Inspector!");
        if (reassemblyPositionMarker == null) Debug.LogWarning("reassemblyPositionMarker is not assigned in the Inspector!");
        if (FixedPositionHandler.Instance == null) Debug.LogWarning("FixedPositionHandler.Instance is null!");
    }

    public void Set(BarcodeTracker.Result result) => Set(result.text, result.corners);

    public void Set(string text, Vector3[] corners)
    {
        bool isSingleDetection = text == "Mocap" || text == "Standard Wrench";

        if (text == "Mocap" && hasScannedWrench && singleDetectionBarcodes.Contains("Mocap"))
        {
            singleDetectionBarcodes.Remove("Mocap");
        }

        if (isSingleDetection && singleDetectionBarcodes.Contains(text))
        {
            return;
        }

        if (isSingleDetection)
        {
            singleDetectionBarcodes.Add(text);
        }

        Vector3 topCenter = (corners[0] + corners[1]) / 2f;
        transform.position = topCenter;

        Vector3 up = (corners[0] - corners[3]).normalized;
        Vector3 right = (corners[2] - corners[3]).normalized;
        Vector3 normal = -Vector3.Cross(up, right).normalized;

        Vector3 center = (corners[0] + corners[1] + corners[2] + corners[3]) / 4f;

        for (int i = 0; i < 4; i++)
        {
            Vector3 dir = (corners[i] - center).normalized;
            offsetPositions[i] = corners[i] + (dir * lineRenderer.startWidth);
        }

        transform.rotation = Quaternion.LookRotation(normal, up);

        lineRenderer.SetPositions(offsetPositions);
        textMesh.text = text;

        if (text == "Mocap")
        {
            Debug.Log(text);
            if (!hasScannedMocap)
            {
                textMesh.text = "Object Detected!";
                SimulationUI.Instance.TriggerToolSelectUI();
                SimulationUI.Instance.TurnOffViewFinder();
                SimulationUI.Instance.TurnOffPanelView();

                eventOne.SetActive(true);
                if (eventOne != null && reassemblyPositionMarker != null)
                {
                    reassemblyPositionMarker.position = eventOne.transform.position;
                    reassemblyPositionMarker.rotation = eventOne.transform.rotation;
                    Debug.Log($"Set reassemblyPositionMarker to position: {reassemblyPositionMarker.position}, rotation: {reassemblyPositionMarker.rotation.eulerAngles}");

                    if (FixedPositionHandler.Instance != null)
                    {
                        FixedPositionHandler.Instance.UpdatePosition();
                    }
                }
                else
                {
                    Debug.LogWarning("eventOne or reassemblyPositionMarker is null; cannot set marker position!");
                }
                reassemblyOne.SetActive(false);

                SimulationManager.Instance.DisableTracking();
                hasScannedMocap = true;
            }
            else if (hasScannedWrench)
            {
                textMesh.text = "Second Mocap Detected!";
                Debug.Log("Second Mocap scan detected after Standard Wrench");
                SimulationUI.Instance.TurnOffViewFinder();

                reassemblyOne.SetActive(true);
                
                if (reassemblyPositionMarker != null)
                {
                    reassemblyOne.transform.position = reassemblyPositionMarker.position;
                    reassemblyOne.transform.rotation = reassemblyPositionMarker.rotation;
                }
                SimulationManager.Instance.DisableTracking();
            }
        }
        else if (text == "Standard Wrench")
        {
            Debug.Log("Detected Standard Wrench barcode");
            textMesh.text = "Correct Answer";
            SimulationUI.Instance.TriggerCorrectAnswerUI();
            SimulationUI.Instance.TurnOffViewFinder();
            SimulationUI.Instance.TurnOnPanelView();

            if (hasScannedMocap && eventOne != null)
            {
                eventOne.SetActive(false);
            }
            else
            {
                Debug.LogWarning("eventOne is null or Mocap hasn't been scanned yet!");
            }


            // This was commended out!
            SimulationManager.Instance.DisableTracking();
            hasScannedWrench = true;
        }
        else if (text == "Brake-fan gauge")
        {
            Debug.Log(text);
            textMesh.text = "Wrong Answer";
            SimulationUI.Instance.TriggerWrongAnswerUI();
            SimulationUI.Instance.TurnOffViewFinder();
            SimulationUI.Instance.TurnOnPanelView();

            if (hasScannedMocap)
            {
                eventOne.SetActive(false);
            }
            reassemblyOne.SetActive(false);
        }
        else if (text == "Torque Wrench")
        {
            Debug.Log(text);
            textMesh.text = "Wrong Answer";
            SimulationUI.Instance.TriggerTWrenchAnswer();
            SimulationUI.Instance.TurnOffViewFinder();
            SimulationUI.Instance.TurnOnPanelView();

            if (hasScannedMocap)
            {
                eventOne.SetActive(false);
            }
            reassemblyOne.SetActive(false);
        }
    }

    private IEnumerator EnsurePositionAfterActivation(GameObject target, Vector3 position, Quaternion rotation)
    {
        yield return new WaitForEndOfFrame();
        if (target != null)
        {
            target.transform.position = position;
            target.transform.rotation = rotation;
            Debug.Log($"Reapplied to {target.name}: position {target.transform.position}, rotation: {target.transform.rotation.eulerAngles}");
        }
    }

    public void ResetDetections()
    {
        singleDetectionBarcodes.Clear();
        hasScannedMocap = false;
        hasScannedWrench = false;

    }

    public void ResetSimulation()
    {
        ResetDetections();

        if (eventOne != null) eventOne.SetActive(false);
        if (reassemblyOne != null) reassemblyOne.SetActive(false);



        // Destroy the spawned wrench object if it exists
        if (spawnedWrenchObject != null)
        {
            Destroy(spawnedWrenchObject);
            spawnedWrenchObject = null; // Clear the reference
            Debug.Log("Spawned wrench object destroyed and cleared.");
        }

        if (textMesh != null) textMesh.text = "";
        if (lineRenderer != null) lineRenderer.SetPositions(new Vector3[4]);
    }
}
}