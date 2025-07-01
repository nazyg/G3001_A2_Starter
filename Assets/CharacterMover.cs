using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class CharacterMover : MonoBehaviour
{
    public Tilemap tilemap;
    public List<TileBase> walkableTiles;
    public float moveSpeed = 5f;

    private Vector3 targetPosition;
    private bool isMoving = false;

    // Path following
    private Queue<Vector3Int> pathQueue = new Queue<Vector3Int>();
    private bool isFollowingPath = false;

    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        if (isFollowingPath)
        {
            FollowPath();
        }
        else
        {
            ManualControl();
        }
    }

    void ManualControl()
    {
        if (!isMoving)
        {
            Vector3Int currentCell = tilemap.WorldToCell(transform.position);
            Vector3Int nextCell = currentCell;

            if (Input.GetKeyDown(KeyCode.W)) nextCell += Vector3Int.up;
            if (Input.GetKeyDown(KeyCode.S)) nextCell += Vector3Int.down;
            if (Input.GetKeyDown(KeyCode.A)) nextCell += Vector3Int.left;
            if (Input.GetKeyDown(KeyCode.D)) nextCell += Vector3Int.right;

            if (nextCell != currentCell && IsWalkable(nextCell))
            {
                targetPosition = tilemap.GetCellCenterWorld(nextCell);
                isMoving = true;
            }
        }
        else
        {
            MoveToTarget();
        }
    }

    void FollowPath()
    {
        if (!isMoving && pathQueue.Count > 0)
        {
            Vector3Int nextTile = pathQueue.Dequeue();
            targetPosition = tilemap.GetCellCenterWorld(nextTile);
            isMoving = true;
        }

        if (isMoving)
        {
            MoveToTarget();

            if (pathQueue.Count == 0 && !isMoving)
            {
                isFollowingPath = false; // path bitti
            }
        }
    }

    void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            isMoving = false;
        }
    }

    bool IsWalkable(Vector3Int pos)
    {
        TileBase tile = tilemap.GetTile(pos);
        return walkableTiles.Contains(tile);
    }

    public void SetPath(List<Vector3Int> path)
    {
        pathQueue.Clear();

        // start tile karakterin altındaki tile olabilir, onu atla
        Vector3Int currentTile = tilemap.WorldToCell(transform.position);

        foreach (var step in path)
        {
            if (step != currentTile)
                pathQueue.Enqueue(step);
        }

        isFollowingPath = true;
    }
}
