using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGate : MonoBehaviour
{
    public Sprite defaultSprite; // Default sprite when inactive
    public Sprite activeSprite; // Sprite when active
    public float activationTime = 0.3f; // Time to show the active sprite
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component

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

    #region SGate Methods

    public float ApplyGate(float incomingProbability, Vector3Int sourcePosition)
    {
        StartCoroutine(ActivateVisual());

        float result = incomingProbability;

        if (result != 0.5f)
        {
            // Apply 50% adjustment
            if (result > 0.5f)
            {
                result -= 0.5f; // Subtract 50% for probabilities >50%
            }
            else
            {
                result += 0.5f; // Add 50% for probabilities <50%
            }

            // Clamp between 0% and 100%
            result = Mathf.Clamp01(result);
        }

        Debug.Log($"SGate: Adjusted probability from {incomingProbability} to {result}");
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
