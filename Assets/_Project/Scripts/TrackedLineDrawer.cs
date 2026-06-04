using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class TrackedLineDrawer : MonoBehaviour
{
    [Header("Drawing")]
    [SerializeField] private float maxLineLength = 40f;
    [SerializeField] private float minPointDistance = 0.03f;
    [SerializeField] private float lineHeightOffset = 0.15f;

    [Header("Upside Down Detection")]
    [SerializeField] private float upsideDownThreshold = 0.7f;

    [Header("Visuals")]
    [SerializeField] private float lineWidth = 0.8f;
    [SerializeField] private Material lineMaterial;

    [Header("Physical Barrier")]
    [SerializeField] private float colliderHeight = 1.5f;
    [SerializeField] private float colliderThickness = 0.8f;
    [SerializeField] private string obstacleLayerName = "Obstacle";

    [Header("Behaviour")]
    [SerializeField] private bool clearOldLineWhenStartingNewLine = true;

    [Header("Home Testing")]
    [SerializeField] private bool keyboardTestMode = false;
    [SerializeField] private float testMoveSpeed = 20f;

    private LineRenderer lineRenderer;
    private readonly List<Vector3> points = new List<Vector3>();
    private readonly List<GameObject> colliders = new List<GameObject>();

    private bool wasDrawing = false;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;

        ApplyLineSettings();
    }

    private void Update()
    {
        ApplyLineSettings();

        if (keyboardTestMode)
        {
            MoveWithKeyboard();
        }

        bool isDrawing = keyboardTestMode
            ? Keyboard.current != null && Keyboard.current.spaceKey.isPressed
            : IsDeviceUpsideDown();

        // When drawing starts again, reset old line instead of connecting from the old position.
        if (isDrawing && !wasDrawing)
        {
            StartNewLine();
        }

        if (isDrawing)
        {
            DrawPoint();
        }

        wasDrawing = isDrawing;
    }

    private void ApplyLineSettings()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.widthMultiplier = 1f;
        lineRenderer.useWorldSpace = true;

        if (lineMaterial != null)
        {
            lineRenderer.material = lineMaterial;
        }
    }

    private void StartNewLine()
    {
        if (clearOldLineWhenStartingNewLine)
        {
            ClearLine();
        }

        Vector3 startPoint = transform.position;
        startPoint.y += lineHeightOffset;

        points.Add(startPoint);
        UpdateLine();
    }

    private void MoveWithKeyboard()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        Vector3 movement = Vector3.zero;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            movement.z += 1f;
        }

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            movement.z -= 1f;
        }

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            movement.x += 1f;
        }

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            movement.x -= 1f;
        }

        movement.Normalize();

        transform.position += movement * testMoveSpeed * Time.deltaTime;
    }

    private bool IsDeviceUpsideDown()
    {
        // Correct tracker rotation:
        // Drawing starts when the tracker's local forward axis points downward.
        return Vector3.Dot(transform.forward, Vector3.down) > upsideDownThreshold;
    }

    private void DrawPoint()
    {
        Vector3 currentPoint = transform.position;
        currentPoint.y += lineHeightOffset;

        if (points.Count == 0)
        {
            points.Add(currentPoint);
            UpdateLine();
            return;
        }

        Vector3 lastPoint = points[points.Count - 1];
        float distanceFromLastPoint = Vector3.Distance(lastPoint, currentPoint);

        if (distanceFromLastPoint < minPointDistance)
        {
            return;
        }

        // This fills gaps if the tracker moves quickly between frames.
        int pointsToAdd = Mathf.FloorToInt(distanceFromLastPoint / minPointDistance);

        for (int i = 1; i <= pointsToAdd; i++)
        {
            float t = i / (float)pointsToAdd;
            Vector3 interpolatedPoint = Vector3.Lerp(lastPoint, currentPoint, t);
            points.Add(interpolatedPoint);
        }

        TrimLineToMaxLength();
        UpdateLine();
        RebuildColliders();
    }

    private void TrimLineToMaxLength()
    {
        while (GetTotalLineLength() > maxLineLength && points.Count > 1)
        {
            points.RemoveAt(0);
        }
    }

    private float GetTotalLineLength()
    {
        float total = 0f;

        for (int i = 1; i < points.Count; i++)
        {
            total += Vector3.Distance(points[i - 1], points[i]);
        }

        return total;
    }

    private void UpdateLine()
    {
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    private void RebuildColliders()
    {
        ClearColliders();

        for (int i = 1; i < points.Count; i++)
        {
            CreateSegmentCollider(points[i - 1], points[i]);
        }
    }

    private void CreateSegmentCollider(Vector3 start, Vector3 end)
    {
        Vector3 middle = (start + end) / 2f;
        Vector3 direction = end - start;

        float length = direction.magnitude;

        if (length <= 0.01f)
        {
            return;
        }

        GameObject segment = new GameObject("Line Collider Segment");

        // Do not parent this to the moving tracker.
        // The collider must stay fixed in world space.
        segment.transform.position = middle;
        segment.transform.rotation = Quaternion.LookRotation(direction);

        int obstacleLayer = LayerMask.NameToLayer(obstacleLayerName);

        if (obstacleLayer == -1)
        {
            Debug.LogWarning("Obstacle layer does not exist. Create a layer named: " + obstacleLayerName);
        }
        else
        {
            segment.layer = obstacleLayer;
        }

        BoxCollider boxCollider = segment.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(colliderThickness, colliderHeight, length);
        boxCollider.center = Vector3.zero;
        boxCollider.isTrigger = false;

        colliders.Add(segment);
    }

    private void ClearColliders()
    {
        foreach (GameObject colliderObject in colliders)
        {
            if (colliderObject != null)
            {
                Destroy(colliderObject);
            }
        }

        colliders.Clear();
    }

    private void ClearLine()
    {
        points.Clear();
        lineRenderer.positionCount = 0;
        ClearColliders();
    }
}