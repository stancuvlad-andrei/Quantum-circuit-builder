using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public bool placed { get; set; }
    public BoundsInt area;
    public Transform spriteTransform;
    public SpriteRenderer spriteRenderer;

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

        if (spriteTransform != null)
        {
            spriteTransform.localPosition = new Vector3(0, -0.05f, 0);
        }

        placed = true;
        GridBuildingSystem.current.inventoryUISlotImage.enabled = false;

        // Register with sorter
        if (BuildingSorter.Instance != null)
        {
            BuildingSorter.Instance.RegisterBuilding(this);
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
    }

    #endregion
}