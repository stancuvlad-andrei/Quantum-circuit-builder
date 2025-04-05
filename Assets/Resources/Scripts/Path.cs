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

    public void StartWave(Vector3Int sourcePosition, int state = -1)
    {
        if (!isAnimating)
        {
            StartCoroutine(WaveAnimation(sourcePosition, state));
        }
    }

    private IEnumerator WaveAnimation(Vector3Int sourcePosition, int state)
    {
        isAnimating = true;
        if (waveSprites.Length >= 2)
        {
            spriteRenderer.sprite = waveSprites[1];
            yield return new WaitForSeconds(waveDelay);

            // Get current position
            Vector3Int currentCell = GridBuildingSystem.current.gridLayout.WorldToCell(transform.position);

            // Calculate incoming direction (from source to current)
            Vector3Int incomingDir = sourcePosition - currentCell;
            incomingDir.Clamp(Vector3Int.one * -1, Vector3Int.one);

            // Directions to propagate (excluding incoming)
            Vector3Int[] directions = { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down };
            foreach (var dir in directions)
            {
                if (dir == incomingDir) continue;

                Vector3Int neighborPos = currentCell + dir;
                if (GridBuildingSystem.current.placedBuildings.TryGetValue(neighborPos, out Building neighbor))
                {
                    // Skip destroyed or null neighbors
                    if (neighbor == null || neighbor.gameObject == null) continue;

                    if (neighbor.TryGetComponent<Path>(out Path path))
                    {
                        path.StartWave(currentCell, state);
                    }
                    else if (neighbor.TryGetComponent<MeasuringGate>(out MeasuringGate gate))
                    {
                        gate.ReceiveMeasurement(state, currentCell);
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