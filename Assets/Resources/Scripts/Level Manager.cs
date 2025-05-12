using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private int levelID; // ID of the current level
    private bool gateActivated; // Flag to check if a gate is activated
    private string[] currentLevelMessages =>
    (levelID == 1) ? level1Messages :
    (levelID == 2) ? level2Messages :
    (levelID == 3) ? level3Messages :
    (levelID == 4) ? level4Messages :
    level5Messages; // Array of messages based on level ID

    [Header("Tutorial Settings")]
    [SerializeField] private TutorialPrompt tutorialPrompt; // Reference to the tutorial prompt
    [SerializeField] private string[] level1Messages; // Array of tutorial messages for level 1
    [SerializeField] private string[] level2Messages; // Array of tutorial messages for level 2
    [SerializeField] private string[] level3Messages; // Array of tutorial messages for level 3
    [SerializeField] private string[] level4Messages; // Array of tutorial messages for level 4
    [SerializeField] private string[] level5Messages; // Array of tutorial messages for level 5

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

        gateActivated = false;
        startSimulationButton.onClick.AddListener(OnStartSimulation);
        gridSystem.OnBuildingPlaced += HandleBuildingPlaced;
        MeasuringGate.OnMeasurement += HandleMeasurement;
        XGate.OnGateActivated += HandleGateActivated;
        YGate.OnGateActivated += HandleGateActivated;
        ZGate.OnGateActivated += HandleGateActivated;
        HGate.OnGateActivated += HandleGateActivated;
        SGate.OnGateActivated += HandleGateActivated;
        TGate.OnGateActivated += HandleGateActivated;
        CXGate.OnGateActivated += HandleGateActivated;
        CZGate.OnGateActivated += HandleGateActivated;
        SPGate.OnGateActivated += HandleGateActivated;
        ShowNextTutorialMessage();
    }

    #endregion

    #region Manager Methods

    private void ShowNextTutorialMessage()
    {
        if (currentMessageIndex >= currentLevelMessages.Length)
        {
            return;
        }

        tutorialPrompt.ShowTutorialMessage(currentLevelMessages[currentMessageIndex]);

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

        if (levelID == 1)
        {
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
        else if (levelID == 2)
        {
            if (currentMessageIndex == 1 && building.TryGetComponent<Qubit>(out _))
            {
                AdvanceTutorial();
            }
            else if (currentMessageIndex == 2 && (building.TryGetComponent<XGate>(out _) || building.TryGetComponent<YGate>(out _) || building.TryGetComponent<ZGate>(out _)))
            {
                AdvanceTutorial();
            }
            else if (currentMessageIndex == 3 && building.TryGetComponent<MeasuringGate>(out _))
            {
                AdvanceTutorial();
            }
            else if (currentMessageIndex == 4 && building.TryGetComponent<Path>(out _))
            {
                AdvanceTutorial();
            }
        }
        else if (levelID == 3)
        {
            if (currentMessageIndex == 1 && building.TryGetComponent<Qubit>(out _))
            {
                AdvanceTutorial();
            }
            if (currentMessageIndex == 2 && building.TryGetComponent<HGate>(out _))
            {
                AdvanceTutorial();
            }
            else if (currentMessageIndex == 3 && building.TryGetComponent<MeasuringGate>(out _))
            {
                AdvanceTutorial();
            }
            else if (currentMessageIndex == 4 && building.TryGetComponent<Path>(out _))
            {
                AdvanceTutorial();
            }
        }
        else if (levelID == 4)
        {
            if (currentMessageIndex == 1 && building.TryGetComponent<Qubit>(out _))
            {
                AdvanceTutorial();
            }
            else if (currentMessageIndex == 2 && (building.TryGetComponent<SGate>(out _) || building.TryGetComponent<TGate>(out _)))
            {
                AdvanceTutorial();
            }
            else if (currentMessageIndex == 3 && building.TryGetComponent<MeasuringGate>(out _))
            {
                AdvanceTutorial();
            }
            else if (currentMessageIndex == 4 && building.TryGetComponent<Path>(out _))
            {
                AdvanceTutorial();
            }
        }
        else if (levelID == 5)
        {
            if (currentMessageIndex == 1 && building.TryGetComponent<Qubit>(out _))
            {
                AdvanceTutorial();
            }
            else if (currentMessageIndex == 2 && (building.TryGetComponent<CXGate>(out _) || building.TryGetComponent<CZGate>(out _) || building.TryGetComponent<SPGate>(out _)))
            {
                AdvanceTutorial();
            }
            else if (currentMessageIndex == 3 && building.TryGetComponent<MeasuringGate>(out _))
            {
                AdvanceTutorial();
            }
            else if (currentMessageIndex == 4 && building.TryGetComponent<Path>(out _))
            {
                AdvanceTutorial();
            }
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
        if (levelCompleted) return;

        if (levelID == 1 || ((levelID == 2 || levelID == 3 || levelID == 4 || levelID == 5) && gateActivated))
        {
            levelCompleted = true;
            Debug.Log($"LEVEL {levelID} COMPLETED!");
            tutorialPrompt.ShowTutorialMessage("Well done! You completed the level!");
        }
    }

    private void HandleGateActivated<T>(T gate) => gateActivated = true;

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
        XGate.OnGateActivated -= HandleGateActivated;
        YGate.OnGateActivated -= HandleGateActivated;
        ZGate.OnGateActivated -= HandleGateActivated;
        HGate.OnGateActivated -= HandleGateActivated;
        SGate.OnGateActivated -= HandleGateActivated;
        TGate.OnGateActivated -= HandleGateActivated;
        CXGate.OnGateActivated -= HandleGateActivated;
        CZGate.OnGateActivated -= HandleGateActivated;
        SPGate.OnGateActivated -= HandleGateActivated;
    }

    #endregion

}