using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LaptopStorySequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] DialogueManager dialogueManager;
    [SerializeField] GameObject messagePopupObject;
    [SerializeField] TextMeshProUGUI messagePopupText;
    [SerializeField] ScreenFader screenFader;
    [SerializeField] PlayerController playerController;
    [SerializeField] LaptopInteract laptopInteract;
    [SerializeField] RectTransform shakeTarget; // LaptopCanvas 또는 흔들 UI

    [Header("Scene")]
    [SerializeField] string nextSceneName = "School";

    [Header("Timing")]
    [SerializeField] float firstPopupDuration = 1.2f;
    [SerializeField] float firstDialogueDuration = 3.5f;
    [SerializeField] float typedMessageSpeed = 0.05f;
    [SerializeField] float typedMessageStayDuration = 1.2f;
    [SerializeField] float secondDialogueDuration = 3.5f;
    [SerializeField] float fadeOutDuration = 0.75f;

    [Header("Shake")]
    [SerializeField] Transform cameraTarget;
    [SerializeField] float cameraShakeDuration = 3f;
    [SerializeField] float cameraShakeMagnitude = 15f;

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
    bool isPlaying = false;

    public bool IsPlaying()
    {
        return isPlaying;
    }

    public void StartSequence()
    {
        if (isPlaying) return;
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        isPlaying = true;

        if (playerController != null)
        {
            playerController.canMove = false;
            playerController.canJump = false;
        }

        if (laptopInteract != null)
        {
            laptopInteract.SetStoryLock(true);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (messagePopupObject != null)
            messagePopupObject.SetActive(false);

        yield return new WaitForSeconds(0.4f);

        // 1) 문자 도착 알림
        yield return StartCoroutine(
            ShowPopupText(
                "[람브]에게서 문자가 도착했습니다.",
                firstPopupDuration,
                TextAlignmentOptions.Center
            )
        );

        // 2) "문자..?"
        if (dialogueManager != null)
        {
            yield return StartCoroutine(
                dialogueManager.ShowAutoDialogueInstant("문자..?", firstDialogueDuration)
            );
        }

        yield return new WaitForSeconds(1f);

        // 3) 입력되는 것처럼 텍스트
        // 중앙(위아래) + 왼쪽 정렬 = MidlineLeft
        yield return StartCoroutine(
            TypePopupText(
                "안녕, 강남대생!\n난 람브다!\n드디어 네가 졸업을 한다지?? 내가 이 학교에 있게 된 지도 벌써 80년...\n날 그렇게 귀여워해놓고 다들 이렇게 날 떠나간다니 참을 수 없다...!\n\n\n전투다 강남대생..!\n졸업하려면 날 이길 각오는 되어 있어야 할 거야. 그럼 기다리겠다!",
                typedMessageSpeed,
                TextAlignmentOptions.MidlineLeft
            )
        );

        // 암전 전까지 계속 보이게 잠깐 유지
        yield return new WaitForSeconds(typedMessageStayDuration);
        Debug.Log("shakeTarget null? " + (shakeTarget == null));
        Debug.Log("shakeTarget name: " + (shakeTarget != null ? shakeTarget.name : "null"));
        // 4) "어... 지진??" + 화면 흔들림
        if (dialogueManager != null)
        {
            StartCoroutine(
                dialogueManager.ShowAutoDialogueInstant("어... 지진??", secondDialogueDuration)
            );
        }

        // 5) 흔들림
        if (cameraTarget != null)
        {
            yield return StartCoroutine(
                ShakeCamera(cameraTarget, cameraShakeDuration, cameraShakeMagnitude)
            );

        }

        // 5) 암전
        if (screenFader != null)
        {
            yield return StartCoroutine(screenFader.FadeOut(fadeOutDuration));
        }

        // 암전 끝나고 나서 텍스트 끄기
        if (messagePopupText != null)
            messagePopupText.text = "";

        if (messagePopupObject != null)
            messagePopupObject.SetActive(false);

        // 6) 다음 씬 이동
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator ShowPopupText(string text, float duration, TextAlignmentOptions alignment)
    {
        if (messagePopupObject == null || messagePopupText == null)
            yield break;

        messagePopupObject.SetActive(true);
        messagePopupText.alignment = alignment;
        messagePopupText.text = text;

        yield return new WaitForSeconds(duration);

        messagePopupObject.SetActive(false);
    }

    IEnumerator TypePopupText(string text, float speed, TextAlignmentOptions alignment)
    {
        if (messagePopupObject == null || messagePopupText == null)
            yield break;

        messagePopupObject.SetActive(true);
        messagePopupText.alignment = alignment;
        messagePopupText.text = "";

        foreach (char c in text)
        {
            messagePopupText.text += c;
            yield return new WaitForSeconds(speed);
        }
    }

    IEnumerator ShakeUI(RectTransform target, float duration, float magnitude)
    {
        Debug.Log("ShakeUI 시작: " + target.name);

        Vector2 originalPos = target.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-magnitude, magnitude);
            float y = Random.Range(-magnitude, magnitude);

            target.anchoredPosition = originalPos + new Vector2(x, y);

            Debug.Log("현재 흔들림 위치: " + target.anchoredPosition);

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.anchoredPosition = originalPos;
        Debug.Log("ShakeUI 끝");
    }
}