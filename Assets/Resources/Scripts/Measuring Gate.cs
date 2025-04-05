using UnityEngine;

public class MeasuringGate : MonoBehaviour
{
    public Sprite inactiveSprite;
    public Sprite state0Sprite;
    public Sprite state1Sprite;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        ResetState();
    }

    public void ReceiveMeasurement(int state, Vector3Int sourcePosition)
    {
        if (spriteRenderer == null) return;

        // Update sprite based on state
        if (state == 0 && state0Sprite != null)
        {
            spriteRenderer.sprite = state0Sprite;
        }
        else if (state == 1 && state1Sprite != null)
        {
            spriteRenderer.sprite = state1Sprite;
        }

        // Calculate current position
        Vector3Int currentCell = GridBuildingSystem.current.gridLayout.WorldToCell(transform.position);

        // Calculate incoming direction (from source to current)
        Vector3Int incomingDir = sourcePosition - currentCell;
        incomingDir.Clamp(Vector3Int.one * -1, Vector3Int.one);

        // Propagate to adjacent paths (excluding incoming direction)
        Vector3Int[] directions = { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down };
        foreach (var dir in directions)
        {
            if (dir == incomingDir) continue;

            Vector3Int neighborPos = currentCell + dir;
            if (GridBuildingSystem.current.placedBuildings.TryGetValue(neighborPos, out Building neighbor))
            {
                // Skip destroyed or null neighbors
                if (neighbor == null || neighbor.gameObject == null) continue;

                if (neighbor.TryGetComponent<Path>(out Path path))
                {
                    path.StartWave(currentCell, state);
                }
            }
        }
    }

    public void ResetState()
    {
        if (spriteRenderer != null && inactiveSprite != null)
        {
            spriteRenderer.sprite = inactiveSprite;
        }
    }
}