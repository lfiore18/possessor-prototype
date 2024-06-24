using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Terrain Tile", menuName = "Tiles/Terrain Tile")]
public class TerrainTile : Tile
{
    public bool isWalkable = true;

}
