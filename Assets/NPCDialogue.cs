using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    DialogueManager dialogueManager;

    string[] myLines = {
        "me : mom..",
        "mom : So when are you graduating?",
        "me : sorry.."
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
            dialogueManager.gameObject.GetComponent<DialogueManager>().enabled = true;
        }
    }
}