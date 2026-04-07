using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    Vector3 originalLocalPosition;
    [SerializeField] PlayerController playerController;

    void Awake()
    {
        originalLocalPosition = transform.localPosition;
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        if (playerController != null)
        {
            playerController.canMove = false;
            playerController.canJump = false;
        }

        CameraController camController = FindObjectOfType<CameraController>();
        if (camController != null)
        {
            camController.enabled = false;
        }

        originalLocalPosition = transform.localPosition;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalLocalPosition + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalLocalPosition;
    }
}