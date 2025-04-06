using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D rayHit = Physics2D.GetRayIntersection(ray);

        if (rayHit.collider == null)
        {
            return;
        }

        Building building = rayHit.collider.GetComponent<Building>();
        if (building != null && building.placed)
        {
            gridSystem.SelectBuildingForDeletion(building);
            gridSystem.SelectBuildingForRelocation(building);
        }
    }

    #endregion

}
