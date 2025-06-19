using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TilemapGameLevel : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase floorTile;
    public int width = 10;
    public int height = 10;
    [Range(0f, 1f)]
    public float floorSpawnThreshold = 0.75f;

    public void GenerateMap()
    {
        tilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                float rand = Random.value;
                if (rand < floorSpawnThreshold)
                {
                    tilemap.SetTile(pos, floorTile);
                }
            }
        }
    }

    public List<Vector3Int> GetNeighbours(Vector3Int currentTilePos)
    {
        List<Vector3Int> neighbours = new List<Vector3Int>();

        Vector3Int[] directions = {
            new Vector3Int(0, 1, 0),   // North
            new Vector3Int(0, -1, 0),  // South
            new Vector3Int(1, 0, 0),   // East
            new Vector3Int(-1, 0, 0)   // West
        };

        foreach (Vector3Int dir in directions)
        {
            Vector3Int neighbourPos = currentTilePos + dir;

            if (tilemap.GetTile(neighbourPos) == floorTile)
            {
                neighbours.Add(neighbourPos);
            }
        }

        return neighbours;
        void OnDrawGizmos()
        {
            if (tilemap == null || floorTile == null) return;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);

                    if (tilemap.GetTile(pos) == floorTile)
                    {
                        Vector3 center = tilemap.GetCellCenterWorld(pos);

                        var neighbours = GetNeighbours(pos);

                        foreach (var nPos in neighbours)
                        {
                            Vector3 nCenter = tilemap.GetCellCenterWorld(nPos);

                            Gizmos.color = Color.green;
                            Gizmos.DrawLine(center, nCenter);
                        }

                        Gizmos.color = Color.yellow;
                        Gizmos.DrawSphere(center, 0.1f);
                    }
                }
            }
        }
    }
}
    
