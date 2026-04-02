using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpForce = 5f; // 점프 힘
    Rigidbody rb;
    Vector3 moveDir;
    Animator animator;
    Camera mainCamera;
    bool isGrounded = true; // 땅에 있는지 확인

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        mainCamera = Camera.main;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject spawn = GameObject.Find("SpawnPoint");
        if (spawn != null)
        {
            transform.position = spawn.transform.position;
        }
        mainCamera = Camera.main;

        // 씬별 캐릭터 크기 설정
        if (scene.name == "House Interior")
        {
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        else if (scene.name == "Town")
        {
            transform.localScale = new Vector3(0.4f, 0.4f, 0.4f); // 0.3 - 0.2 = 0.1
        }
    }
    void Update()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.A)) h = -1f;
        if (Input.GetKey(KeyCode.D)) h = 1f;
        if (Input.GetKey(KeyCode.W)) v = 1f;
        if (Input.GetKey(KeyCode.S)) v = -1f;



        if (mainCamera != null)
        {
            Vector3 camForward = mainCamera.transform.forward;
            Vector3 camRight = mainCamera.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            moveDir = (camForward * v + camRight * h).normalized;
        }
        else
        {
            moveDir = new Vector3(h, 0f, v).normalized;
        }

        // 스페이스바로 점프
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        if (animator != null)
        {
            animator.SetBool("isWalking", moveDir.magnitude > 0);
        }
    }

    void FixedUpdate()
    {
        Vector3 newPos = rb.position + moveDir * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 10f * Time.fixedDeltaTime));
        }
    }

    // 땅에 닿으면 다시 점프 가능
    void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
