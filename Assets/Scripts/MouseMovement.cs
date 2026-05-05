using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.5f;

    [Header("Obstacle Detection")]
    public float rayDistance = 0.8f;
    public float castRadius = 0.15f;
    public LayerMask obstacleLayer;

    [Header("Bounce Settings")]
    public float bounceAngle = 45f;

    private Vector3 moveDirection;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        ChooseRandomDirection();
    }

    void Update()
    {
        CheckForObstacle();
        MoveMouse();
    }

    void MoveMouse()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    void CheckForObstacle()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.2f;

        if (Physics.SphereCast(rayOrigin, castRadius, moveDirection, out RaycastHit hit, rayDistance, obstacleLayer))
        {
            BounceAwayFromWall(hit.normal);
        }
    }

    void BounceAwayFromWall(Vector3 wallNormal)
    {
        wallNormal.y = 0;
        wallNormal.Normalize();

        int randomSide = Random.Range(0, 2) == 0 ? -1 : 1;

        moveDirection = Quaternion.Euler(0, bounceAngle * randomSide, 0) * wallNormal;
        moveDirection.Normalize();

        transform.position += wallNormal * 0.1f;
    }

    void ChooseRandomDirection()
    {
        int randomDirection = Random.Range(0, 4);

        switch (randomDirection)
        {
            case 0:
                moveDirection = Vector3.forward;
                break;
            case 1:
                moveDirection = Vector3.back;
                break;
            case 2:
                moveDirection = Vector3.left;
                break;
            case 3:
                moveDirection = Vector3.right;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hole"))
        {
            Debug.Log("Mouse fell into the hole!");

            if (gameManager != null)
            {
                gameManager.AddPoint();
            }

            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.2f;
            Gizmos.DrawRay(rayOrigin, moveDirection * rayDistance);
        }
    }
}