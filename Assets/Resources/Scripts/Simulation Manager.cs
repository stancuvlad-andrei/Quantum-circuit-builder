using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SimulationManager : MonoBehaviour
{
    public Button startButton; // Button to start/stop simulation
    public TextMeshProUGUI buttonText; // Text on the button
    public float waveInterval = 2f; // Interval for wave activation
    private bool simulationRunning = false; // Flag to check if simulation is running
    private Coroutine waveCoroutine; // Coroutine for wave loop

    #region Unity Methods

    private void Start()
    {
        startButton.onClick.AddListener(ToggleSimulation);
        UpdateButtonText();
    }

    #endregion

    #region Simulation Control

    public void ToggleSimulation()
    {
        simulationRunning = !simulationRunning;
        if (simulationRunning)
        {
            StartSimulation();
        }
        else
        {
            StopSimulation();
        }
        UpdateButtonText();
    }

    private void StartSimulation()
    {
        ActivateAllQubits();
        waveCoroutine = StartCoroutine(WaveLoop());
    }

    private void ActivateAllQubits()
    {
        Qubit[] qubits = FindObjectsOfType<Qubit>();

        foreach (Qubit qubit in qubits)
        {
            qubit.ResetState();
            qubit.Activate();
        }
    }

    private void StopSimulation()
    {
        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
        }

        Qubit[] qubits = FindObjectsOfType<Qubit>();

        foreach (Qubit qubit in qubits)
        {
            qubit.Deactivate();
        }

        Path[] paths = FindObjectsOfType<Path>();

        foreach (Path path in paths)
        {
            path.ResetPath();
            path.spriteRenderer.sprite = path.waveSprites[0];
        }

        MeasuringGate[] gates = FindObjectsOfType<MeasuringGate>();
        foreach (MeasuringGate gate in gates)
        {
            gate.ResetState();
        }

        Debug.Log("Simulation Stopped");
    }

    private void UpdateButtonText()
    {
        if (buttonText != null)
        {
            buttonText.text = simulationRunning ? "Stop Simulation" : "Start Simulation";
        }
    }

    #endregion

    #region Coroutine

    private IEnumerator WaveLoop()
    {
        while (simulationRunning)
        {
            yield return new WaitForSeconds(waveInterval);
            ActivateAllQubits();
        }
    }

    #endregion

}