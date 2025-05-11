using UnityEngine;
using System.Collections;

public class XGate : MonoBehaviour
{
    public Sprite defaultSprite; // Default sprite when inactive
    public Sprite activeSprite; // Sprite when active
    public float activationTime = 0.3f; // Time to show the active sprite
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    public static event System.Action<XGate> OnGateActivated; // Event for gate activation

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

    #region XGate Methods

    public float ApplyGate(float incomingProbability, Vector3Int sourcePosition)
    {
        StartCoroutine(ActivateVisual());

        float result;

        if (incomingProbability == 0f || incomingProbability == 1f)
        {
            result = 1f - incomingProbability;
            Debug.Log($"XGate (Collapsed): Flipped {incomingProbability} to {result}");
        }
        else
        {
            result = 1f - incomingProbability;
            Debug.Log($"XGate (Uncollapsed): Flipped {incomingProbability} to {result}");
        }

        OnGateActivated?.Invoke(this);

        StartCoroutine(PropagateWave(result, incomingProbability == 0f || incomingProbability == 1f, sourcePosition));

        return result;
    }

    #endregion

    #region Wave Propagation

    private IEnumerator PropagateWave(float modifiedProbability, bool isCollapsed, Vector3Int sourcePosition)
    {
        yield return new WaitForSeconds(activationTime);

        Vector3Int currentCell = GridBuildingSystem.current.gridLayout.WorldToCell(transform.position);
        Vector3Int incomingDir = sourcePosition - currentCell;
        incomingDir.Clamp(Vector3Int.one * -1, Vector3Int.one);
        Vector3Int[] directions = { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down };

        foreach (var dir in directions)
        {
            if (dir == incomingDir)
            {
                continue;
            }

            Vector3Int neighborPos = currentCell + dir;

            if (GridBuildingSystem.current.placedBuildings.TryGetValue(neighborPos, out Building neighbor))
            {
                if (neighbor == null)
                {
                    continue;
                }

                if (neighbor.TryGetComponent<Path>(out Path path))
                {
                    path.StartWave(currentCell, modifiedProbability, isCollapsed);
                }

                else if (neighbor.TryGetComponent<MeasuringGate>(out MeasuringGate gate))
                {
                    gate.ReceiveMeasurement(currentCell, modifiedProbability, isCollapsed);
                }
            }
        }
    }

    public void PropagateAfterGate(Vector3Int sourcePosition, float modifiedProbability, Vector3Int direction, bool isCollapsed)
    {
        Vector3Int currentCell = GridBuildingSystem.current.gridLayout.WorldToCell(transform.position);
        Vector3Int nextPos = currentCell + direction; // Propagate in the same direction as input

        if (GridBuildingSystem.current.placedBuildings.TryGetValue(nextPos, out Building neighbor))
        {
            if (neighbor == null)
            {
                return;
            }

            if (neighbor.TryGetComponent<Path>(out Path path))
            {
                path.StartWave(currentCell, modifiedProbability, isCollapsed);
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