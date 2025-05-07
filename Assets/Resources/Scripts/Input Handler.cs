using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private Camera mainCamera; // Main camera reference
    [SerializeField] private GridBuildingSystem gridSystem; // GridBuildingSystem reference
    [SerializeField] private TutorialPrompt tutorialPrompt; // Tutorial prompt reference

    #region Unity Methods

    private void Awake()
    {
        mainCamera = Camera.main;

        // First try to find existing instance
        if (gridSystem == null)
        {
            gridSystem = FindObjectOfType<GridBuildingSystem>();
        }

        // Then check singleton reference
        if (gridSystem == null && GridBuildingSystem.current != null)
        {
            gridSystem = GridBuildingSystem.current;
        }

        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
        }

        if (gridSystem == null)
        {
            Debug.LogError("GridBuildingSystem not found! Make sure it's in the scene and initialized.");
        }
    }

    void Start()
    {

    }

    void Update()
    {

    }

    #endregion

    #region Input Methods

    public void onClick(InputAction.CallbackContext context)
    {
        if (!context.started) 
        { 
            return; 
        }
        if (gridSystem == null) 
        { 
            return; 
        }
        if (EventSystem.current.IsPointerOverGameObject()) 
        { 
            return; 
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider == null)
        {
            gridSystem.ClearSelection();
            if (tutorialPrompt != null) 
            {
                tutorialPrompt.ShowTutorialMessage("Try placing buildings on empty grid spaces!");
            }

            return;
        }

        Building b = hit.collider.GetComponent<Building>();
        if (b != null && b.placed)
        {
            if (tutorialPrompt != null)
            {
                tutorialPrompt.ShowObjectInfo(b.GetDescription());
            }
            gridSystem.ClearSelection();
            gridSystem.SelectBuildingForDeletion(b);
            gridSystem.SelectBuildingForRelocation(b);
        }
        else
        {
            if (tutorialPrompt != null) 
            {
                tutorialPrompt.ShowTutorialMessage("Click on placed buildings to interact!");
            }
        }
    }

    #endregion

}
