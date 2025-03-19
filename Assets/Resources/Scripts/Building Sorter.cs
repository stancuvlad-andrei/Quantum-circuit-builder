using System.Collections.Generic;
using UnityEngine;

public class BuildingSorter : MonoBehaviour
{
    public static BuildingSorter Instance { get; private set; }

    [SerializeField] private int _baseSortingOrder = 0;
    [SerializeField] private int _sortingOrderOffset = 10;

    public int BaseSortingOrder => _baseSortingOrder;
    public int SortingOrderOffset => _sortingOrderOffset;

    private List<Building> buildings = new List<Building>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterBuilding(Building building)
    {
        if (!buildings.Contains(building))
        {
            buildings.Add(building);
            UpdateAllSortingOrders();
        }
    }

    public void UnregisterBuilding(Building building)
    {
        if (buildings.Contains(building))
        {
            buildings.Remove(building);
            UpdateAllSortingOrders();
        }
    }

    private void UpdateAllSortingOrders()
    {
        buildings.Sort((a, b) =>
        {
            int yCompare = b.transform.position.y.CompareTo(a.transform.position.y);
            return yCompare != 0 ? yCompare : a.transform.position.x.CompareTo(b.transform.position.x);
        });

        for (int i = 0; i < buildings.Count; i++)
        {
            buildings[i].UpdateSortingOrder(_baseSortingOrder + i * _sortingOrderOffset);
        }
    }
}