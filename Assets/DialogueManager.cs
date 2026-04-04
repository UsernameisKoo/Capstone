using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject dialogueBox;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] GameObject nextIcon;

    [Header("Typing")]
    [SerializeField] float typingSpeed = 0.05f;

    [Header("Look")]
    [SerializeField] LookAtTarget daughterLookAt;

    string[] lines;
    int currentLine = 0;
    bool isDialogueActive = false;

    bool isTyping = false;
    string currentText;
    Coroutine typingCoroutine;

    void Update()
    {
        if (!isDialogueActive) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentText;
                isTyping = false;
                nextIcon.SetActive(true);
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue(string[] dialogueLines)
    {
        if (isDialogueActive) return;
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        lines = dialogueLines;
        currentLine = 0;
        isDialogueActive = true;

        dialogueBox.SetActive(true);

        if (daughterLookAt != null)
            daughterLookAt.StartLooking();

        ShowLine();
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    void ShowLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        currentText = lines[currentLine];
        nextIcon.SetActive(false);

        typingCoroutine = StartCoroutine(TypeText(currentText));
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        nextIcon.SetActive(true);
    }

    void NextLine()
    {
        currentLine++;

        if (currentLine < lines.Length)
        {
            ShowLine();
        }
        else
        {
            CloseDialogue();
        }
    }

    public void CloseDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (daughterLookAt != null)
            daughterLookAt.StopLooking();

        isDialogueActive = false;
        isTyping = false;
        dialogueBox.SetActive(false);
        nextIcon.SetActive(false);
        currentLine = 0;
        currentText = "";
    }
}