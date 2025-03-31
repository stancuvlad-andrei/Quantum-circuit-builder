using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Path : MonoBehaviour
{
    public Sprite[] waveSprites; // Assign in Inspector: [default, active]
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
                if (neighbor.TryGetComponent<Path>(out Path path) && path != this) // Exclude self
                {
                    neighborPaths.Add(path);
                }
            }
        }
    }

    public void StartWave(Path source = null)
    {
        if (!isAnimating)
        {
            StartCoroutine(WaveAnimation(source));
        }
    }

    private IEnumerator WaveAnimation(Path source)
    {
        isAnimating = true;
        if (waveSprites.Length >= 2)
        {
            spriteRenderer.sprite = waveSprites[1]; // Activate
            yield return new WaitForSeconds(waveDelay);

            // Propagate to neighbors EXCEPT the source
            foreach (Path neighbor in neighborPaths)
            {
                if (neighbor != source) // Skip the path that triggered this wave
                {
                    neighbor.StartWave(this); // Pass "this" as the source
                }
            }

            yield return new WaitForSeconds(0.1f);
            spriteRenderer.sprite = waveSprites[0]; // Reset
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