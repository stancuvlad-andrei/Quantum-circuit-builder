using UnityEngine;

public class Qubit : MonoBehaviour
{
    public Sprite activeSprite;
    public Sprite inactiveSprite;
    private SpriteRenderer spriteRenderer;
    public int state;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && inactiveSprite != null)
        {
            spriteRenderer.sprite = inactiveSprite;
        }
        else
        {
            Debug.LogError("SpriteRenderer not found!");
        }
    }

    public void Activate()
    {
        state = Random.Range(0, 2);
        if (spriteRenderer != null && activeSprite != null)
        {
            spriteRenderer.sprite = activeSprite;
        }

        // Get current position
        Vector3Int currentCell = GridBuildingSystem.current.gridLayout.WorldToCell(transform.position);

        // Trigger adjacent paths with qubit's position
        TriggerAdjacentPaths(currentCell);
        Debug.Log($"Qubit activated at {transform.position}");
        Debug.Log($"Qubit state: {state}");
    }

    private void TriggerAdjacentPaths(Vector3Int sourcePosition)
    {
        Vector3Int[] directions = { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down };
        foreach (var dir in directions)
        {
            Vector3Int neighborPos = sourcePosition + dir;
            if (GridBuildingSystem.current.placedBuildings.TryGetValue(neighborPos, out Building neighbor))
            {
                if (neighbor.TryGetComponent<Path>(out Path path))
                {
                    path.StartWave(sourcePosition, state);
                }
            }
        }
    }

    public void Deactivate()
    {
        if (spriteRenderer != null && inactiveSprite != null)
        {
            spriteRenderer.sprite = inactiveSprite;
        }
    }
}