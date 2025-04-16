using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SPGate : MonoBehaviour
{
    public Sprite defaultSprite; // Default sprite when inactive
    public Sprite activeSprite;  // Sprite when active
    public float activationTime = 0.3f; // Time to show the active sprite
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    private ControlData firstSignal = null; // First signal received

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

    #region SPGate Methods

    public float ApplyGate(float incomingProbability, Vector3Int sourcePosition)
    {
        StartCoroutine(ActivateVisual());

        Vector3Int currentCell = GridBuildingSystem.current.gridLayout.WorldToCell(transform.position);
        Vector3Int incomingDir = sourcePosition - currentCell;

        Debug.Log($"SPGate received signal from {incomingDir}");

        if (firstSignal == null)
        {
            // First signal: store it
            firstSignal = new ControlData
            {
                probability = incomingProbability,
                isCollapsed = (incomingProbability == 0f || incomingProbability == 1f),
                inputDirection = incomingDir
            };

            Debug.Log($"SPGate: Stored first signal from {incomingDir} (probability: {incomingProbability})");
            return incomingProbability;
        }
        else
        {
            // Second signal: process together
            if (incomingDir == firstSignal.inputDirection || incomingDir == -firstSignal.inputDirection)
            {
                Debug.Log($"SPGate: Invalid second direction {incomingDir} (same or opposite of first)");
                return incomingProbability;
            }

            Debug.Log($"SPGate: Received second signal from {incomingDir}");

            float swappedFirst = incomingProbability;
            float swappedSecond = firstSignal.probability;

            Debug.Log($"SPGate: Swapping states: first={firstSignal.probability}, second={incomingProbability}");

            // Start propagation
            StartCoroutine(PropagateBoth(
                firstSignal.inputDirection,
                incomingDir,
                swappedFirst,
                swappedSecond,
                firstSignal.isCollapsed || (incomingProbability == 0f || incomingProbability == 1f)
            ));

            // Clear stored signal
            firstSignal = null;

            return swappedSecond;
        }
    }

    #endregion

    #region Wave Propagation

    private IEnumerator PropagateBoth(Vector3Int firstInputDir, Vector3Int secondInputDir, float firstProb, float secondProb, bool isCollapsed)
    {
        Debug.Log($"SPGate: Waiting {activationTime}s before propagation...");
        yield return new WaitForSeconds(activationTime);

        Vector3Int firstOut = -firstInputDir;
        Vector3Int secondOut = -secondInputDir;

        Debug.Log($"SPGate: Propagating first (swapped={firstProb}) to {firstOut}");
        PropagateDirection(firstOut, firstProb, isCollapsed);

        Debug.Log($"SPGate: Propagating second (swapped={secondProb}) to {secondOut}");
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
                Debug.Log($"SPGate: Forwarding signal to {nextCell}");
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
