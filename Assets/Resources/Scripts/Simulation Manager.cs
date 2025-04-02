using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimulationManager : MonoBehaviour
{
    public Button startButton;
    public TextMeshProUGUI buttonText;
    private bool simulationRunning = false;

    private void Start()
    {
        startButton.onClick.AddListener(ToggleSimulation);
        UpdateButtonText();
    }

    public void ToggleSimulation()
    {
        simulationRunning = !simulationRunning;
        if (simulationRunning) StartSimulation();
        else StopSimulation();
        UpdateButtonText();
    }

    private void StartSimulation()
    {
        Qubit[] qubits = FindObjectsOfType<Qubit>();
        foreach (Qubit qubit in qubits)
        {
            qubit.Activate();
        }
    }

    private void StopSimulation()
    {
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
}