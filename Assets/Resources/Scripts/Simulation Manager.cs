using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimulationManager : MonoBehaviour
{
    public Button startButton;             // Assign this in the Inspector
    public TextMeshProUGUI buttonText;     // Text component of the button (TextMeshPro)
    private bool simulationRunning = false;

    private void Start()
    {
        // Hook up the button click event
        startButton.onClick.AddListener(ToggleSimulation);
        UpdateButtonText();
    }

    // Toggle simulation on/off
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

    // When simulation starts, activate all qubits (randomizing their state and switching sprite)
    private void StartSimulation()
    {
        Qubit[] qubits = FindObjectsOfType<Qubit>();
        foreach (Qubit qubit in qubits)
        {
            qubit.Activate();
        }
    }

    // When simulation stops, deactivate all qubits (switching their sprite back)
    private void StopSimulation()
    {
        Qubit[] qubits = FindObjectsOfType<Qubit>();
        foreach (Qubit qubit in qubits)
        {
            qubit.Deactivate(); // This already calls Deactivate() on Qubit
        }

        Path[] paths = FindObjectsOfType<Path>();
        foreach (Path path in paths)
        {
            path.ResetPath();
            path.spriteRenderer.sprite = path.waveSprites[0]; // Immediate reset
        }

        Debug.Log("Simulation Stopped");
    }

    // Update button text based on simulation state
    private void UpdateButtonText()
    {
        if (buttonText != null)
        {
            buttonText.text = simulationRunning ? "Stop Simulation" : "Start Simulation";
        }
    }
}
