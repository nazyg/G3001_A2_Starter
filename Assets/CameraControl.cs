using UnityEngine;

/// <summary>
/// Used to control the main camera in the scene by panning and zooming within set boundaries. To be attached to the main camera.
/// </summary>
public class CameraControl : MonoBehaviour
{
    private float panSpeed = 6.0f;
    private float zoomSpeed = 8.0f;
    private Vector2 zoomRange = new Vector2(4, 20);
    private Bounds bounds = new Bounds(new Vector3(8, 8, -10), new Vector3(10, 10, 0));

    // Update is called once per frame
    void Update()
    {
        Camera.main.orthographicSize += Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, zoomRange.x, zoomRange.y);

       Vector3 input = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);

        transform.position += input.normalized * panSpeed *Time.deltaTime;

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, bounds.min.x, bounds.max.x),
             Mathf.Clamp(transform.position.y, bounds.min.y, bounds.max.y),
              Mathf.Clamp(transform.position.z, bounds.min.z, bounds.max.z)
              );
    }
}
