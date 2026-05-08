using UnityEngine;
using System.Collections;

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
    private bool isFalling = false;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        audioSource = GetComponent<AudioSource>();
        ChooseRandomDirection();

        if (musicSource != null)
        {
            musicSource.volume = 0.25f;
        }
    }

    void Update()
    {
        if (isFalling)
        {
            return;
        }

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

    IEnumerator FallIntoHole(Vector3 holePosition)
    {
        isFalling = true;

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
        Vector3 endPosition = new Vector3(holePosition.x, startPosition.y - fallDistance, holePosition.z);

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

    IEnumerator DuckMusic()
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
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.2f;
            Gizmos.DrawRay(rayOrigin, moveDirection * rayDistance);
        }
    }
}