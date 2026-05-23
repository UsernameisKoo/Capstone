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
    [SerializeField] RectTransform shakeTarget;

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

    [Header("Earthquake Sound")]
    [SerializeField] AudioClip earthquakeSfx;
    [SerializeField] float earthquakeVolume = 1f;

    [Header("Typing Voice Sound")]
    [SerializeField] AudioClip typingVoiceSfx;
    [SerializeField] float typingVoiceVolume = 0.8f;

    AudioSource audioSource;
    AudioSource voiceSource;

    bool isPlaying = false;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        voiceSource = gameObject.AddComponent<AudioSource>();
        voiceSource.playOnAwake = false;
    }

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

        yield return StartCoroutine(
            ShowPopupText(
                "[람브]에게서 문자가 도착했습니다.",
                firstPopupDuration,
                TextAlignmentOptions.Center
            )
        );

        if (dialogueManager != null)
        {
            yield return StartCoroutine(
                dialogueManager.ShowAutoDialogueInstant("문자..?", firstDialogueDuration)
            );
        }

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(
            TypePopupText(
                "안녕, 강남대생!\n난 람브다!\n드디어 네가 졸업을 한다지?? 내가 이 학교에 있게 된 지도 벌써 80년...\n날 그렇게 귀여워해놓고 다들 이렇게 날 떠나간다니 참을 수 없다...!\n\n\n전투다 강남대생..!\n졸업하려면 날 이길 각오는 되어 있어야 할 거야. 그럼 기다리겠다!",
                typedMessageSpeed,
                TextAlignmentOptions.MidlineLeft
            )
        );

        yield return new WaitForSeconds(typedMessageStayDuration);

        if (dialogueManager != null)
        {
            StartCoroutine(
                dialogueManager.ShowAutoDialogueInstant("어... 지진??", secondDialogueDuration)
            );
        }

        if (earthquakeSfx != null && audioSource != null)
        {
            audioSource.PlayOneShot(earthquakeSfx, earthquakeVolume);
        }

        if (cameraTarget != null)
        {
            yield return StartCoroutine(
                ShakeCamera(cameraTarget, cameraShakeDuration, cameraShakeMagnitude)
            );
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        if (screenFader != null)
        {
            yield return StartCoroutine(screenFader.FadeOut(fadeOutDuration));
        }

        if (messagePopupText != null)
            messagePopupText.text = "";

        if (messagePopupObject != null)
            messagePopupObject.SetActive(false);

        SceneManager.LoadScene(nextSceneName);
    }

    void PlayLineVoiceSound()
    {
        if (typingVoiceSfx == null || voiceSource == null)
            return;

        if (voiceSource.isPlaying)
            return;

        int mode = Random.Range(0, 5);

        voiceSource.clip = typingVoiceSfx;
        voiceSource.volume = typingVoiceVolume;
        voiceSource.time = 0f;

        switch (mode)
        {
            case 0:
                voiceSource.pitch = 1f;
                break;

            case 1:
                voiceSource.pitch = 0.75f;
                break;

            case 2:
                voiceSource.pitch = 1.35f;
                break;

            case 3:
                voiceSource.pitch = 0.55f;
                break;

            case 4:
                voiceSource.pitch = 1.6f;
                break;
        }

        voiceSource.Play();
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

        PlayLineVoiceSound();

        foreach (char c in text)
        {
            messagePopupText.text += c;

            if (c == '\n')
            {
                PlayLineVoiceSound();
            }

            yield return new WaitForSeconds(speed);
        }

        if (voiceSource != null)
        {
            voiceSource.Stop();
        }
    }

    IEnumerator ShakeUI(RectTransform target, float duration, float magnitude)
    {
        Vector2 originalPos = target.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-magnitude, magnitude);
            float y = Random.Range(-magnitude, magnitude);

            target.anchoredPosition = originalPos + new Vector2(x, y);

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.anchoredPosition = originalPos;
    }
}