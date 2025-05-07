using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [SerializeField] private TutorialPrompt tutorialPrompt; // Reference to the tutorial prompt
    [SerializeField] private string[] tutorialMessages; // Array of tutorial messages

    [Header("References")]
    [SerializeField] private GridBuildingSystem gridSystem; // Reference to the grid system
    [SerializeField] private Button startSimulationButton; // Reference to the start simulation button

    private int currentMessageIndex = 0; // Index for the current tutorial message
    private bool levelCompleted; // Flag to check if the level is completed

    #region Unity Methods

    private void Start()
    {
        // Null-check critical references
        if (startSimulationButton == null || gridSystem == null || tutorialPrompt == null)
        {
            Debug.LogError("Critical references not assigned in LevelManager!");
            return;
        }

        startSimulationButton.onClick.AddListener(OnStartSimulation);
        gridSystem.OnBuildingPlaced += HandleBuildingPlaced;
        MeasuringGate.OnMeasurement += HandleMeasurement;
        ShowNextTutorialMessage();
    }

    #endregion

    #region Manager Methods

    private void ShowNextTutorialMessage()
    {
        if (currentMessageIndex >= tutorialMessages.Length) 
        { 
            return; 
        }

        tutorialPrompt.ShowTutorialMessage(tutorialMessages[currentMessageIndex]);

        // Set up conditions for advancing messages
        switch (currentMessageIndex)
        {
            case 0: // Welcome message
                StartCoroutine(AdvanceAfterDelay(5f));
                break;
            case 1: // Qbit placement
                // Handled by HandleBuildingPlaced() when Qubit is detected
                break;
            case 2: // Measurement gate placement
                // Handled by HandleBuildingPlaced() when MeasuringGate is detected 
                break;
            case 3: // Path connection
                // Handled by HandleBuildingPlaced() when Path is detected
                break;
            case 4: // Start simulation
                // Handled by button click
                break;
        }
    }

    private IEnumerator AdvanceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentMessageIndex++;
        ShowNextTutorialMessage();
    }

    private void HandleBuildingPlaced(Building building)
    {
        if (building == null) 
        { 
            return; 
        }

        // Check for required components using TryGetComponent
        if (currentMessageIndex == 1 && building.TryGetComponent<Qubit>(out _))
        {
            AdvanceTutorial();
        }
        else if (currentMessageIndex == 2 && building.TryGetComponent<MeasuringGate>(out _))
        {
            AdvanceTutorial();
        }
        else if (currentMessageIndex == 3 && building.TryGetComponent<Path>(out _))
        {
            AdvanceTutorial();
        }
    }

    private void AdvanceTutorial()
    {
        currentMessageIndex++;
        ShowNextTutorialMessage();
    }

    private void OnStartSimulation()
    {
        if (currentMessageIndex == 4)
        {
            currentMessageIndex++;
            tutorialPrompt.HideAllPanels();
        }
    }

    private void HandleMeasurement(MeasuringGate gate, int measuredState)
    {
        if (!levelCompleted)
        {
            levelCompleted = true;
            Debug.Log("LEVEL COMPLETED!");
            tutorialPrompt.ShowTutorialMessage("Well done! You completed the level!");
        }
    }

    #endregion

    #region Helper Methods

    private void OnDestroy()
    {
        // Proper cleanup
        if (gridSystem != null) 
        { 
            gridSystem.OnBuildingPlaced -= HandleBuildingPlaced; 
        }
        if (startSimulationButton != null) 
        { 
            startSimulationButton.onClick.RemoveListener(OnStartSimulation); 
        }

        MeasuringGate.OnMeasurement -= HandleMeasurement;
    }

    #endregion

}