using UnityEngine;

public class FridgeDialogue : MonoBehaviour
{
    DialogueManager dialogueManager;

    string[] myLines = {
        "me : ...empty.",
        "me : Not even leftovers.",
        "me : A college student who can't graduate...",
        "me : and now can't even eat.",
        "me : What a life."
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
