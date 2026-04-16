using UnityEngine;

public class MinimapPlayerArrow : MonoBehaviour
{
    public Transform player;
    public RectTransform arrow;

    private Vector3 lastPlayerPosition;
    private Vector3 moveDir = Vector3.up;

    void Start()
    {
        if (player != null)
            lastPlayerPosition = player.position;

        if (arrow != null)
            arrow.anchoredPosition = Vector2.zero; // 중앙 고정
    }

    void Update()
    {
        if (player == null || arrow == null) return;

        // 항상 중앙 고정
        arrow.anchoredPosition = Vector2.zero;

        // 이동 방향으로 회전
        Vector3 delta = player.position - lastPlayerPosition;

        if (delta.sqrMagnitude > 0.0001f)
            moveDir = delta.normalized;

        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        arrow.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);

        lastPlayerPosition = player.position;
    }
}