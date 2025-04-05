using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public enum TileType
{
    empty,
    white,
    green,
    red
}

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem current; // Singleton instance
    public GridLayout gridLayout; // Grid layout for tilemaps
    public Tilemap mainTilemap; // Main tilemap for building placement
    public Tilemap tempTilemap; // Temporary tilemap for building preview
    private static Dictionary<TileType, TileBase> tileBases = new Dictionary<TileType, TileBase>(); // Tile bases for tilemaps
    private Building temp; // Temporary building
    private Vector3 prevPos; // Previous position of the building
    private BoundsInt prevArea; // Previous area of the building
    public Image inventoryUISlotImage; // Inventory UI slot image
    private SpriteRenderer tempSpriteRenderer; // Temporary sprite renderer
    [SerializeField] private BuildingSorter buildingSorter; // Building sorter
    [Header("Delete System")] // Delete system
    public Button deleteButton; // Delete button
    private Building selectedBuildingForDeletion; // Selected building for deletion
    public Button relocateButton; // Relocate button
    private Building selectedBuildingForRelocation; // Selected building for relocation
    public Dictionary<Vector3Int, Building> placedBuildings = new Dictionary<Vector3Int, Building>(); // Placed buildings


    #region Unity Methods
    private void Awake()
    {
        if (current != null && current != this)
        {
            Destroy(gameObject);
        }
        else
        {
            current = this;
        }

        // Initialize other components here
        if (mainTilemap == null)
            Debug.LogError("Main Tilemap not assigned!");
    }

    private void Start()
    {
        string tilePath = @"Tiles/";
        tileBases.Add(TileType.empty, null);
        tileBases.Add(TileType.white, Resources.Load<TileBase>(tilePath + "WhiteTile"));
        tileBases.Add(TileType.green, Resources.Load<TileBase>(tilePath + "GreenTile"));
        tileBases.Add(TileType.red, Resources.Load<TileBase>(tilePath + "RedTile"));
    }

    private void Update()
    {
        if (!temp)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject(0)) return;

            if (!temp.placed)
            {
                // New: Activate building sprite on first click
                if (tempSpriteRenderer != null && !tempSpriteRenderer.enabled)
                {
                    tempSpriteRenderer.enabled = true;
                }

                // Existing movement code...
                Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int cellPos = gridLayout.LocalToCell(touchPos);

                if (prevPos != cellPos)
                {
                    temp.transform.localPosition = gridLayout.CellToLocalInterpolated(cellPos + new Vector3(.5f, .5f, .0f));
                    prevPos = cellPos;
                    FollowBuilding();

                    // Update sorting order while moving
                    if (temp != null)
                    {
                        temp.UpdateSortingOrder(BuildingSorter.Instance.BaseSortingOrder + 999);
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            if (temp.CanBePlaced())
            {
                temp.Place();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            clearArea();
            Destroy(temp.gameObject);
            inventoryUISlotImage.enabled = false;
        }
    }

    #endregion

    #region Building Placement

    public void initializeWithBuilding(GameObject building)
    {
        temp = Instantiate(building, Vector3.zero, Quaternion.identity).GetComponent<Building>();

        // New: Get and disable the sprite renderer
        if (temp.spriteTransform != null)
        {
            tempSpriteRenderer = temp.spriteTransform.GetComponent<SpriteRenderer>();
            if (tempSpriteRenderer != null)
            {
                tempSpriteRenderer.enabled = false;
            }
        }

        // New: Show in inventory
        if (inventoryUISlotImage != null && tempSpriteRenderer != null)
        {
            inventoryUISlotImage.sprite = tempSpriteRenderer.sprite;
            inventoryUISlotImage.enabled = true;
        }

    }

    private void clearArea()
    {
        TileBase[] toClear = new TileBase[prevArea.size.x * prevArea.size.y * prevArea.size.z];
        FillTiles(toClear, TileType.empty);
        tempTilemap.SetTilesBlock(prevArea, toClear);
    }

    private void FollowBuilding()
    {
        clearArea();

        temp.area.position = gridLayout.WorldToCell(temp.gameObject.transform.position);
        BoundsInt buildingArea = temp.area;

        TileBase[] baseArray = GetTilesBlock(buildingArea, mainTilemap);

        int size = baseArray.Length;
        TileBase[] tileArea = new TileBase[size];

        for (int i = 0; i < baseArray.Length; i++)
        {
            if (baseArray[i] == tileBases[TileType.white])
            {
                tileArea[i] = tileBases[TileType.green];
            }
            else
            {
                FillTiles(tileArea, TileType.red);
                break;
            }
        }

        tempTilemap.SetTilesBlock(buildingArea, tileArea);
        prevArea = buildingArea;
    }

    public bool CanPlaceBuilding(BoundsInt area)
    {
        TileBase[] baseArray = GetTilesBlock(area, mainTilemap);
        foreach (TileBase tile in baseArray)
        {
            if (tile != tileBases[TileType.white])
            {
                Debug.Log("Can't place building here");
                return false;
            }
        }
        return true;
    }

    public void PlaceBuilding(BoundsInt area)
    {
        SetTilesBlock(area, TileType.empty, tempTilemap);
        SetTilesBlock(area, TileType.green, mainTilemap);

        // Track ALL cells in the area (even for single-tile)
        foreach (Vector3Int cell in area.allPositionsWithin)
        {
            if (!placedBuildings.ContainsKey(cell))
            {
                placedBuildings.Add(cell, temp);
            }
        }

        if (buildingSorter != null) buildingSorter.RegisterBuilding(temp);
    }

    public Building GetBuildingAt(Vector3Int cell)
    {
        placedBuildings.TryGetValue(cell, out Building building);
        return building;
    }

    public void SelectBuildingForDeletion(Building building)
    {
        if (building == null)
        {
            Debug.LogError("Tried to select null building!");
            return;
        }

        selectedBuildingForDeletion = building;

        if (inventoryUISlotImage != null && building.spriteRenderer != null)
        {
            inventoryUISlotImage.sprite = building.spriteRenderer.sprite;
            inventoryUISlotImage.enabled = true;
        }

        // Uppdate delete button position version
        /*if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(true);
            // Position button using canvas coordinates
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
                Camera.main,
                building.transform.position
            );
            deleteButton.GetComponent<RectTransform>().position = screenPos + new Vector2(0, 50);
        }*/

        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(true);
        }

        else
        {
            Debug.LogError("Delete button reference not set in GridBuildingSystem!");
        }
    }

    public void DeleteSelectedBuilding()
    {
        if (selectedBuildingForDeletion == null) return;

        foreach (Vector3Int cell in selectedBuildingForDeletion.area.allPositionsWithin)
        {
            placedBuildings.Remove(cell);
        }

        SetTilesBlock(selectedBuildingForDeletion.area, TileType.white, mainTilemap);
        Destroy(selectedBuildingForDeletion.gameObject);

        // Hide both buttons
        if (deleteButton != null)
            deleteButton.gameObject.SetActive(false);
        if (relocateButton != null)
            relocateButton.gameObject.SetActive(false);

        // Clear inventory UI
        inventoryUISlotImage.enabled = false;
        inventoryUISlotImage.sprite = null;

        selectedBuildingForDeletion = null;
    }

    public void SelectBuildingForRelocation(Building building)
    {
        if (building == null)
        {
            Debug.LogError("Tried to select null building for relocation!");
            return;
        }

        selectedBuildingForRelocation = building;

        if (relocateButton != null)
        {
            relocateButton.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Relocate button reference not set in GridBuildingSystem!");
        }
    }

    public void RelocateSelectedBuilding()
    {
        if (selectedBuildingForRelocation == null) return;

        // Free up the original area
        SetTilesBlock(selectedBuildingForRelocation.area, TileType.white, mainTilemap);
        BuildingSorter.Instance.UnregisterBuilding(selectedBuildingForRelocation);

        // Prepare the building for relocation
        temp = selectedBuildingForRelocation;
        temp.placed = false;

        // Reset sprite position with Y offset and enable it
        if (temp.spriteTransform != null)
        {
            // Set Y offset to 0.3f for visibility during relocation
            temp.spriteTransform.localPosition = new Vector3(0, 0.3f, 0);
            tempSpriteRenderer = temp.spriteTransform.GetComponent<SpriteRenderer>();
            tempSpriteRenderer.enabled = true;
        }

        // Update inventory UI
        if (inventoryUISlotImage != null && tempSpriteRenderer != null)
        {
            inventoryUISlotImage.sprite = tempSpriteRenderer.sprite;
            inventoryUISlotImage.enabled = true;
        }

        // Hide buttons
        relocateButton.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(false);
        selectedBuildingForRelocation = null;
    }

    #endregion

    #region Helper Methods

    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        TileBase[] tiles = new TileBase[area.size.x * area.size.y * area.size.z];
        int index = 0;

        foreach (Vector3Int v in area.allPositionsWithin)
        {
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);
            tiles[index] = tilemap.GetTile(pos);
            index++;
        }

        return tiles;
    }

    private static void SetTilesBlock(BoundsInt area, TileType type, Tilemap tileMap)
    {
        int size = area.size.x * area.size.y * area.size.z;
        TileBase[] tiles = new TileBase[size];
        FillTiles(tiles, type);
        tileMap.SetTilesBlock(area, tiles);
    }

    private static void FillTiles(TileBase[] tiles, TileType type)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = tileBases[type];
        }
    }

    #endregion
}