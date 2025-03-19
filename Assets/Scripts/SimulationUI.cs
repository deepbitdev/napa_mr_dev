using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Anaglyph.DisplayCapture.Barcodes;

public class SimulationUI : MonoBehaviour
{
    public static SimulationUI Instance { get; private set; }

    // Instruction label text
    public TextMeshProUGUI instructionTxt;

    public GameObject restartButton;
    public GameObject tryAgainButton;

    // Ok Button
    public GameObject okButton;

    public GameObject viewFinderUIObj;

    public GameObject logoUiObj;
    public GameObject panelViewObj;

    void Start()
    {
        Instance = this;

        RestartSimulation(); // Start the simulation in a reset state
    }

    public void TurnOnViewFinder()
    {
        viewFinderUIObj.SetActive(true);
    }

    public void TurnOffViewFinder()
    {
        viewFinderUIObj.SetActive(false);
    }

    public void TurnOnLogo()
    {
        logoUiObj.SetActive(true);
    }

    public void TurnOffLogo()
    {
        logoUiObj.SetActive(false);
    }

    public void TurnOnPanelView()
    {
        panelViewObj.SetActive(true);
    }

    public void TurnOffPanelView()
    {
        panelViewObj.SetActive(false);
    }

    public void SimulationStartText()
    {
        instructionTxt.text = "Scan QRCode to begin training";
        TurnOffPanelView();
        restartButton.SetActive(false);
        tryAgainButton.SetActive(false);
        okButton.SetActive(false);

        SimulationManager.Instance.EnableTracking();
    }

    public void TriggerToolSelectUI()
    {
        instructionTxt.text = "There are three tools in front of you: a standard wrench, a torque wrench, and a brake-fan gauge. Each tool has a specific function, but only one is correct for removing the caliper bolt from the rear brake mock-up. Pick up the correct tool to solve the problem.";
        FindObjectOfType<FixedPositionHandler>()?.UpdatePosition(); // Call after Mocap
        SimulationManager.Instance.EnableTracking();
    }

    public void TriggerWrongAnswerUI()
    {
        instructionTxt.text = "You selected the brake-fan gauge. That tool is used for measuring pad thickness, not for removing the caliper bolt. Would you like to try again?";
        SimulationManager.Instance.WrongAnswerSound();
        SimulationManager.Instance.TriggerWrongAnswerOne();
        TurnOnPanelView();
        tryAgainButton.SetActive(true);
        StartCoroutine(RestartScan());
    }

    public void TriggerTWrenchAnswer()
    {
        instructionTxt.text = "You selected the torque wrench. That tool is correct for assembly, but not for removing the caliper bolt. Let’s try again.";
        SimulationManager.Instance.WrongAnswerSound();
        SimulationManager.Instance.TriggerWrongAnswerTwo();
        tryAgainButton.SetActive(true);
        StartCoroutine(RestartScan());
    }

    IEnumerator RestartScan()
    {
        yield return new WaitForSeconds(10);
        TurnOffPanelView();
        TurnOnViewFinder();
        SimulationManager.Instance.EnableTracking();
        StopCoroutine(RestartScan());
    }

    public void TryAgainUI()
    {
        TurnOffPanelView();
        TurnOnViewFinder();
        SimulationManager.Instance.EnableTracking();
    }

    public void TriggerCorrectAnswerUI()
    {
        instructionTxt.text = "Correct tool";
        restartButton.SetActive(false);
        tryAgainButton.SetActive(false);
        okButton.SetActive(true);
        SimulationManager.Instance.CorrectAnswerSound();
        SimulationManager.Instance.TriggerCorrectAnswer();


    }

    public void TriggerNextTrainingStep()
    {
        restartButton.SetActive(false);
        tryAgainButton.SetActive(false);
        okButton.SetActive(false);
        TurnOffPanelView();
        TurnOnViewFinder();
        // SimulationManager.Instance.EnableTracking();
    }

    public void TriggerTrainingComplete()
    {
        instructionTxt.text = "Congratulations! You have successfully completed this Mixed Reality demonstration";
        restartButton.SetActive(true);
        tryAgainButton.SetActive(false);
        okButton.SetActive(false);

        TurnOnPanelView();
        TurnOffViewFinder();
    }

    public void EndSimulation()
    {
        StartCoroutine(TriggerEndSimUI());
    }

    IEnumerator TriggerEndSimUI()
    {
        yield return new WaitForSeconds(10);
        SimulationManager.Instance.TriggerCompletion();
        TriggerRestartUI();
    }

    public void TriggerRestartUI()
    {
        instructionTxt.text = "Congratulations! You have successfully completed this Mixed Reality demonstration";
    }

    // Enhanced method to restart the simulation
    public void RestartSimulation()
    {
        // Stop any running coroutines to prevent conflicts
        StopAllCoroutines();

        StartCoroutine(RestartWithLogo());
    }

    private IEnumerator RestartWithLogo()
    {
        if(logoUiObj != null)
        {
            logoUiObj.SetActive(true);
            TurnOffViewFinder();
            TurnOffPanelView();
            Debug.Log("Displaying Logo");
            yield return new WaitForSeconds(5f);
            logoUiObj.SetActive(false);
            Debug.Log("Hiding Logo");
        }
        else
        {
            Debug.LogWarning("Logo is not assigned");
        }

         // Reset UI state
        SimulationStartText();
        TurnOnViewFinder();
        TurnOffPanelView();

        // Reset Indicator state
        if (Indicator.Instance != null)
        {
            Indicator.Instance.ResetSimulation();
        }
        else
        {
            Debug.LogWarning("Indicator.Instance is null. Ensure it is properly assigned.");
        }

        // Reset SimulationManager state (if applicable)
        if (SimulationManager.Instance != null)
        {
            SimulationManager.Instance.ResetState(); // Add this method to SimulationManager
        }
    }
}