using UnityEngine;

public class ArrowSpawner: MonoBehaviour
{
    void Start()
    {
        GameObject arrow = new GameObject("Arrow");

        // Body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.transform.SetParent(arrow.transform);
        body.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);

        // Head (Sphere로 간단하게)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(arrow.transform);
        head.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        head.transform.localPosition = new Vector3(0, 1f, 0);

        arrow.transform.position = transform.position;
    }
}