using UnityEngine;

public class FridgeDialogue : MonoBehaviour
{
    DialogueManager dialogueManager;

    [SerializeField] AudioClip dialogueOpenSfx;

    AudioSource sfxSource;

    string[] myLines = {
        "비어있다.",
        "배고프면 편의점에서 뭐 사면 되니까.",
        "다른 곳이나 둘러볼까...",
    };

    void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();

        sfxSource = gameObject.AddComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 효과음 재생
            if (dialogueOpenSfx != null)
            {
                sfxSource.PlayOneShot(dialogueOpenSfx);
            }

            // 대사 시작
            dialogueManager.StartDialogue(myLines);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dialogueManager.CloseDialogue();
        }
    }
}