using UnityEngine;

public class ArrowBuilder : MonoBehaviour
{
    [Header("Arrow Parts")]
    [SerializeField] Renderer bodyRenderer;
    [SerializeField] Renderer headRenderer;

    [Header("Color")]
    [SerializeField] Color bodyColor = new Color32(48, 218, 255, 100);
    [SerializeField] Color headColor = new Color32(48, 218, 255, 100);

    [Header("Floating")]
    [SerializeField] float floatHeight = 0.15f;
    [SerializeField] float floatSpeed = 2f;

    Vector3 startLocalPosition;

    void Awake()
    {
        startLocalPosition = transform.localPosition;
        ApplyColors();
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.localPosition = startLocalPosition + new Vector3(0f, y, 0f);
    }

    public void ApplyColors()
    {
        if (bodyRenderer != null)
            bodyRenderer.material.color = bodyColor;

        if (headRenderer != null)
            headRenderer.material.color = headColor;
    }
}