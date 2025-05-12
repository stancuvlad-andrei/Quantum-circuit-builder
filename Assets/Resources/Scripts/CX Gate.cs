using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CXGate : MonoBehaviour
{
    public Sprite defaultSprite; // Default sprite when inactive
    public Sprite activeSprite; // Sprite when active
    public float activationTime = 0.3f; // Time to show the active sprite
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    private ControlData controlSignal = null; // Control signal data
    public static event System.Action<CXGate> OnGateActivated; // Event for gate activation

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

    #region CXGate Methods

    public float ApplyGate(float incomingProbability, Vector3Int sourcePosition)
    {
        StartCoroutine(ActivateVisual());

        Vector3Int currentCell = GridBuildingSystem.current.gridLayout.WorldToCell(transform.position);
        Vector3Int incomingDir = sourcePosition - currentCell;

        Debug.Log($"CXGate received signal from {incomingDir}");

        if (controlSignal == null)
        {
            // First signal: store as control
            controlSignal = new ControlData
            {
                probability = incomingProbability,
                isCollapsed = (incomingProbability == 0f || incomingProbability == 1f),
                inputDirection = incomingDir
            };

            Debug.Log($"CXGate: Stored control signal from {incomingDir} (probability: {incomingProbability})");
            return incomingProbability;
        }
        else
        {
            // Second signal: treat as target
            if (incomingDir == controlSignal.inputDirection || incomingDir == -controlSignal.inputDirection)
            {
                Debug.Log($"CXGate: Invalid target direction {incomingDir} (same or opposite of control)");
                return incomingProbability;
            }

            Debug.Log($"CXGate: Received target signal from {incomingDir}");

            float targetProbability = incomingProbability;

            // If control qubit is collapsed in 1, flip the target probability
            if (controlSignal.isCollapsed && controlSignal.probability == 1f)
            {
                targetProbability = 1f - incomingProbability;
                Debug.Log("CXGate: Control qubit is 1, flipping target qubit probability.");
            }
            else
            {
                Debug.Log("CXGate: Control qubit is not 1, target qubit remains unchanged.");
            }

            // Start propagation coroutine
            StartCoroutine(PropagateBoth(
                controlSignal.inputDirection,
                incomingDir,
                controlSignal.probability,
                targetProbability,
                controlSignal.isCollapsed
            ));

            // Reset control signal after use
            controlSignal = null;
            OnGateActivated?.Invoke(this);
            return targetProbability;
        }
    }

    #endregion

    #region Wave Propagation

    private IEnumerator PropagateBoth(Vector3Int controlInputDir, Vector3Int targetInputDir, float controlProb, float targetProb, bool controlIsCollapsed)
    {
        Debug.Log($"CXGate: Waiting {activationTime}s before propagation...");
        yield return new WaitForSeconds(activationTime);

        Vector3Int controlOut = -controlInputDir;
        Vector3Int targetOut = -targetInputDir;

        Debug.Log($"CXGate: Propagating control ({controlProb}) to {controlOut}");
        PropagateDirection(controlOut, controlProb, controlIsCollapsed);

        Debug.Log($"CXGate: Propagating target ({targetProb}) to {targetOut}");
        PropagateDirection(targetOut, targetProb, controlIsCollapsed);
    }

    private void PropagateDirection(Vector3Int direction, float probability, bool isCollapsed)
    {
        Vector3Int currentCell = GridBuildingSystem.current.gridLayout.WorldToCell(transform.position);
        Vector3Int nextCell = currentCell + direction;

        if (GridBuildingSystem.current.placedBuildings.TryGetValue(nextCell, out Building neighbor))
        {
            if (neighbor.TryGetComponent<Path>(out Path path))
            {
                Debug.Log($"CXGate: Forwarding signal to {nextCell}");
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
