using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SimulationManager : MonoBehaviour
{
    public Button startButton; // Button to start/stop simulation
    public Button pauseButton; // New pause/resume button
    public TextMeshProUGUI buttonText; // Text on the start/stop button
    public TextMeshProUGUI pauseButtonText; // Text on the pause button
    public float waveInterval = 2f; // Interval for wave activation
    private bool simulationRunning = false; // Flag to check if simulation is running
    private Coroutine waveCoroutine; // Coroutine for wave loop
    public static bool isPaused; // Static flag for pause state

    #region Unity Methods

    private void Start()
    {
        startButton.onClick.AddListener(ToggleSimulation);
        pauseButton.onClick.AddListener(TogglePause);
        pauseButton.gameObject.SetActive(false);
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
        isPaused = false;
        pauseButton.gameObject.SetActive(true);
        UpdatePauseButtonText();
        ActivateAllQubits();
        waveCoroutine = StartCoroutine(WaveLoop());
    }

    private void StopSimulation()
    {
        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
        }

        isPaused = false;
        pauseButton.gameObject.SetActive(false);

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

    public void TogglePause()
    {
        isPaused = !isPaused;
        UpdatePauseButtonText();
    }

    private void UpdateButtonText()
    {
        if (buttonText != null)
        {
            buttonText.text = simulationRunning ? "Stop Simulation" : "Start Simulation";
        }
    }

    private void UpdatePauseButtonText()
    {
        if (pauseButtonText != null)
        {
            pauseButtonText.text = isPaused ? "Resume" : "Pause";
        }
    }

    #endregion

    #region Coroutine

    private IEnumerator WaveLoop()
    {
        float waveTimer = 0f;

        while (simulationRunning)
        {
            if (!isPaused)
            {
                waveTimer += Time.deltaTime;
                if (waveTimer >= waveInterval)
                {
                    ActivateAllQubits();
                    // Reset timer
                    waveTimer = 0f;
                }
            }
            yield return null;
        }
    }

    #endregion

    private void ActivateAllQubits()
    {
        Qubit[] qubits = FindObjectsOfType<Qubit>();
        foreach (Qubit qubit in qubits)
        {
            qubit.ResetState();
            qubit.Activate();
        }
    }
}