using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTeleport : MonoBehaviour
{
    [SerializeField] string targetScene;
    [SerializeField] DialogueManager dialogueManager;

    [Header("Sound")]
    [SerializeField] AudioClip blockedSfx;
    [SerializeField] AudioClip doorOpenSfx;

    AudioSource sfxSource;

    string[] blockedMessage =
    {
        "뭔가 중요한 걸 두고 온 것 같다.\n집 안을 잘 살펴보자."
    };

    void Start()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        // 아직 게임을 안 깼으면 경고 메시지 표시
        if (!player.isGameCleared)
        {
            // 경고 효과음
            if (blockedSfx != null)
            {
                sfxSource.PlayOneShot(blockedSfx);
            }

            if (dialogueManager != null && !dialogueManager.IsDialogueActive())
            {
                dialogueManager.StartDialogue(blockedMessage);
            }

            return;
        }

        // 문 열리는 효과음
        if (doorOpenSfx != null)
        {
            sfxSource.PlayOneShot(doorOpenSfx);
        }

        // 씬 이동
        SceneManager.LoadScene(targetScene);
    }
}