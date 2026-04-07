using System.Collections;
using UnityEngine;

public class SchoolSceneArrivalEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform cameraTarget;
    [SerializeField] ScreenFader screenFader;

    [Header("Arrival Effect")]
    [SerializeField] float shakeDuration = 2.5f;
    [SerializeField] float shakeMagnitude = 15f;
    [SerializeField] float settleDelay = 0.3f;

    void Awake()
    {
        if (cameraTarget == null && Camera.main != null)
            cameraTarget = Camera.main.transform;
    }

    void Start()
    {
        StartCoroutine(PlayArrivalEffect());
    }

    IEnumerator PlayArrivalEffect()
    {
        // 씬 시작 시 검은 화면에서 시작하고 싶다면
        // ScreenFader 쪽에 즉시 검게 만드는 함수/초기 alpha 세팅이 있으면 먼저 적용

        yield return new WaitForSeconds(0.1f);

        if (cameraTarget != null)
            yield return StartCoroutine(ShakeCamera(cameraTarget, shakeDuration, shakeMagnitude));

        yield return new WaitForSeconds(settleDelay);

        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeIn(1.2f));
    }

    IEnumerator ShakeCamera(Transform target, float duration, float magnitude)
    {
        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-magnitude, magnitude);
            float y = Random.Range(-magnitude, magnitude);

            target.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalPos;
    }
}