using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public bool placed { get; private set; }
    public BoundsInt area;
    public Transform spriteTransform;

    #region Unity Methods

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    #endregion

    #region Build Methods

    public bool CanBePlaced()
    {
        Vector3Int cellPos = GridBuildingSystem.current.gridLayout.LocalToCell(transform.position);
        BoundsInt areaTemp = area;
        areaTemp.position = cellPos;

        if (GridBuildingSystem.current.CanPlaceBuilding(areaTemp))
        {
            return true;
        }

        return false;
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
    }

    #endregion
}
