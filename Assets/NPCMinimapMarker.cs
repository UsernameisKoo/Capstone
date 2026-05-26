using UnityEngine;
using UnityEngine.UI;

public class NPCMinimapMarker : MonoBehaviour
{
    public Sprite markerSprite;
    public Camera miniMapCamera;

    // MiniMapMask 넣기
    public RectTransform minimapRoot;

    public float markerSize = 12f;

    // 테두리에서 살짝 안쪽으로
    public float edgePadding = 6f;

    [Header("마커 스프라이트")]
    public Sprite normalSprite;
    public Sprite defeatedSprite;

    private Image markerImage;
    private Trainer trainer;
    private RectTransform markerRect;

    private bool lastDefeatedState = false;

    private void Start()
    {
        // NPCMinimapMarker가 자식에 붙어 있으므로 부모에서 Trainer 찾기
        trainer = GetComponentInParent<Trainer>();

        if (trainer == null)
        {
            Debug.LogError("부모에서 Trainer를 찾지 못했습니다: " + gameObject.name);
            return;
        }

        Debug.Log("미니맵 마커 연결됨: " + trainer.trainerName);

        if (minimapRoot == null)
        {
            Debug.LogWarning("minimapRoot가 비어 있습니다.");
            return;
        }

        GameObject marker = new GameObject("MiniMapMarker");
        marker.transform.SetParent(minimapRoot, false);

        markerRect = marker.AddComponent<RectTransform>();
        markerRect.sizeDelta = new Vector2(markerSize, markerSize);
        markerRect.anchoredPosition = Vector2.zero;

        markerImage = marker.AddComponent<Image>();
        markerImage.sprite = normalSprite != null ? normalSprite : markerSprite;
        markerImage.raycastTarget = false;

        UpdateMarkerVisual();
    }

    private void Update()
    {
        if (miniMapCamera == null || minimapRoot == null || markerRect == null)
            return;

        UpdateMarkerPosition();
        UpdateMarkerVisual();
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
                markerPos = markerPos.normalized * radius;
        }
        else
        {
            if (centered == Vector2.zero)
                centered = Vector2.up;

            markerPos = centered.normalized * radius;
        }

        markerRect.anchoredPosition = markerPos;
    }

    private void UpdateMarkerVisual()
    {
        if (markerImage == null || trainer == null) return;

        bool defeated = trainer.IsDefeated;

        markerImage.color = defeated ? Color.blue : Color.red;

        if (defeated)
        {
            if (defeatedSprite != null)
                markerImage.sprite = defeatedSprite;
        }
        else
        {
            markerImage.sprite = normalSprite != null ? normalSprite : markerSprite;
        }

        if (defeated && !lastDefeatedState)
        {
            Debug.Log(trainer.trainerName + " 처치 완료!");
        }

        lastDefeatedState = defeated;
    }

    private void OnDestroy()
    {
        if (markerRect != null)
            Destroy(markerRect.gameObject);
    }
}