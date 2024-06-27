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

    Vector3Int startPos;

    [Header("Debug Properties")]
    [SerializeField]
    bool drawStartPos;
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

        // Set the initial positions of starting and target points of the breadth first search
        // Currently, not using "target" points, just scanning the entire tilemap and returning a distance field
        SetStartPosition();
        BreadthFirstSearch();
    }

    private void Update()
    {
        if (PlayerPositionHasChanged())
        {
            SetStartPosition();
            BreadthFirstSearch();
        }
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

        // NOTE: Currently unused, we do not end early, we create a distance field of the tilemap
        bool foundTarget = false;

        // For testing/debug purposes, can limit the loop count here
        // otherwise, BFS will run until it's exhausted the search perimeter
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

        //Debug.Log("Checking: " + posX + ", " + posY);

        List<Vector3Int> neighbours = GetValidNeighbours(cell);


        foreach(Vector3Int neighbour in neighbours)
        {
            if (!cameFrom.ContainsKey(neighbour))
            {
                searchPerimeter.Add(neighbour);
                cameFrom.Add(neighbour, cell);
            }
            
            // Line below would end the breadthfirst search early
            // if (newPos == targetPos) return true;
        }
        return false;
    }

    List<Vector3Int> GetValidNeighbours(Vector3Int cell)
    {
        List<Vector3Int> neighbours = new List<Vector3Int>();

        int y = cell.y;
        int x = cell.x;

        bool topRightBlocked = false;
        bool topLeftBlocked = false;
        bool bottomLeftBlocked = false;
        bool bottomRightBlocked = false;


        Vector3Int newPos = new Vector3Int(x + 1, y, 0);
        // Straight directions

        // Check cell right is within bounds
        if (TileIsWalkable(newPos))
        {
            neighbours.Add(newPos);
        }
        else
        {
            topRightBlocked = true;
            bottomRightBlocked = true;
        }

        newPos.x = x - 1;
        // Check cell left is within bounds
        if (TileIsWalkable(newPos))
        {
            neighbours.Add(newPos);
        } 
        else
        {
            topLeftBlocked = true;
            bottomLeftBlocked = true;
        }

        newPos.x = x;
        newPos.y = y + 1;
        // Check cell above is within bounds
        if (TileIsWalkable(newPos))
        {
            neighbours.Add(newPos);
        }
        else
        {
            topLeftBlocked = true;
            topRightBlocked = true;
        }

        newPos.y = y - 1;
        // Check cell below is within bounds
        if (TileIsWalkable(newPos))
        {
            neighbours.Add(newPos);
        }
        else
        {
            bottomRightBlocked = true;
            bottomLeftBlocked = true;
        }

        // Diagonal directions - only add under the condition that they are within bounds
        newPos.x = x + 1;
        newPos.y = y + 1;
        if (!topRightBlocked && TileIsWalkable(newPos)) neighbours.Add(newPos);

        newPos.x = x - 1;
        newPos.y = y + 1;
        if (!topLeftBlocked && TileIsWalkable(newPos)) neighbours.Add(newPos);

        newPos.x = x + 1;
        newPos.y = y - 1;
        if (!bottomRightBlocked && TileIsWalkable(newPos)) neighbours.Add(newPos);

        newPos.x = x - 1;
        newPos.y = y - 1;
        if (!bottomLeftBlocked && TileIsWalkable(newPos)) neighbours.Add(newPos);

        return neighbours;
    }

    bool TileIsWalkable(Vector3Int position)
    {
        int tMinX = tilemap.cellBounds.xMin;
        int tMaxX = tilemap.cellBounds.xMax;

        int tMinY = tilemap.cellBounds.yMin;
        int tMaxY = tilemap.cellBounds.yMax;

        if (position.x + 1 >= tMaxX) return false;
        if (position.x - 1 < tMinX) return false;
        if (position.y + 1 >= tMaxY) return false;
        if (position.y - 1 < tMinY) return false;

        TileBase tile = tilemap.GetTile(position);
        return tile is TerrainTile terrainTile && terrainTile.isWalkable;
    }

    void SetDebugPositions()
    {
        SetStartPosition();
    }

    void SetStartPosition()
    {
        if (startObj != null)
        {
            startPos = tilemap.WorldToCell(startObj.transform.position);
            if (!dontDestroyDebugObjsOnStart) Destroy(startObj);
        }
        else
        {
            Debug.LogError("startObj is null! No start position set!");
        }
    }

    public bool PlayerPositionHasChanged()
    {
        Vector3Int playerCurrentPos = tilemap.WorldToCell(startObj.transform.position);

        bool positionHasChanged = startPos != playerCurrentPos;        

        if (positionHasChanged)
        {
            Debug.Log("Player position has Changed");
        }

        return positionHasChanged;   
    }

    public Vector3Int GetCellPosition(Vector3 position)
    {
        return tilemap.WorldToCell(position);
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

            /*foreach (Vector3Int cell in searchPerimeter)
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
