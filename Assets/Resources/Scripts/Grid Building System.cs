using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

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


    #region Unity Methods
    private void Awake()
    {
        current = this;
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
            if (EventSystem.current.IsPointerOverGameObject(0))
            {
                return;
            }

            if (!temp.placed)
            {
                Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int cellPos = gridLayout.LocalToCell(touchPos);

                if (prevPos != cellPos)
                {
                    temp.transform.localPosition = gridLayout.CellToLocalInterpolated(cellPos + new Vector3(.5f, .5f, .0f));
                    prevPos = cellPos;
                    FollowBuilding();
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
        }
    }

    #endregion

    #region Tilemap Management

    #endregion

    #region Building Placement

    public void initializeWithBuilding(GameObject building)
    {
        temp = Instantiate(building, Vector3.zero, Quaternion.identity).GetComponent<Building>();
        FollowBuilding();
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

        for(int i = 0; i < baseArray.Length; i++)
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
