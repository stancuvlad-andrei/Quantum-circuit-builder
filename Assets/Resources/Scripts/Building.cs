using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public bool placed { get; set; } // Indicates if the building is placed
    public BoundsInt area; // Area occupied by building
    public Transform spriteTransform; // Transform of the sprite
    public SpriteRenderer spriteRenderer; // Sprite renderer of the sprite
    public string infoText = "Default building info"; // Info text for the building

    #region Unity Methods

    void Start()
    {
        if (spriteTransform != null)
        {
            spriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
        }
    }

    #endregion

    #region Build Methods

    public bool CanBePlaced()
    {
        Vector3Int cellPos = GridBuildingSystem.current.gridLayout.LocalToCell(transform.position);
        BoundsInt areaTemp = area;
        areaTemp.position = cellPos;

        return GridBuildingSystem.current.CanPlaceBuilding(areaTemp);
    }

    public void Place()
    {
        Vector3Int cellPos = GridBuildingSystem.current.gridLayout.LocalToCell(transform.position);
        BoundsInt areaTemp = area;
        areaTemp.position = cellPos;
        GridBuildingSystem.current.PlaceBuilding(areaTemp);

        // Snap to grid center
        transform.position = GridBuildingSystem.current.gridLayout.CellToLocalInterpolated(cellPos + new Vector3(0.5f, 0.5f, 0));

        if (spriteTransform != null)
        {
            spriteTransform.localPosition = new Vector3(0, -0.05f, 0);
        }

        placed = true;
        GridBuildingSystem.current.inventoryUISlotImage.enabled = false;

        // Update sorting
        if (BuildingSorter.Instance != null)
        {
            BuildingSorter.Instance.RegisterBuilding(this);
        }

        // If this is a path, update neighbors and notify adjacent paths
        Path thisPath = GetComponent<Path>();
        if (thisPath != null)
        {
            thisPath.FindNeighbors();

            // Notify adjacent paths to update their neighbors
            Vector3Int[] directions = {
            Vector3Int.right, Vector3Int.left,
            Vector3Int.up, Vector3Int.down
        };

            foreach (var dir in directions)
            {
                Vector3Int neighborPos = cellPos + dir;
                if (GridBuildingSystem.current.placedBuildings.TryGetValue(neighborPos, out Building neighbor))
                {
                    Path neighborPath = neighbor.GetComponent<Path>();
                    if (neighborPath != null)
                    {
                        neighborPath.FindNeighbors();
                    }
                }
            }
        }
    }

    public void UpdateSortingOrder(int newOrder)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = newOrder;
        }
    }

    private void OnDestroy()
    {
        if (BuildingSorter.Instance != null)
        {
            BuildingSorter.Instance.UnregisterBuilding(this);
        }

        // Remove this building from placedBuildings
        if (GridBuildingSystem.current != null)
        {
            foreach (Vector3Int cell in area.allPositionsWithin)
            {
                GridBuildingSystem.current.placedBuildings.Remove(cell);
            }
        }
    }

    #endregion

}