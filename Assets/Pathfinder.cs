using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinder : MonoBehaviour
{
    public Tilemap tilemap;
    public TilemapGameLevel gameLevel;

    public Color visitedColor = Color.gray;
    public Color pathColor = Color.green;
    public float iterationDelay = 0.1f;

    private Vector3Int? startTile = null;
    private Vector3Int? goalTile = null;

    private Dictionary<Vector3Int, DijkstraNodeData> nodeData = new();
    private HashSet<Vector3Int> visited = new();
    private HashSet<Vector3Int> unvisited = new();

    private bool isSolving = false;
    private List<Vector3Int> finalPath = new();

    [System.Serializable]
    public struct DijkstraNodeData
    {
        public float gCost;
        public Vector3Int previous;
    }

    void Update()
    {
        if (isSolving) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector3Int tilePos = tilemap.WorldToCell(mouseWorld);

        if (Input.GetMouseButtonDown(1))  // Right click
        {
            startTile = tilePos;
            Debug.Log("Start set to " + tilePos);
        }
        else if (Input.GetMouseButtonDown(0))  // Left click
        {
            goalTile = tilePos;
            Debug.Log("Goal set to " + tilePos);
        }

        if (Input.GetKeyDown(KeyCode.Space) && startTile.HasValue && goalTile.HasValue)
        {
            StartCoroutine(DijkstraSolve());
        }
    }

    IEnumerator DijkstraSolve()
    {
        isSolving = true;
        nodeData.Clear();
        visited.Clear();
        unvisited.Clear();
        finalPath.Clear();

        Vector3Int start = startTile.Value;
        Vector3Int goal = goalTile.Value;

        nodeData[start] = new DijkstraNodeData { gCost = 0f };
        unvisited.Add(start);

        while (unvisited.Count > 0)
        {
            Vector3Int current = GetLowestCostNode(unvisited);
            if (current == goal)
                break;

            unvisited.Remove(current);
            visited.Add(current);

            float currentCost = nodeData[current].gCost;

            foreach (var neighbor in gameLevel.GetNeighbours(current))
            {
                if (visited.Contains(neighbor)) continue;

                float tentativeCost = currentCost + 1; // assume tile cost = 1

                if (!nodeData.ContainsKey(neighbor) || tentativeCost < nodeData[neighbor].gCost)
                {
                    nodeData[neighbor] = new DijkstraNodeData
                    {
                        gCost = tentativeCost,
                        previous = current
                    };
                    unvisited.Add(neighbor);
                }
            }

            yield return new WaitForSeconds(iterationDelay);
        }

        if (nodeData.ContainsKey(goal))
        {
            Vector3Int current = goal;
            while (current != start)
            {
                finalPath.Insert(0, current);
                current = nodeData[current].previous;
            }
            finalPath.Insert(0, start);

            FindObjectOfType<CharacterMover>()?.SetPath(finalPath);
        }


        isSolving = false;
    }

    Vector3Int GetLowestCostNode(HashSet<Vector3Int> nodeSet)
    {
        float minCost = float.MaxValue;
        Vector3Int bestNode = default;

        foreach (var node in nodeSet)
        {
            if (nodeData.ContainsKey(node) && nodeData[node].gCost < minCost)
            {
                minCost = nodeData[node].gCost;
                bestNode = node;
            }
        }

        return bestNode;
    }

    void OnDrawGizmos()
    {
        if (!tilemap) return;

        foreach (var node in visited)
        {
            Gizmos.color = visitedColor;
            Gizmos.DrawSphere(tilemap.GetCellCenterWorld(node), 0.1f);
        }

        Gizmos.color = pathColor;
        for (int i = 0; i < finalPath.Count - 1; i++)
        {
            Vector3 from = tilemap.GetCellCenterWorld(finalPath[i]);
            Vector3 to = tilemap.GetCellCenterWorld(finalPath[i + 1]);
            Gizmos.DrawLine(from, to);
        }
    }



}
