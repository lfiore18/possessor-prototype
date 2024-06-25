using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinding : MonoBehaviour
{
    [SerializeField]
    Tilemap tilemap;

    public Vector3 halfCellSize; 

    [SerializeField]
    GameObject startObj;
    [SerializeField]
    GameObject targetObj;

    Vector3Int startPos;
    Vector3Int targetPos;

    [Header("Debug Properties")]
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
        halfCellSize = tilemap.cellSize / 2;

        Debug.Log("tilemap Min X,Y: " + tilemap.cellBounds.xMin + ", " + tilemap.cellBounds.yMin);
        Debug.Log("tilemap Max X,Y: " + tilemap.cellBounds.xMax + ", " + tilemap.cellBounds.yMax);


        SetDebugPositions();
        BreadthFirstSearch();
    }

    private void Update()
    {
        SetDebugPositions();
        BreadthFirstSearch();
    }

    public List<Vector3Int> CalculatePath(Vector3 targetPos)
    {
        // Trace a path back from the target to the starting position
        List<Vector3Int> path = new List<Vector3Int>();
        
        Vector3Int lastCell = cameFrom[tilemap.WorldToCell(targetPos)];

        path.Add(lastCell);
        while(lastCell != startPos)
        {
            lastCell = cameFrom[lastCell];
            path.Add(lastCell);
        }

        return path;
    }

    // TODO: Currently still does not handle diagonals with equal weight,
    // consider implementing Djikstras algorithm instead
    void BreadthFirstSearch()
    {
        // Clear incase any data is left over from previous search
        searchPerimeter.Clear();
        cameFrom.Clear();

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
            searchPerimeter.RemoveAt(0);

            foundTarget = CheckNeighbourCells(currentTile);
            count = turnOnDebugLoopLimit ? count - 1 : searchPerimeter.Count;
        }
    }

    bool CheckNeighbourCells(Vector3Int cell)
    {
        int posX = cell.x;
        int posY = cell.y;

        Debug.Log("Checking: " + posX + ", " + posY);

        // Prioritise checking straight paths before diagonals, y in the first slow of the array, x in second
        int[,] positions =
        {
            { 0, 1 }, // right
            { 0, -1 }, // left
            { 1, 0 }, // up
            { -1, 0 }, // down
            
            { 1, 1 }, // top-right
            { -1, -1}, // top-left
            { -1, 1 }, // bottom-right
            { -1, -1} // bottom-left
        };


        List<Vector3Int> neighbours = GetValidNeighbours(cell);


        bool topRightBlocked = false;
        bool topLeftBlocked = false;
        bool bottomLeftBlocked = false;
        bool bottomRightBlocked = false;

        for (int i = 0; i < positions.GetLength(0); i++)
        {
            // Add the co-ordinates from the position array to co-ordinates of the cell we're currently checking
            int newX = cell.x + positions[i, 1];
            int newY = cell.y + positions[i, 0];

            Vector3Int newPos = new Vector3Int(newX, newY, 0);
            if (tilemap.GetTile(newPos) == null) { continue; }


            if (TileIsWalkable(newPos) && !cameFrom.ContainsKey(newPos))
            {
                searchPerimeter.Add(newPos);
                cameFrom.Add(newPos, cell);
            }
            
            // if (newPos == targetPos) return true;
        }
        return false;
    }

    List<Vector3Int> GetValidNeighbours(Vector3Int cell)
    {
        List<Vector3Int> neighbours = new List<Vector3Int>();

        int y = cell.y;
        int x = cell.x;

        int tMinX = tilemap.cellBounds.xMin;
        int tMaxX = tilemap.cellBounds.xMax;

        int tMinY = tilemap.cellBounds.yMin;
        int tMaxY = tilemap.cellBounds.yMax;
/*
        int right = x + 1 >= tMaxX ? x : x + 1;
        int left = x - 1 < tMinX ? x : x - 1;
        int up = y + 1 >= tMaxY ? y : y + 1;
        int down = y - 1 < tMinY ? y : y - 1;*/

        bool topRightBlocked = false;
        bool topLeftBlocked = false;
        bool bottomLeftBlocked = false;
        bool bottomRightBlocked = false;

        // Straight directions

        // Check cell right is within bounds
        if (x + 1 < tMaxX)
        {
            neighbours.Add(new Vector3Int(x + 1, y, 0));
        }
        else
        {
            topRightBlocked = true;
            bottomRightBlocked = true;
        }

        // Check cell left is within bounds
        if (x - 1 >= tMinX)
        {
            neighbours.Add(new Vector3Int(x - 1, y, 0));
        } 
        else
        {
            topLeftBlocked = true;
            bottomLeftBlocked = true;
        }

        // Check cell above is within bounds
        if (y + 1 < tMaxY)
        {
            neighbours.Add(new Vector3Int(x, y + 1, 0));
        }
        else
        {
            topLeftBlocked = true;
            topRightBlocked = true;
        }

        // Check cell below is within bounds
        if (y - 1 >= tMinY)
        {
            neighbours.Add(new Vector3Int(x, y - 1, 0));
        }
        else
        {
            bottomRightBlocked = true;
            bottomLeftBlocked = true;
        }

        // Diagonal directions - only add under the condition that they are within bounds
        if (!topRightBlocked) neighbours.Add(new Vector3Int(x + 1, y + 1, 0));
        if (!topLeftBlocked) neighbours.Add(new Vector3Int(x - 1, y + 1, 0));
        if (!bottomRightBlocked) neighbours.Add(new Vector3Int(x + 1, y - 1, 0));
        if (!bottomLeftBlocked) neighbours.Add(new Vector3Int(x - 1, y - 1, 0));

        return neighbours;
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
        if (Application.isPlaying)
        {

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

            if (cameFrom != null)
            {
                // Draw debug overlay for all cells visited during breadth-first search
                foreach (KeyValuePair<Vector3Int, Vector3Int> entry in cameFrom)
                {
                    Gizmos.color = new Color(0, 1, 1, 0.2f);
                    Gizmos.DrawCube(entry.Key + halfCellSize, tilemap.cellSize);
                }
            }
        }
    }
}
