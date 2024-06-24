using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinding : MonoBehaviour
{
    [SerializeField]
    Tilemap tilemap;

    [SerializeField]
    GameObject player;

    Vector3Int playerCellPos;

    [SerializeField]
    GameObject startObj;
    [SerializeField]
    GameObject targetObj;

    Vector3Int startPos;
    Vector3Int targetPos;


    [Header("Debug Properties")]
    [SerializeField]
    bool drawPlayerPos;
    [SerializeField]
    bool drawStartPos;
    [SerializeField]
    bool drawTargetPos;
    [SerializeField]
    bool dontDestroyDebugObjsOnStart; // incase I'm using enemies or player obj to test pathfinding

    [SerializeField]
    bool turnOnDebugLoopLimit;
    [SerializeField]
    int limitLoopCount = 0;


    // Data structures for tracking searched and visited tiles
    List<Vector3Int> searchPerimeter = new List<Vector3Int>();
    Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

    private void Start()
    {
        tilemap = GetComponent<Tilemap>();

        Debug.Log("tilemap Min X,Y: " + tilemap.cellBounds.xMin + ", " + tilemap.cellBounds.yMin);
        Debug.Log("tilemap Max X,Y: " + tilemap.cellBounds.xMax + ", " + tilemap.cellBounds.yMax);

        if (player == null)
            player = GameObject.Find("Player");


        //playerCellPos = tilemap.WorldToCell(player.transform.position);

        SetDebugPositions();
        BreadthFirstSearch();

    }

    private void Update()
    {
        //playerCellPos = tilemap.WorldToCell(player.transform.position);

    }

    List<Vector3Int> CalculatePath()
    {
        // Trace a path back from the target to the starting position
        List<Vector3Int> path = new List<Vector3Int>();

        Vector3Int lastCell = cameFrom[targetPos];

        path.Add(lastCell);
        while(lastCell != startPos)
        {
            lastCell = cameFrom[lastCell];
            path.Add(lastCell);
        }

        return path;
    }


    void BreadthFirstSearch()
    {
        Vector3Int startTilePos = startPos;

        // Start tile is the first tile to search
        searchPerimeter.Add(startTilePos);

        // Set the starting tile's "came from" position to itself for now
        cameFrom.Add(startTilePos, startTilePos);

        bool foundTarget = false;

        int count = turnOnDebugLoopLimit ? limitLoopCount : searchPerimeter.Count; 
        while (count > 0 && !foundTarget)
        {
            Vector3Int currentTile = searchPerimeter[0];
            searchPerimeter.Remove(currentTile);

            foundTarget = CheckNeighbourCells(currentTile);
            count = turnOnDebugLoopLimit ? count - 1 : searchPerimeter.Count;
        }
    }

    bool CheckNeighbourCells(Vector3Int cell)
    {
        int posX = cell.x;
        int posY = cell.y;

        Debug.Log("Checking: " + posX + ", " + posY);

        // X boundaries of tilemap
        int tmBoundsMinX = tilemap.cellBounds.xMin;
        int tmBoundsMaxX = tilemap.cellBounds.xMax;

        // Y boundaries of tilemap
        int tmBoundsMinY = tilemap.cellBounds.yMin;
        int tmBoundsMaxY = tilemap.cellBounds.yMax;

        // Check cells to the right, left, up or down as long as they're within the boundaries of the map
        int startCheckX = posX - 1 < tmBoundsMinX ? posX : posX - 1;
        int endCheckX = posX + 1 >= tmBoundsMaxX ? posX : posX + 1;
        int startCheckY = posY - 1 < tmBoundsMinY ? posY : posY - 1;
        int endCheckY = posY + 1 >= tmBoundsMaxY ? posY : posY + 1;

        // For now, y and x are evaluated seperately to avoid diagonals paths
        // Breadth-first treats cells diagonal to the cell currently being checked
        // as equally viable when plotting the shortest path from one cell to another
        // leading to awkward/illogical path

        for (int y = startCheckY; y <= endCheckY; y++)
        {
            // Skip the cell whose neighbours we're checking
            if (y == cell.y && posX == cell.x) { continue; }

            Vector3Int newPos = new Vector3Int(posX, y, 0);
            if (TileIsWalkable(newPos) && !cameFrom.ContainsKey(newPos))
            {
                searchPerimeter.Add(newPos);
                cameFrom.Add(newPos, cell);
            }
        }

        for (int x = startCheckX; x <= endCheckX; x++)
        {
            // Skip the cell whose neighbours we're checking
            if (posY == cell.y && x == cell.x) { continue; }

            Vector3Int newPos = new Vector3Int(x, posY, 0);
            if (TileIsWalkable(newPos) && !cameFrom.ContainsKey(newPos))
            {
                searchPerimeter.Add(newPos);
                cameFrom.Add(newPos, cell);
            }
        }
        return false;
    }

    bool TileIsWalkable(Vector3Int position)
    {
        TileBase tile = tilemap.GetTile(position);
        return tile is TerrainTile terrainTile && terrainTile.isWalkable;
    }

    void SetDebugPositions()
    {
        if (startObj != null)
        {
            startPos = tilemap.WorldToCell(startObj.transform.position);
            if (!dontDestroyDebugObjsOnStart) Destroy(startObj);
        }
        else
        {
            Debug.LogError("targetObj is null! No target position set!");
        }

        if (targetObj != null)
        {
            targetPos = tilemap.WorldToCell(targetObj.transform.position);
            if (!dontDestroyDebugObjsOnStart) Destroy(targetObj);
        }
        else
        {
            Debug.LogError("targetObj is null! No target position set!");
        }
    }


    private void OnDrawGizmos()
    {
        var halfCellSize = tilemap.cellSize / 2;

        if (startObj != null)
        {
            startPos = tilemap.WorldToCell(startObj.transform.position);         
        }

        if (targetObj != null)
        {
            targetPos = tilemap.WorldToCell(targetObj.transform.position);
        }

        if (Application.isPlaying)
        {
            if (drawPlayerPos)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(playerCellPos + halfCellSize, tilemap.cellSize);
            }

            if (drawStartPos)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(startPos + halfCellSize, tilemap.cellSize);
            }

            if (drawTargetPos)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(targetPos + halfCellSize, tilemap.cellSize);
            }

/*            foreach(Vector3Int cell in searchPerimeter)
            {
                Gizmos.color = new Color(0, 1, 1, 0.4f);
                Gizmos.DrawCube(cell + halfCellSize, tilemap.cellSize);
            }*/

            // Draw debug overlay for all cells visited during breadth-first search
            foreach (KeyValuePair<Vector3Int, Vector3Int> entry in cameFrom)
            {
                Gizmos.color = new Color(0, 1, 1, 0.2f);
                Gizmos.DrawCube(entry.Key + halfCellSize, tilemap.cellSize);
            }

            List<Vector3Int> path = CalculatePath();
            foreach (Vector3Int position in path)
            {
                Gizmos.color = new Color(1, 0.6f, 1, 0.4f);
                Gizmos.DrawCube(position + halfCellSize, tilemap.cellSize);
            }
        }
    }
}
