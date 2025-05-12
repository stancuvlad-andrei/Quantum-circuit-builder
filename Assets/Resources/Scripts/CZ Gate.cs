using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CZGate : MonoBehaviour
{
    public Sprite defaultSprite; // Default sprite when inactive
    public Sprite activeSprite; // Sprite when active
    public float activationTime = 0.3f; // Time to show the active sprite
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    private ControlData controlSignal = null; // First arriving signal
    public static event System.Action<CZGate> OnGateActivated; // Event for gate activation

    private class ControlData
    {
        public float probability;
        public bool isCollapsed;
        public Vector3Int inputDirection;
    }

    #region Unity Methods

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && defaultSprite != null)
        {
            spriteRenderer.sprite = defaultSprite;
        }
    }

    #endregion

    #region CZGate Methods

    public float ApplyGate(float incomingProbability, Vector3Int sourcePosition)
    {
        StartCoroutine(ActivateVisual());

        Vector3Int currentCell = GridBuildingSystem.current.gridLayout.WorldToCell(transform.position);
        Vector3Int incomingDir = sourcePosition - currentCell;

        Debug.Log($"CZGate received signal from {incomingDir}");

        if (controlSignal == null)
        {
            // First signal: store it
            controlSignal = new ControlData
            {
                probability = incomingProbability,
                isCollapsed = (incomingProbability == 0f || incomingProbability == 1f),
                inputDirection = incomingDir
            };

            Debug.Log($"CZGate: Stored first signal from {incomingDir} (probability: {incomingProbability})");
            return incomingProbability;
        }
        else
        {
            // Second signal: process together
            if (incomingDir == controlSignal.inputDirection || incomingDir == -controlSignal.inputDirection)
            {
                Debug.Log($"CZGate: Invalid second direction {incomingDir} (same or opposite of first)");
                return incomingProbability;
            }

            Debug.Log($"CZGate: Received second signal from {incomingDir}");

            float finalControlProb = controlSignal.probability;
            float finalTargetProb = incomingProbability;

            // If both are collapsed in 1, flip both to 0
            if (controlSignal.isCollapsed && controlSignal.probability == 1f &&
                (incomingProbability == 1f))
            {
                finalControlProb = 0f;
                finalTargetProb = 0f;
                Debug.Log("CZGate: Both signals are 1, flipping both to 0");
            }
            else
            {
                Debug.Log("CZGate: Condition not met, both signals continue unchanged.");
            }

            // Start propagation
            StartCoroutine(PropagateBoth(
                controlSignal.inputDirection,
                incomingDir,
                finalControlProb,
                finalTargetProb,
                true
            ));

            // Clear stored signal
            controlSignal = null;
            OnGateActivated?.Invoke(this);
            return finalTargetProb;
        }
    }

    #endregion

    #region Wave Propagation

    private IEnumerator PropagateBoth(Vector3Int firstInputDir, Vector3Int secondInputDir, float firstProb, float secondProb, bool isCollapsed)
    {
        Debug.Log($"CZGate: Waiting {activationTime}s before propagation...");
        yield return new WaitForSeconds(activationTime);

        Vector3Int firstOut = -firstInputDir;
        Vector3Int secondOut = -secondInputDir;

        Debug.Log($"CZGate: Propagating first ({firstProb}) to {firstOut}");
        PropagateDirection(firstOut, firstProb, isCollapsed);

        Debug.Log($"CZGate: Propagating second ({secondProb}) to {secondOut}");
        PropagateDirection(secondOut, secondProb, isCollapsed);
    }

    private void PropagateDirection(Vector3Int direction, float probability, bool isCollapsed)
    {
        Vector3Int currentCell = GridBuildingSystem.current.gridLayout.WorldToCell(transform.position);
        Vector3Int nextCell = currentCell + direction;

        if (GridBuildingSystem.current.placedBuildings.TryGetValue(nextCell, out Building neighbor))
        {
            if (neighbor.TryGetComponent<Path>(out Path path))
            {
                Debug.Log($"CZGate: Forwarding signal to {nextCell}");
                path.StartWave(currentCell, probability, isCollapsed);
            }
        }
    }

    private IEnumerator ActivateVisual()
    {
        if (activeSprite != null)
        {
            spriteRenderer.sprite = activeSprite;
            yield return new WaitForSeconds(activationTime);
            spriteRenderer.sprite = defaultSprite;
        }
    }

    #endregion

}
