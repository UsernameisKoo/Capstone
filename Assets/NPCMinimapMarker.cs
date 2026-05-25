using UnityEngine;
using UnityEngine.UI;

public class NPCMinimapMarker : MonoBehaviour
{
    public Sprite markerSprite;

    public Camera miniMapCamera;

    // 여기에 MiniMapMask 넣기
    public RectTransform minimapRoot;

    public float markerSize = 12f;

    // 테두리에서 살짝 안쪽으로
    public float edgePadding = 6f;

    private Image markerImage;
    private NPC npc;
    private RectTransform markerRect;

    private void Start()
    {
        npc = GetComponent<NPC>();

        GameObject marker = new GameObject("MiniMapMarker");
        marker.transform.SetParent(minimapRoot, false);

        markerRect = marker.AddComponent<RectTransform>();
        markerRect.sizeDelta = new Vector2(markerSize, markerSize);
        markerRect.anchoredPosition = Vector2.zero;

        markerImage = marker.AddComponent<Image>();
        markerImage.sprite = markerSprite;
        markerImage.raycastTarget = false;

        UpdateMarkerColor();
    }

    private void Update()
    {
        if (miniMapCamera == null || minimapRoot == null) return;

        UpdateMarkerPosition();
        UpdateMarkerColor();
    }

    private void UpdateMarkerPosition()
    {
        Vector3 viewportPos = miniMapCamera.WorldToViewportPoint(transform.position);

        Vector2 centered = new Vector2(
            viewportPos.x - 0.5f,
            viewportPos.y - 0.5f
        );

        float radius = Mathf.Min(minimapRoot.rect.width, minimapRoot.rect.height) * 0.5f;
        radius -= edgePadding;

        Vector2 markerPos;

        bool isInsideCameraView =
            viewportPos.z > 0 &&
            viewportPos.x >= 0f && viewportPos.x <= 1f &&
            viewportPos.y >= 0f && viewportPos.y <= 1f;

        if (isInsideCameraView)
        {
            markerPos = new Vector2(
                centered.x * minimapRoot.rect.width,
                centered.y * minimapRoot.rect.height
            );

            if (markerPos.magnitude > radius)
            {
                markerPos = markerPos.normalized * radius;
            }
        }
        else
        {
            if (centered == Vector2.zero)
                centered = Vector2.up;

            markerPos = centered.normalized * radius;
        }

        markerRect.anchoredPosition = markerPos;
    }

    private void UpdateMarkerColor()
    {
        if (npc == null || markerImage == null) return;

        markerImage.color = npc.IsDefeated ? Color.blue : Color.red;
    }
}