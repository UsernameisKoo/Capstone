using UnityEngine;

public class CarCharacter : MonoBehaviour
{
    public enum MoveDirection
    {
        Left,
        Right
    }

    [Header("Movement")]
    public float moveSpeed = 3f;
    public MoveDirection moveDirection = MoveDirection.Right;

    [Header("Crosswalk Points")]
    public Transform respawnCrosswalk;
    public Transform endCrosswalk;

    [Header("Stop Settings")]
    public float endStopDistance = 0.5f;

    [Header("Player Collision")]
    public LayerMask playerLayer;

    [Header("Camera")]
    public Camera mainCamera;
    private Rigidbody2D rb;

    [Header("Respawn")]
    public float respawnDelay = 3f;


    private float respawnTimer;
    private bool isWaitingRespawn;

    private bool isStoppedByPlayer;
    private bool reachedEnd;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        rb = GetComponent<Rigidbody2D>();

        if (respawnCrosswalk != null)
            rb.position = respawnCrosswalk.position;
    }

    void FixedUpdate()
    {
        if (reachedEnd)
        {
            if (!IsVisibleFromMainCamera())
            {
                if (!isWaitingRespawn)
                {
                    isWaitingRespawn = true;
                    respawnTimer = respawnDelay;
                }

                respawnTimer -= Time.fixedDeltaTime;

                if (respawnTimer <= 0f)
                    RespawnCar();
            }

            return;
        }

        if (!IsVisibleFromMainCamera())
            return;

        if (isStoppedByPlayer)
            return;

        if (endCrosswalk != null)
        {
            float distToEnd = Vector2.Distance(rb.position, endCrosswalk.position);

            if (distToEnd <= endStopDistance)
            {
                reachedEnd = true;
                return;
            }
        }

        MoveCar();
    }

    void MoveCar()
    {
        Vector2 dir = moveDirection == MoveDirection.Right
            ? Vector2.right
            : Vector2.left;

        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
    }

    void RespawnCar()
    {
        if (respawnCrosswalk != null)
            rb.position = respawnCrosswalk.position;

        reachedEnd = false;
        isStoppedByPlayer = false;

        isWaitingRespawn = false;   // 추가
    }

    bool IsVisibleFromMainCamera()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return true;

        Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);

        return viewPos.z > 0f &&
               viewPos.x >= 0f && viewPos.x <= 1f &&
               viewPos.y >= 0f && viewPos.y <= 1f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger 대상: " + other.name);

        if (((1 << other.gameObject.layer) & playerLayer) != 0)
            isStoppedByPlayer = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
            isStoppedByPlayer = false;
    }
}