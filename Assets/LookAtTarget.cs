using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float rotateSpeed = 8f;

    bool isLooking = false;

    public void StartLooking()
    {
        isLooking = true;
    }

    public void StopLooking()
    {
        isLooking = false;
    }

    void Update()
    {
        if (!isLooking || target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );
    }
}