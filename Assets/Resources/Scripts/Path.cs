using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Path : MonoBehaviour
{
    public Sprite[] waveSprites;
    public float waveDelay = 0.3f;
    public SpriteRenderer spriteRenderer;
    private Building building;
    private List<Path> neighborPaths = new List<Path>();
    private bool isAnimating = false;

    private void Awake()
    {
        building = GetComponent<Building>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (building != null && building.placed)
        {
            FindNeighbors();
        }
    }

    public void FindNeighbors()
    {
        neighborPaths.Clear();
        if (building == null) return;

        Vector3Int centerCell = GridBuildingSystem.current.gridLayout.WorldToCell(transform.position);
        Vector3Int[] directions = { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down };

        foreach (var dir in directions)
        {
            Vector3Int neighborPos = centerCell + dir;
            if (GridBuildingSystem.current.placedBuildings.TryGetValue(neighborPos, out Building neighbor))
            {
                if (neighbor.TryGetComponent<Path>(out Path path) && path != this)
                {
                    neighborPaths.Add(path);
                }
            }
        }
    }

    public void StartWave(Path source = null, int state = -1)
    {
        if (!isAnimating)
        {
            StartCoroutine(WaveAnimation(source, state));
        }
    }

    private IEnumerator WaveAnimation(Path source, int state)
    {
        isAnimating = true;
        if (waveSprites.Length >= 2)
        {
            spriteRenderer.sprite = waveSprites[1];
            yield return new WaitForSeconds(waveDelay);

            // Propagate to neighbors
            foreach (Path neighbor in neighborPaths)
            {
                if (neighbor != source)
                {
                    neighbor.StartWave(this, state);
                }
            }

            // Notify adjacent components
            Vector3Int centerCell = GridBuildingSystem.current.gridLayout.WorldToCell(transform.position);
            Vector3Int[] directions = { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down };

            foreach (var dir in directions)
            {
                Vector3Int neighborPos = centerCell + dir;
                if (GridBuildingSystem.current.placedBuildings.TryGetValue(neighborPos, out Building neighborBuilding))
                {
                    // Handle Measuring Gates
                    MeasuringGate mg = neighborBuilding.GetComponent<MeasuringGate>();
                    if (mg != null && state != -1) // Only send valid states
                    {
                        mg.ReceiveMeasurement(state);
                    }
                }
            }

            yield return new WaitForSeconds(0.1f);
            spriteRenderer.sprite = waveSprites[0];
        }
        isAnimating = false;
    }

    public void ResetPath()
    {
        StopAllCoroutines();
        isAnimating = false;
        if (waveSprites.Length > 0)
        {
            spriteRenderer.sprite = waveSprites[0];
        }
    }
}