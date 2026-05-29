using UnityEngine;

public class MenuHoverSelector : MonoBehaviour
{
    [Header("Ray Source")]
    [SerializeField] private Transform rayOrigin;

    [Header("Ray Direction")]
    [SerializeField] private Vector3 localRayDirection = Vector3.forward;

    [Header("Ray Settings")]
    [SerializeField] private float rayDistance = 10f;
    [SerializeField] private LayerMask buttonLayerMask;

    [Header("Selection Settings")]
    [SerializeField] private float hoverTimeToSelect = 3f;

    [Header("Debug")]
    [SerializeField] private bool drawDebugRay = true;
    [SerializeField] private LineRenderer debugLine;

    private MenuButton currentButton;
    private float hoverTimer;
    private bool hasSelected;

    private void Update()
    {
        if (rayOrigin == null || hasSelected)
        {
            return;
        }

        Vector3 rayStart = rayOrigin.position;
        Vector3 rayDirection = rayOrigin.TransformDirection(localRayDirection.normalized);
        Vector3 rayEnd = rayStart + rayDirection * rayDistance;

        if (drawDebugRay)
        {
            Debug.DrawRay(rayStart, rayDirection * rayDistance, Color.red);

            if (debugLine != null)
            {
                debugLine.SetPosition(0, rayStart);
                debugLine.SetPosition(1, rayEnd);
            }
        }

        if (Physics.Raycast(rayStart, rayDirection, out RaycastHit hit, rayDistance, buttonLayerMask))
        {
            MenuButton hitButton = hit.collider.GetComponentInParent<MenuButton>();

            if (hitButton != null)
            {
                HandleHover(hitButton);
                return;
            }
        }

        ClearHover();
    }

    private void HandleHover(MenuButton button)
    {
        if (currentButton != button)
        {
            ClearHover();

            currentButton = button;
            currentButton.SetHover(true);
            hoverTimer = 0f;

            Debug.Log("Hovering over: " + currentButton.name);
        }

        hoverTimer += Time.deltaTime;

        Debug.Log("Hover progress: " + hoverTimer.ToString("F1") + " / " + hoverTimeToSelect);

        if (hoverTimer >= hoverTimeToSelect)
        {
            hasSelected = true;

            Debug.Log("Selected level: " + currentButton.name);
            currentButton.Select();
        }
    }

    private void ClearHover()
    {
        if (currentButton != null)
        {
            currentButton.SetHover(false);
            currentButton = null;
        }

        hoverTimer = 0f;
    }
}