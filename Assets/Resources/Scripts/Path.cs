using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Path : MonoBehaviour
{
    public Sprite[] waveSprites; // Array of sprites for wave animation
    public float waveDelay = 0.3f; // Delay between wave frames
    public SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    private Building building; // Reference to the Building component
    private List<Path> neighborPaths = new List<Path>(); // List of neighboring paths
    private bool isAnimating = false; // Flag to check if animation is in progress

    #region Unity Methods

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

    #endregion

    #region Path Methods

    public void FindNeighbors()
    {
        // If *this* Path or its Building has already been destroyed, do nothing.
        if (this == null || building == null)
        { 
            return; 
        }

        // Clear out any stale entries
        neighborPaths.Clear();

        var grid = GridBuildingSystem.current;
        if (grid == null) 
        { 
            return; 
        }

        var centerCell = grid.gridLayout.WorldToCell(transform.position);
        Vector3Int[] directions = { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down };

        foreach (var dir in directions)
        {
            var neighborPos = centerCell + dir;
            if (!grid.placedBuildings.TryGetValue(neighborPos, out Building neighbor) || neighbor == null)
            { 
                continue; 
            }

            // If the neighbor�s GameObject survived, get its Path
            Path neighborPath = neighbor.GetComponent<Path>();
            if (neighborPath != null && neighborPath != this)
            {
                neighborPaths.Add(neighborPath);
            }
        }
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

    #endregion

    #region Animation Methods

    public void StartWave(Vector3Int sourcePosition, float incomingProbability, bool isCollapsed)
    {
        if (isCollapsed)
        {
            StopAllCoroutines();
            isAnimating = false;
            StartCoroutine(WaveAnimation(sourcePosition, incomingProbability, isCollapsed));
        }
        else if (!isAnimating)
        {
            StartCoroutine(WaveAnimation(sourcePosition, incomingProbability, isCollapsed));
        }
    }

    private IEnumerator WaveAnimation(Vector3Int sourcePosition, float incomingProbability, bool isCollapsed)
    {
        while (SimulationManager.isPaused)
        {
            yield return null;
        }

        isAnimating = true;

        if (waveSprites.Length >= 2)
        {
            spriteRenderer.sprite = waveSprites[1];
            float delayTimer = 0f;
            while (delayTimer < waveDelay)
            {
                if (!SimulationManager.isPaused)
                {
                    delayTimer += Time.deltaTime;
                }
                yield return null;
            }

            Vector3Int currentCell = GridBuildingSystem.current.gridLayout.WorldToCell(transform.position);
            Vector3Int incomingDir = sourcePosition - currentCell;
            incomingDir.Clamp(Vector3Int.one * -1, Vector3Int.one);

            float modifiedProbability = incomingProbability;

            Vector3Int[] directions = { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down };

            foreach (var dir in directions)
            {
                while (SimulationManager.isPaused)
                {
                    yield return null;
                }

                if (dir == incomingDir)
                {
                    continue;
                }

                Vector3Int neighborPos = currentCell + dir;
                if (GridBuildingSystem.current.placedBuildings.TryGetValue(neighborPos, out Building neighbor))
                {
                    if (neighbor == null)
                    {
                        continue;
                    }

                    while (SimulationManager.isPaused)
                    {
                        yield return null;
                    }

                    // Path handling
                    if (neighbor.TryGetComponent<Path>(out Path path))
                    {
                        path.StartWave(currentCell, modifiedProbability, isCollapsed);
                        continue;
                    }

                    // MeasuringGate handling
                    else if (neighbor.TryGetComponent<MeasuringGate>(out MeasuringGate gate))
                    {
                        gate.ReceiveMeasurement(currentCell, modifiedProbability, isCollapsed);
                        continue;
                    }

                    // XGate handling
                    else if (neighbor.TryGetComponent<XGate>(out XGate xGate))
                    {
                        float gatedProbability = xGate.ApplyGate(modifiedProbability, currentCell);
                        bool newIsCollapsed = (gatedProbability == 0f || gatedProbability == 1f);
                        xGate.PropagateAfterGate(currentCell, gatedProbability, dir, newIsCollapsed);
                        Debug.Log($"PropagateAfterGate called with isCollapsed = {newIsCollapsed}");
                    }

                    // YGate handling
                    else if (neighbor.TryGetComponent<YGate>(out YGate yGate))
                    {
                        float gatedProbability = yGate.ApplyGate(modifiedProbability, currentCell);
                        bool newIsCollapsed = (gatedProbability == 0f || gatedProbability == 1f);
                        yGate.PropagateAfterGate(currentCell, gatedProbability, dir, newIsCollapsed);
                        Debug.Log($"PropagateAfterGate called with isCollapsed = {newIsCollapsed}");
                    }

                    // ZGate handling
                    else if (neighbor.TryGetComponent<ZGate>(out ZGate zGate))
                    {
                        float gatedProbability = zGate.ApplyGate(modifiedProbability, currentCell);
                        bool newIsCollapsed = (gatedProbability == 0f || gatedProbability == 1f);
                        zGate.PropagateAfterGate(currentCell, gatedProbability, dir, newIsCollapsed);
                        Debug.Log($"PropagateAfterGate called with isCollapsed = {newIsCollapsed}");
                    }

                    // HGate handling
                    else if (neighbor.TryGetComponent<HGate>(out HGate hGate))
                    {
                        float gatedProbability = hGate.ApplyGate(modifiedProbability, currentCell);
                        bool newIsCollapsed = false; // H gates always produce uncollapsed states
                        hGate.PropagateAfterGate(currentCell, gatedProbability, dir, newIsCollapsed);
                        Debug.Log($"PropagateAfterGate called with isCollapsed = {newIsCollapsed}");
                    }

                    // SGate handling
                    else if (neighbor.TryGetComponent<SGate>(out SGate sGate))
                    {
                        float gatedProbability = sGate.ApplyGate(modifiedProbability, currentCell);
                        bool newIsCollapsed = (gatedProbability == 0f || gatedProbability == 1f);
                        sGate.PropagateAfterGate(currentCell, gatedProbability, dir, newIsCollapsed);
                        Debug.Log($"PropagateAfterGate called with isCollapsed = {newIsCollapsed}");
                    }

                    // TGate handling
                    else if (neighbor.TryGetComponent<TGate>(out TGate tGate))
                    {
                        float gatedProbability = tGate.ApplyGate(modifiedProbability, currentCell);
                        bool newIsCollapsed = (gatedProbability == 0f || gatedProbability == 1f);
                        tGate.PropagateAfterGate(currentCell, gatedProbability, dir, newIsCollapsed);
                        Debug.Log($"PropagateAfterGate called with isCollapsed = {newIsCollapsed}");
                    }

                    // CXGate handling
                    else if (neighbor.TryGetComponent<CXGate>(out CXGate cxGate))
                    {
                        float gatedProbability = cxGate.ApplyGate(modifiedProbability, currentCell);
                        bool newIsCollapsed = (gatedProbability == 0f || gatedProbability == 1f);
                        Debug.Log($"PropagateAfterGate called with isCollapsed = {newIsCollapsed}");
                    }

                    // CZGate handling
                    else if (neighbor.TryGetComponent<CZGate>(out CZGate czGate))
                    {
                        float gatedProbability = czGate.ApplyGate(modifiedProbability, currentCell);
                        bool newIsCollapsed = (gatedProbability == 0f || gatedProbability == 1f);
                        Debug.Log($"PropagateAfterGate called with isCollapsed = {newIsCollapsed}");
                    }

                    // SPGate handling
                    else if (neighbor.TryGetComponent<SPGate>(out SPGate spGate))
                    {
                        float gatedProbability = spGate.ApplyGate(modifiedProbability, currentCell);
                        bool newIsCollapsed = (gatedProbability == 0f || gatedProbability == 1f);
                        Debug.Log($"PropagateAfterGate called with isCollapsed = {newIsCollapsed}");
                    }
                }
            }
            spriteRenderer.sprite = waveSprites[0];
            isAnimating = false;
        }
    }

    #endregion

}