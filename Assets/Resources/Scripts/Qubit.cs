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
        TriggerAdjacentPaths();
        Debug.Log($"Qubit activated at {transform.position}");
        Debug.Log($"Qubit state: {state}");
    }

    private void TriggerAdjacentPaths()
    {
        Building building = GetComponent<Building>();
        if (building == null) return;

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
                    path.StartWave(null, state);
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