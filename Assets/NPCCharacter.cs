using System.Collections;
using UnityEngine;

[System.Serializable]
public class SimpleDialogueGroup
{
    [TextArea(2, 5)]
    public string[] lines;
}

public class NPCCharacter : MonoBehaviour
{
    [Header("NPC Info")]
    public string npcName;

    [Header("Dialogue")]
    public SimpleDialogueGroup[] dialogues;
    public DialogueManager dialogueManager;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public int roamRadius = 3;
    public Direction startingDirection;

    [Header("Detection")]
    public float talkDistance = 1.5f;
    public float idleTimeMin = 1f;
    public float idleTimeMax = 3f;

    [Header("References")]
    public PlayerLogic playerLogic;
    public AudioSource detectedPlayer;
    public Animator animator;
    public BoxCollider2D boxCollider2D;

    [Header("Layers")]
    public LayerMask solidLayer;
    public LayerMask waterLayer;
    public LayerMask jumpLayer;

    private Vector3 originalPosition;
    private Direction currentDirection;
    private bool isTalking;

    void Start()
    {
        originalPosition = transform.position;
        currentDirection = startingDirection;

        if (playerLogic == null)
            playerLogic = FindObjectOfType<PlayerLogic>();

        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (boxCollider2D == null)
            boxCollider2D = GetComponent<BoxCollider2D>();

        FaceDirection(currentDirection);
        StartCoroutine(RoamRoutine());
    }

    void Update()
    {
        if (playerLogic == null || dialogueManager == null) return;
        if (isTalking || dialogueManager.IsDialogueActive()) return;

        if (IsTalkInputPressed())
            TryTalk();
    }

    bool IsTalkInputPressed()
    {
        return Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.KeypadEnter)
            || Input.GetMouseButtonDown(0);
    }

    IEnumerator RoamRoutine()
    {
        while (true)
        {
            if (isTalking || (dialogueManager != null && dialogueManager.IsDialogueActive()))
            {
                StopMoving();
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(Random.Range(idleTimeMin, idleTimeMax));

            // 대기하는 동안 대화가 시작됐으면 여기서 중단
            if (isTalking || (dialogueManager != null && dialogueManager.IsDialogueActive()))
            {
                StopMoving();
                continue;
            }

            currentDirection = GetRandomDirection();
            FaceDirection(currentDirection);

            Vector3 target = transform.position + DirectionToVector3(currentDirection);

            if (Vector3.Distance(originalPosition, target) <= roamRadius && IsWalkable(target))
                yield return MoveTo(target);
            else
                StopMoving();
        }
    }

    IEnumerator MoveTo(Vector3 target)
    {
        if (animator != null)
            animator.SetBool("isMoving", true);

        while ((target - transform.position).sqrMagnitude > 0.001f)
        {
            if (isTalking || (dialogueManager != null && dialogueManager.IsDialogueActive()))
            {
                StopMoving();
                yield break;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = target;
        StopMoving();
    }

    void StopMoving()
    {
        if (animator != null)
            animator.SetBool("isMoving", false);
    }

    bool IsWalkable(Vector3 target)
    {
        Collider2D hit = Physics2D.OverlapCircle(
            target,
            0.2f,
            solidLayer | waterLayer | jumpLayer
        );

        return hit == null;
    }

    void TryTalk()
    {
        if (dialogues == null || dialogues.Length == 0) return;

        float distance = Vector2.Distance(transform.position, playerLogic.transform.position);
        if (distance > talkDistance) return;

        isTalking = true;
        StopMoving();

        FacePlayer();
        StopMoving();

        // NPC가 플레이어 방향 보기
        FacePlayer();

        // 플레이어 이동 고정
        playerLogic.IsBusy = true;
        playerLogic.IsMoving = false;
        playerLogic.IsRunning = false;

        if (playerLogic.Animator != null)
        {
            playerLogic.Animator.SetBool("isMoving", false);
            playerLogic.Animator.SetBool("isRunning", false);
        }

        PlayDetectedSound();

        int randomIndex = Random.Range(0, dialogues.Length);
        SimpleDialogueGroup selected = dialogues[randomIndex];

        if (selected == null || selected.lines == null || selected.lines.Length == 0)
        {
            isTalking = false;
            playerLogic.IsBusy = false;
            return;
        }

        dialogueManager.StartDialogue(selected.lines);
        StartCoroutine(WaitUntilDialogueEnds());
    }

    void PlayDetectedSound()
    {
        if (detectedPlayer == null) return;
        if (!detectedPlayer.enabled) return;
        if (!detectedPlayer.gameObject.activeInHierarchy) return;

        detectedPlayer.Play();
    }

    IEnumerator WaitUntilDialogueEnds()
    {
        while (dialogueManager != null && dialogueManager.IsDialogueActive())
            yield return null;

        isTalking = false;

        // 대화 끝나면 플레이어 이동 가능
        if (playerLogic != null)
            playerLogic.IsBusy = false;
    }

    void FacePlayer()
    {
        Vector3 diff = playerLogic.transform.position - transform.position;

        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
            currentDirection = diff.x > 0 ? Direction.Right : Direction.Left;
        else
            currentDirection = diff.y > 0 ? Direction.Up : Direction.Down;

        FaceDirection(currentDirection);
    }

    void FaceDirection(Direction dir)
    {
        if (animator == null) return;

        Vector2 v = DirectionToVector(dir);

        animator.SetFloat("moveX", v.x);
        animator.SetFloat("moveY", v.y);
    }

    Direction GetRandomDirection()
    {
        int value = Random.Range(0, 4);

        switch (value)
        {
            case 0: return Direction.Up;
            case 1: return Direction.Down;
            case 2: return Direction.Left;
            default: return Direction.Right;
        }
    }

    Vector3 DirectionToVector3(Direction dir)
    {
        Vector2 v = DirectionToVector(dir);
        return new Vector3(v.x, v.y, 0f);
    }

    Vector2 DirectionToVector(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return Vector2.up;
            case Direction.Down: return Vector2.down;
            case Direction.Left: return Vector2.left;
            case Direction.Right: return Vector2.right;
            default: return Vector2.down;
        }
    }
}