using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private Camera mainCamera; // Main camera reference
    [SerializeField] private GridBuildingSystem gridSystem; // GridBuildingSystem reference

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

        // Raycast only if not over UI
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        // If we hit nothing clear any selection
        if (hit.collider == null)
        {
            // (optionally skip if user is panning / dragging the view)
            gridSystem.ClearSelection();
            return;
        }

        // If we hit a placed Building switch selection
        Building b = hit.collider.GetComponent<Building>();
        if (b != null && b.placed)
        {
            gridSystem.ClearSelection();
            gridSystem.SelectBuildingForDeletion(b);
            gridSystem.SelectBuildingForRelocation(b);
        }
    }

    #endregion

}
