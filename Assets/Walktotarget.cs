using UnityEngine;

public class WalkToTarget : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] Transform target;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float stopDistance = 0.5f;

    [Header("Dialogue")]
    [SerializeField] DialogueManager dialogueManager;
    [TextArea(2, 5)]
    [SerializeField] string[] arrivalLines;

    Animator animator;
    bool hasArrived = false;
    bool hasStartedDialogue = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetBool("isWalking", true);
        }
    }

    void Update()
    {
        if (target == null || hasArrived) return;

        Vector3 targetPos = target.position;
        targetPos.y = transform.position.y;

        Vector3 direction = targetPos - transform.position;
        float distance = direction.magnitude;

        if (distance <= stopDistance)
        {
            hasArrived = true;

            if (animator != null)
            {
                animator.SetBool("isWalking", false);
            }

            StartArrivalDialogue();
            return;
        }

        direction.Normalize();
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                8f * Time.deltaTime
            );
        }
    }

    void StartArrivalDialogue()
    {
        if (hasStartedDialogue) return;
        if (dialogueManager == null) return;
        if (dialogueManager.IsDialogueActive()) return;

        hasStartedDialogue = true;
        dialogueManager.StartDialogue(arrivalLines);
    }
}