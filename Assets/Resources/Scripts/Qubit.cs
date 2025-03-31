using UnityEngine;

public class Qubit : MonoBehaviour
{
    public Sprite activeSprite;    // Sprite for activated state
    public Sprite inactiveSprite;  // Sprite for deactivated state

    private SpriteRenderer spriteRenderer;
    public int state;

    private void Awake()
    {
        // This will search this GameObject and its children for a SpriteRenderer
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
        TriggerAdjacentPaths(); // Trigger paths
        Debug.Log($"Qubit activated at {transform.position}");
    }

    private void TriggerAdjacentPaths()
    {
        Building building = GetComponent<Building>();
        if (building == null) return;

        // Use grid-aligned position
        Vector3Int centerCell = GridBuildingSystem.current.gridLayout.WorldToCell(transform.position);

        Vector3Int[] directions = {
            Vector3Int.right, Vector3Int.left,
            Vector3Int.up, Vector3Int.down
        };

        foreach (var dir in directions)
        {
            Vector3Int neighborPos = centerCell + dir;
            if (GridBuildingSystem.current.placedBuildings.TryGetValue(neighborPos, out Building neighbor))
            {
                if (neighbor.TryGetComponent<Path>(out Path path))
                {
                    // Start wave with null source (qubit-initiated)
                    path.StartWave(null);
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
