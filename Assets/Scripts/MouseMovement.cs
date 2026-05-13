using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MouseMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.5f;

    [Header("Obstacle Detection")]
    public float rayDistance = 2.5f;
    public float castRadius = 0.5f;
    public LayerMask obstacleLayer;

    [Header("Bounce Settings")]
    public float bounceAngle = 45f;
    public float pushAwayDistance = 0.4f;
    public float bounceCooldown = 0.15f;

    [Header("Fall Animation")]
    public float fallDuration = 1f;
    public float spinSpeed = 720f;
    public float fallDistance = 1f;

    [Header("Audio")]
    public AudioClip fallSound;
    public AudioSource musicSource;

    private Vector3 moveDirection;
    private GameManager gameManager;
    private AudioSource audioSource;
    private Rigidbody rb;

    private bool isFalling = false;
    private float lastBounceTime = -999f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.constraints = RigidbodyConstraints.FreezePositionY |
                         RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
    }

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        audioSource = GetComponent<AudioSource>();

        ChooseRandomDirection();

        if (musicSource != null)
        {
            musicSource.volume = 0.25f;
        }
    }

    private void FixedUpdate()
    {
        if (isFalling)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        CheckForObstacle();

        rb.linearVelocity = moveDirection * moveSpeed;

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(targetRotation);
        }
    }

    private void CheckForObstacle()
    {
        if (Time.time - lastBounceTime < bounceCooldown)
        {
            return;
        }

        Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;

        if (Physics.SphereCast(
            rayOrigin,
            castRadius,
            moveDirection,
            out RaycastHit hit,
            rayDistance,
            obstacleLayer,
            QueryTriggerInteraction.Ignore))
        {
            Bounce(hit.normal);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isFalling)
        {
            return;
        }

        if (Time.time - lastBounceTime < bounceCooldown)
        {
            return;
        }

        bool hitObstacle = ((1 << collision.gameObject.layer) & obstacleLayer) != 0;

        if (!hitObstacle)
        {
            return;
        }

        if (collision.contacts.Length > 0)
        {
            Bounce(collision.contacts[0].normal);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (isFalling)
        {
            return;
        }

        bool hitObstacle = ((1 << collision.gameObject.layer) & obstacleLayer) != 0;

        if (!hitObstacle)
        {
            return;
        }

        if (collision.contacts.Length > 0)
        {
            Vector3 normal = collision.contacts[0].normal;
            normal.y = 0f;

            if (normal != Vector3.zero)
            {
                rb.position += normal.normalized * pushAwayDistance * Time.fixedDeltaTime;
            }
        }
    }

    private void Bounce(Vector3 normal)
    {
        lastBounceTime = Time.time;

        normal.y = 0f;

        if (normal == Vector3.zero)
        {
            ChooseRandomDirection();
            return;
        }

        normal.Normalize();

        Vector3 reflectedDirection = Vector3.Reflect(moveDirection, normal);
        reflectedDirection.y = 0f;

        if (reflectedDirection == Vector3.zero)
        {
            reflectedDirection = normal;
        }

        reflectedDirection.Normalize();

        int randomSide = Random.Range(0, 2) == 0 ? -1 : 1;

        moveDirection = Quaternion.Euler(0f, bounceAngle * randomSide, 0f) * reflectedDirection;
        moveDirection.Normalize();

        rb.position += normal * pushAwayDistance;
        rb.linearVelocity = moveDirection * moveSpeed;
    }

    private void ChooseRandomDirection()
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
        if (isFalling)
        {
            return;
        }

        if (other.CompareTag("Hole"))
        {
            Debug.Log("Mouse fell into the hole!");

            if (gameManager != null)
            {
                gameManager.AddPoint();
            }

            StartCoroutine(FallIntoHole(other.transform.position));
        }
    }

    private IEnumerator FallIntoHole(Vector3 holePosition)
    {
        isFalling = true;
        rb.linearVelocity = Vector3.zero;

        Collider mouseCollider = GetComponent<Collider>();

        if (mouseCollider != null)
        {
            mouseCollider.enabled = false;
        }

        if (musicSource != null)
        {
            StartCoroutine(DuckMusic());
        }

        if (fallSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fallSound, 1f);
        }

        Vector3 startPosition = transform.position;
        Vector3 endPosition = new Vector3(
            holePosition.x,
            startPosition.y - fallDistance,
            holePosition.z
        );

        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;

        float elapsedTime = 0f;

        while (elapsedTime < fallDuration)
        {
            float t = elapsedTime / fallDuration;

            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private IEnumerator DuckMusic()
    {
        if (musicSource != null)
        {
            musicSource.volume = 0.08f;

            yield return new WaitForSeconds(1f);

            musicSource.volume = 0.25f;
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Gizmos.color = Color.red;

        Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;
        Gizmos.DrawRay(rayOrigin, moveDirection * rayDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rayOrigin + moveDirection * rayDistance, castRadius);
    }
}