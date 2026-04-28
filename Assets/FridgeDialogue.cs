using UnityEngine;

public class FridgeDialogue : MonoBehaviour
{
    DialogueManager dialogueManager;

    string[] myLines = {
        "비어있다.",
        "배고프면 편의점에서 뭐 사면 되니까.",
        "다른 곳이나 둘러볼까...",
    };

    void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dialogueManager.StartDialogue(myLines);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dialogueManager.CloseDialogue(); // 이 부분만 변경!
        }
    }
}
