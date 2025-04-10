using UnityEngine;

public class MeasuringGate : MonoBehaviour
{
    public Sprite inactiveSprite; // Sprite when inactive
    public Sprite state0Sprite; // Sprite for state 0
    public Sprite state1Sprite; // Sprite for state 1
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component

    #region Unity Methods

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        ResetState();
    }

    #endregion

    #region Measurement Methods

    public void ReceiveMeasurement(Vector3Int sourcePosition, float incomingProbability, bool isCollapsed)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        // Collapse the qubit's state
        int measuredState = (Random.value <= incomingProbability) ? 1 : 0;

        // Update sprite based on state
        spriteRenderer.sprite = (measuredState == 0) ? state0Sprite : state1Sprite;

        // Propagate collapsed state (0 or 1 probability)
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
                    path.StartWave(currentCell, measuredState, true); // Pass measuredState as 0 or 1
                }
            }
        }
    }

    #endregion

    #region State Management

    public void ResetState()
    {
        if (spriteRenderer != null && inactiveSprite != null)
        {
            spriteRenderer.sprite = inactiveSprite;
        }
    }

    #endregion

}