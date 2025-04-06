using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 1f; // Units per second
    [Header("Pan Boundaries")]
    [SerializeField] private Vector2 panLimitMin = new(-50, -50); // Min pan boundaries
    [SerializeField] private Vector2 panLimitMax = new(50, 50); // Max pan boundaries

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f; // Units per scroll
    [SerializeField] private float minZoom = 2f; // Orthographic size
    [SerializeField] private float maxZoom = 10f; // Orthographic size

    private Vector3 dragOrigin; // Origin of the drag
    private Camera cam; // Camera reference
    private bool isDraggingCamera; // Is the camera currently being dragged?
    private float aspectRatio; // Aspect ratio of the camera

    #region Unity Methods

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        HandleCameraPan();
        HandleCameraZoom();
    }

    #endregion

    #region Camera Controls

    private void HandleCameraPan()
    {
        // Only check UI/building collision at drag START
        if (Input.GetMouseButtonDown(0))
        {
            // Prevent starting drag over UI or buildings
            if (EventSystem.current.IsPointerOverGameObject() || IsClickingBuilding())
            {
                return;
            }

            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
            isDraggingCamera = true;
        }

        // Continue drag regardless of current mouse position
        if (Input.GetMouseButton(0) && isDraggingCamera)
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 newPosition = transform.position + difference * panSpeed;
            transform.position = ClampCameraPosition(newPosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDraggingCamera = false;
        }
    }

    private bool IsClickingBuilding()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        return hit.collider != null && hit.collider.GetComponent<Building>() != null;
    }

    private Vector3 ClampCameraPosition(Vector3 targetPosition)
    {
        // Calculate camera viewport dimensions at current zoom
        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * aspectRatio;

        // Calculate effective boundaries
        float effectiveMinX = panLimitMin.x + horzExtent;
        float effectiveMaxX = panLimitMax.x - horzExtent;
        float effectiveMinY = panLimitMin.y + vertExtent;
        float effectiveMaxY = panLimitMax.y - vertExtent;

        // Apply clamping with boundaries
        return new Vector3(
            Mathf.Clamp(targetPosition.x, effectiveMinX, effectiveMaxX),
            Mathf.Clamp(targetPosition.y, effectiveMinY, effectiveMaxY),
            targetPosition.z
        );
    }

    private void HandleCameraZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0 || EventSystem.current.IsPointerOverGameObject())
        {
            return;
        } 

        float newSize = Mathf.Clamp(
            cam.orthographicSize - scroll * zoomSpeed,
            minZoom,
            maxZoom
        );

        // Only apply zoom if it keeps camera within boundaries
        if (WouldZoomStayInBounds(newSize))
        {
            cam.orthographicSize = newSize;
            transform.position = ClampCameraPosition(transform.position);
        }
    }

    private bool WouldZoomStayInBounds(float proposedSize)
    {
        // Temporary calculate if zoom would keep camera in bounds
        float vertExtent = proposedSize;
        float horzExtent = vertExtent * aspectRatio;

        return (transform.position.x >= panLimitMin.x + horzExtent) &&
               (transform.position.x <= panLimitMax.x - horzExtent) &&
               (transform.position.y >= panLimitMin.y + vertExtent) &&
               (transform.position.y <= panLimitMax.y - vertExtent);
    }

    #endregion

}