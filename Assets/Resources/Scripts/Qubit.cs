using UnityEngine;

public class Qubit : MonoBehaviour
{
    public Sprite activeSprite; // Sprite when active
    public Sprite inactiveSprite; // Sprite when inactive
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    [Range(0, 1)] public float probability = 0.5f; // Probability of state 1
    public bool collapsed; // True after measurement

    #region Unity Methods

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

    #endregion

    #region Qubit Methods

    public void Activate()
    {
        if (!collapsed)
        {
            // Only randomize if not collapsed
            probability = Random.Range(0f, 1f);
        }

        if (spriteRenderer != null && activeSprite != null)
        {
            spriteRenderer.sprite = activeSprite;
        }

        Vector3Int currentCell = GridBuildingSystem.current.gridLayout.WorldToCell(transform.position);
        TriggerAdjacentPaths(currentCell);
        Debug.Log($"Qubit activated. Probability: {probability}");
    }

    private void TriggerAdjacentPaths(Vector3Int sourcePosition)
    {
        Vector3Int[] directions = { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down };
        foreach (var dir in directions)
        {
            Vector3Int neighborPos = sourcePosition + dir;
            if (GridBuildingSystem.current.placedBuildings.TryGetValue(neighborPos, out Building neighbor))
            {
                if (neighbor == null) 
                { 
                    continue; 
                }

                if (neighbor.TryGetComponent<Path>(out Path path))
                {
                    path.StartWave(sourcePosition, probability, collapsed);
                }
            }
        }
    }

    public void CollapseState(int measuredState)
    {
        probability = measuredState; // 0 or 1
        collapsed = true;
        Debug.Log($"Qubit collapsed to state: {measuredState}");
    }

    public void ResetState()
    {
        collapsed = false;
        probability = 0.5f;
    }

    public void Deactivate()
    {
        if (spriteRenderer != null && inactiveSprite != null)
        {
            spriteRenderer.sprite = inactiveSprite;
        }
    }

    #endregion

}