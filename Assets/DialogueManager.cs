using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] GameObject dialogueBox;
    [SerializeField] TextMeshProUGUI dialogueText;

    string[] lines;
    int currentLine = 0;
    bool isDialogueActive = false;

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            NextLine();
        }
    }

    public void StartDialogue(string[] dialogueLines)
    {
        lines = dialogueLines;
        currentLine = 0;
        isDialogueActive = true;
        dialogueBox.SetActive(true);
        dialogueText.text = lines[currentLine];
    }

    void NextLine()
    {
        currentLine++;
        if (currentLine < lines.Length)
        {
            dialogueText.text = lines[currentLine];
        }
        else
        {
            isDialogueActive = false;
            dialogueBox.SetActive(false);
            currentLine = 0;
        }
    }

    public void CloseDialogue()
    {
        isDialogueActive = false;
        dialogueBox.SetActive(false);
        currentLine = 0;
    }
}