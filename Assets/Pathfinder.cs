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

    // Oyun içi cost gösterimi için:
    public GameObject tileCostTextPrefab;
    private Dictionary<Vector3Int, GameObject> costTexts = new();

    private Vector3Int? startTile = null;
    private Vector3Int? goalTile = null;

    private Dictionary<Vector3Int, DijkstraNodeData> nodeData = new();
    private HashSet<Vector3Int> visited = new();
    private HashSet<Vector3Int> unvisited = new();

    private bool isSolving = false;
    private List<Vector3Int> finalPath = new();

    // Step-through variables
    private bool stepMode = false;
    private bool stepReady = false;
    private bool waitForMove = false;

    [System.Serializable]
    public struct DijkstraNodeData
    {
        public float gCost;
        public Vector3Int previous;
    }

    void Update()
    {
        // Çözüm sırasında step-through kontrolü
        if (isSolving)
        {
            // Step-through modunda Space'e basınca bir adım ilerle
            if (stepMode && Input.GetKeyDown(KeyCode.Space))
            {
                stepReady = true;
            }
            // Path hazırsa Enter ile Monster'ı yürüt
            if (waitForMove && Input.GetKeyDown(KeyCode.Return))
            {
                Object.FindFirstObjectByType<CharacterMover>()?.SetPath(finalPath);
                waitForMove = false;
            }
            return;
        }

        // Start ve Goal seçimi (mouse)
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector3Int tilePos = tilemap.WorldToCell(mouseWorld);

        if (Input.GetMouseButtonDown(1))  // Sağ tık: Start tile seç
        {
            startTile = tilePos;
            Debug.Log("Start set to " + tilePos);
        }
        else if (Input.GetMouseButtonDown(0))  // Sol tık: Goal tile seç
        {
            goalTile = tilePos;
            Debug.Log("Goal set to " + tilePos);
        }

        // Step-through başlat (Space)
        if (Input.GetKeyDown(KeyCode.Space) && startTile.HasValue && goalTile.HasValue)
        {
            stepMode = true;
            StartCoroutine(DijkstraStepSolve());
        }

        // Otomatik hızlı çözüm başlat (F)
        if (Input.GetKeyDown(KeyCode.F) && startTile.HasValue && goalTile.HasValue)
        {
            stepMode = false;
            StartCoroutine(DijkstraSolve());
        }
    }

    // Step-through coroutine
    IEnumerator DijkstraStepSolve()
    {
        isSolving = true;
        nodeData.Clear();
        visited.Clear();
        unvisited.Clear();
        finalPath.Clear();
        ClearCostTexts();

        Vector3Int start = startTile.Value;
        Vector3Int goal = goalTile.Value;

        nodeData[start] = new DijkstraNodeData { gCost = 0f };
        unvisited.Add(start);

        ShowTileCost(start, 0f);

        while (unvisited.Count > 0)
        {
            // Space'e basılana kadar bekle
            stepReady = false;
            while (!stepReady)
                yield return null;

            Vector3Int current = GetLowestCostNode(unvisited);
            if (current == goal)
                break;

            unvisited.Remove(current);
            visited.Add(current);

            float currentCost = nodeData[current].gCost;

            foreach (var neighbor in gameLevel.GetNeighbours(current))
            {
                if (visited.Contains(neighbor)) continue;

                float tentativeCost = currentCost + 1; // Tile cost = 1

                if (!nodeData.ContainsKey(neighbor) || tentativeCost < nodeData[neighbor].gCost)
                {
                    nodeData[neighbor] = new DijkstraNodeData
                    {
                        gCost = tentativeCost,
                        previous = current
                    };
                    unvisited.Add(neighbor);

                    // Oyun içinde cost'u göster
                    ShowTileCost(neighbor, tentativeCost);
                }
            }
        }

        // Path bulunduysa, Enter ile yürüt
        if (nodeData.ContainsKey(goal))
        {
            Vector3Int current = goal;
            while (current != start)
            {
                finalPath.Insert(0, current);
                current = nodeData[current].previous;
            }
            finalPath.Insert(0, start);

            waitForMove = true;
            Debug.Log("Path ready! Press ENTER to move the Monster!");
        }
        else
        {
            Debug.Log("No path found!");
            waitForMove = false;
        }

        isSolving = false;
    }

    // Otomatik çözüm (isteğe bağlı)
    IEnumerator DijkstraSolve()
    {
        isSolving = true;
        nodeData.Clear();
        visited.Clear();
        unvisited.Clear();
        finalPath.Clear();
        ClearCostTexts();

        Vector3Int start = startTile.Value;
        Vector3Int goal = goalTile.Value;

        nodeData[start] = new DijkstraNodeData { gCost = 0f };
        unvisited.Add(start);

        ShowTileCost(start, 0f);

        while (unvisited.Count > 0)
        {
            yield return new WaitForSeconds(iterationDelay);

            Vector3Int current = GetLowestCostNode(unvisited);
            if (current == goal)
                break;

            unvisited.Remove(current);
            visited.Add(current);

            float currentCost = nodeData[current].gCost;

            foreach (var neighbor in gameLevel.GetNeighbours(current))
            {
                if (visited.Contains(neighbor)) continue;

                float tentativeCost = currentCost + 1;

                if (!nodeData.ContainsKey(neighbor) || tentativeCost < nodeData[neighbor].gCost)
                {
                    nodeData[neighbor] = new DijkstraNodeData
                    {
                        gCost = tentativeCost,
                        previous = current
                    };
                    unvisited.Add(neighbor);

                    ShowTileCost(neighbor, tentativeCost);
                }
            }
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

            Object.FindFirstObjectByType<CharacterMover>()?.SetPath(finalPath);
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

    void ShowTileCost(Vector3Int pos, float cost)
    {
        if (tileCostTextPrefab == null) return;

        // Zaten varsa güncelle
        if (costTexts.ContainsKey(pos))
        {
            var tmp = costTexts[pos].GetComponent<TMPro.TextMeshPro>();
            tmp.text = cost.ToString(); // Sayıyı yaz!
            return;
        }

        // Yeni oluşturuluyorsa:
        var go = Instantiate(tileCostTextPrefab, tilemap.GetCellCenterWorld(pos) + new Vector3(0, 0.2f, 0), Quaternion.identity);
        var tmpComp = go.GetComponent<TMPro.TextMeshPro>();
        tmpComp.text = cost.ToString(); // YAZMAZSAN "T" KALIR!
        tmpComp.fontSize = 4;
        tmpComp.color = Color.white;
        costTexts[pos] = go;
    }


    // Cost textlerini temizler (her arama öncesi çağır)
    void ClearCostTexts()
    {
        foreach (var go in costTexts.Values)
            Destroy(go);
        costTexts.Clear();
    }

    // Debug view (Editor Gizmos)
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
