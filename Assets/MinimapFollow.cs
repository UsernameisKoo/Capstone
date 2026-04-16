using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform target;
    public float height = 20f;

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = new Vector3(
            target.position.x,
            target.position.y + height,
            target.position.z
        );

        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}